using Ring.Data;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class NetworkStreamExtensions
{
	private const int InitialRowCapacityHint = 16;
	private const int SkipBodyChunkSize = 128;
	private const int SmallMessageStackAllocThreshold = 125;
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture);
	private static readonly string PostGreTrue = "t";

	// Diagnostic helper: prints a hex dump of a buffer to the console.
	private static void LogHex(string title, byte[] buffer)
	{
		var sb = new StringBuilder(buffer.Length * 3);
		for (var i = 0; i < buffer.Length; i++)
		{
			sb.Append(buffer[i].ToString("x2"));
			if (i + 1 < buffer.Length) sb.Append(' ');
		}
		Console.WriteLine($"{title}: {buffer.Length} bytes -> {sb}");
	}

	/// <summary>
	///     Async counterpart to <see cref="ReadMessage"/>. Reads one length-
	///     prefixed backend message: 1-byte type code + Int32 length (self-
	///     inclusive) + body. When <paramref name="errorOnly"/> is true and
	///     the message isn't an ErrorResponse, the body is drained without
	///     allocating a buffer sized to it - and without the hex-dump log,
	///     since there's no body to show.
	/// </summary>
	internal static async ValueTask<(byte Code, byte[] Body)> ReadMessageAsync(this NetworkStream stream, bool errorOnly, CancellationToken cancellationToken = default)
	{
		// Code size: 71 (0x47)
		var header = new byte[5];
		await stream.ReadExactlyAsync(header.AsMemory(), cancellationToken).ConfigureAwait(false);

		var code = header[0];
		var bodyLength = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(1)) - 4;
		if (bodyLength < 0) ThrowInvalidMessageLength();

		if (errorOnly && code != (byte)BackendMessageCode.ErrorResponse)
		{
			await stream.SkipBodyAsync(bodyLength, cancellationToken).ConfigureAwait(false);
			return (code, Array.Empty<byte>());
		}

		var body = bodyLength > 0 ? new byte[bodyLength] : Array.Empty<byte>();
		if (bodyLength > 0) await stream.ReadExactlyAsync(body.AsMemory(), cancellationToken).ConfigureAwait(false);

		// Combined buffer for logging only - never leaves this method (LogHex
		// doesn't retain the reference), so unlike header/body above it's safe
		// to pool. Rent can hand back an array larger than requested, so pass
		// LogHex an explicit slice rather than the whole rented array.
		var combinedLength = 5 + body.Length;
		var combined = ArrayPool<byte>.Shared.Rent(combinedLength);
		try
		{
			Array.Copy(header, 0, combined, 0, 5);
			if (body.Length > 0) Array.Copy(body, 0, combined, 5, body.Length);
			//LogHex($"ServerMessage (code={(char)code})", combined.AsSpan(0, combinedLength));
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(combined);
		}

		return (code, body);
	}

	/// <summary>
	///     Synchronously reads one length-prefixed backend message:
	///     1-byte type code + Int32 length (self-inclusive) + body. When
	///     <paramref name="errorOnly"/> is true and the message isn't an
	///     ErrorResponse, the body is skipped without allocating a buffer
	///     sized to it. Async counterpart: <see cref="ReadMessageAsync"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (byte Code, byte[] Body) ReadMessage(this NetworkStream stream, bool errorOnly)
	{
		// Code size: 187 (0xbb)
		const int headerSize = 5;
		Span<byte> header = stackalloc byte[headerSize];
		var read = 0;

		while (read < headerSize)
		{
			var n = stream.Read(header[read..]);
			if (n == 0) ThrowConnectionWasClosedByServer();
			read += n;
		}

		var code = header[0];
		var bodyLength = BinaryPrimitives.ReadInt32BigEndian(header[1..]) - 4;
		if (bodyLength < 0) ThrowInvalidMessageLength();

		if (errorOnly && code != (byte)BackendMessageCode.ErrorResponse)
		{
			SkipBody(stream, bodyLength);
			return (code, Array.Empty<byte>());
		}

		if (bodyLength == 0) return (code, Array.Empty<byte>());

		var body = new byte[bodyLength]; // heap allocation, only for messages we actually decode
		var bodySpan = new Span<byte>(body);

		read = 0;
		while (read < bodyLength)
		{
			var n = stream.Read(bodySpan[read..]);
			if (n == 0) ThrowConnectionWasClosedByServer();
			read += n;
		}
		return (code, body);
	}

	internal static async ValueTask SendSASLResponseAsync(this NetworkStream stream, byte[] data, CancellationToken cancellationToken = default)
	{
		var length = 4 + data.Length; // 4 bytes for the length field + data
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);
		data.CopyTo(buffer.AsSpan(5));
		LogHex("SASLResponse (client)", buffer);
		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	internal static async ValueTask SendPasswordMessageAsync(this NetworkStream stream, string password, CancellationToken cancellationToken = default)
	{
		// Code size: 71 (0x47)
		var length = 4 + Encoding.UTF8.GetByteCount(password) + 1;
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);
		var written = Encoding.UTF8.GetBytes(password, buffer.AsSpan(5));
		buffer[5 + written] = 0;
		LogHex("PasswordMessage (client)", buffer);
		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	internal static async ValueTask SendSASLInitialResponseAsync(this NetworkStream stream, string mechanism, byte[] data, CancellationToken cancellationToken = default)
	{
		// Code size: 79 (0x4f) - no virtual calls
		var mechanismLength = Encoding.UTF8.GetByteCount(mechanism);
		var outerLength = 4 + mechanismLength + 1 + 4 + data.Length;
		var buffer = new byte[1 + outerLength];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), outerLength);

		var offset = 5;
		offset += Encoding.UTF8.GetBytes(mechanism, buffer.AsSpan(offset));
		buffer[offset++] = 0;
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset), data.Length);
		offset += 4;
		data.CopyTo(buffer.AsSpan(offset));
		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Send the StartupMessage: Int32 protocol version followed by
	///     null-terminated name/value pairs, terminated by a zero byte.
	///     Unlike every other frontend message, this one has no leading
	///     type byte.
	/// </summary>
	internal static async ValueTask SendStartupAsync(this NetworkStream stream, ConnectionParameters connParameters, CancellationToken cancellationToken = default)
	{
		//TODO manage multiple protocol versions, for now we only support 3.0
		//TODO manage SSL negotiation, for now we only support non-SSL connections
		//TODO manage multiple encodings, for now we only support UTF-8
		const int ProtocolVersion3 = 0x00030000;

		var parameters = new List<(string Name, string Value)>(3)
		{
			(connParameters.GetParameterName(ConnectionParametersType.UserName), connParameters.UserName),
			(connParameters.GetParameterName(ConnectionParametersType.ClientEncoding), connParameters.ClientEncoding),
		};
		if (!string.IsNullOrEmpty(connParameters.DatabaseName))
			parameters.Add((connParameters.GetParameterName(ConnectionParametersType.DataBase), connParameters.DatabaseName));
		if (!string.IsNullOrEmpty(connParameters.ApplicationName))
			parameters.Add((connParameters.GetParameterName(ConnectionParametersType.ApplicationName), connParameters.ApplicationName));

		var length = 4 + 4 + 1; // length field + protocol version + trailing terminator
		foreach (var (name, value) in parameters) length += Encoding.UTF8.GetByteCount(name) + 1 + Encoding.UTF8.GetByteCount(value) + 1;

		var buffer = new byte[length];

		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(), length);
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan()[4..], ProtocolVersion3);

		var offset = 8;
		foreach (var (name, value) in parameters)
		{
			offset += Encoding.UTF8.GetBytes(name, buffer.AsSpan()[offset..]);
			buffer[offset++] = 0;
			offset += Encoding.UTF8.GetBytes(value, buffer.AsSpan()[offset..]);
			buffer[offset++] = 0;
		}
		buffer[offset] = 0; // trailing terminator

		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	// NetworkStream's constructor requires a genuinely connected Stream-type
	// socket (it throws IOException "not connected on non-connected sockets"
	// otherwise), so a sentinel can't be built from a bare, never-connected
	// Socket. Instead this spins up a throwaway TCP loopback pair, wraps one
	// end in a NetworkStream, then disposes everything immediately. What's
	// left is a real, fully-disposed NetworkStream: never connected to
	// anything meaningful, and any accidental read/write on it now throws
	// ObjectDisposedException rather than the misleading "not connected" error.
	internal static NetworkStream CreateClosedStream(this NetworkStream? _)
	{
		// Code size: 124 (0x7c)
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		try
		{
			using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
			using var server = listener.AcceptSocket();
			using var stream = new NetworkStream(client, ownsSocket: true);
			return stream;
		}
		finally
		{
			listener.Stop();
		}
	}

	internal static string?[] ReadRetrieveRecords(this NetworkStream stream, ref byte transactionStatus, Encoding encoding, Table table, int rowCount = -1)
		=> rowCount > 0
			? ReadRetrieveRecordsExact(stream, ref transactionStatus, encoding, table, rowCount)
			: ReadRetrieveRecordsPooled(stream, ref transactionStatus, encoding, table);
				
	/// <summary>
	///     Reads backend messages until ReadyForQuery, updating
	///     <see cref="_transactionStatus"/> from its status byte
	///     ('I'/'T'/'E'). Used for commands where the row payload (if any)
	///     is irrelevant - BEGIN/COMMIT/ROLLBACK always reply with just
	///     CommandComplete, but this also tolerates other message types
	///     defensively.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static OperationalError? DrainToReadyForQuery(this NetworkStream stream, ref byte transactionStatus)
	{
		// Code size: 112 (0x70)
		//eg. SQL Error[42P07]: ERREUR: la relation « @meta » existe déjà
		OperationalError? operationalError = null;
		while (true)
		{
			var (code, body) = stream.ReadMessage(true);

			if (code == (byte)BackendMessageCode.ReadyForQuery)	return operationalError;
			if (code == (byte)BackendMessageCode.ErrorResponse)
			{
				//var error = AuthenticationHelper.ParseErrorResponse(body);
				// Drain remaining messages so the connection is left in a
				// clean, known state (ReadyForQuery) before surfacing the error.
				byte drainCode;
				byte[] drainBody;
				
				operationalError = body.ParseErrorFields();

				do
					(drainCode, drainBody) = stream.ReadMessage(false); 
				while (drainCode != (byte)BackendMessageCode.ReadyForQuery);

				transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)'I';
				return operationalError;
			}
		}
	}

	/// <summary>
	///     Async counterpart to <see cref="DrainToReadyForQuery"/>. Drains
	///     messages until ReadyForQuery, allocating for nothing but an
	///     ErrorResponse body - same as the sync version.
	///
	///     <c>ref</c> parameters aren't legal on async methods, so the
	///     transaction-status byte can't be handed back the way the sync
	///     version does it. Instead this returns the raw ReadyForQuery body:
	///     empty on the clean, no-error path (its body is skipped, exactly
	///     like the sync fast path leaves <c>transactionStatus</c> untouched),
	///     or the drained ReadyForQuery body after an error. Callers derive
	///     the status the same way the sync caller already does:
	///     <c>drainBody.Length > 0 ? drainBody[0] : (byte)'I'</c>.
	/// </summary>
	internal static async ValueTask<(OperationalError? Error, byte[] DrainBody)> DrainToReadyForQueryAsync(this NetworkStream stream, CancellationToken cancellationToken = default)
	{
		//eg. SQL Error[42P07]: ERREUR: la relation « @meta » existe déjà
		OperationalError? operationalError = null;
		while (true)
		{
			var (code, body) = await stream.ReadMessageAsync(true, cancellationToken).ConfigureAwait(false);

			if (code == (byte)BackendMessageCode.ReadyForQuery) return (operationalError, Array.Empty<byte>());
			if (code == (byte)BackendMessageCode.ErrorResponse)
			{
				byte drainCode;
				byte[] drainBody;
				operationalError = body.ParseErrorFields();

				do (drainCode, drainBody) = await stream.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
				while (drainCode != (byte)BackendMessageCode.ReadyForQuery);

				return (operationalError, drainBody);
			}
		}
	}

	/// <summary>
	///     Sends the Extended Query subprotocol for a parameterized statement:
	///     Parse ('P') + Bind ('B') + Execute ('E') + Sync ('S'), pipelined
	///     into a single buffer and a single Write so the round-trip cost
	///     matches the Simple Query path in <see cref="SendQuery"/>.
	///
	///     No Describe message is sent - callers already know the shape of
	///     the result set from the client-side <see cref="Table"/>/<see
	///     cref="Column"/> metadata (see ReadRetrieveRecordsPooled), so
	///     RowDescription/NoData/ParameterDescription are never expected
	///     back; the read path already tolerates them defensively for
	///     exactly this reason.
	///
	///     Both statement and portal are unnamed (""), so this is a one-shot
	///     parameterized query: the server discards them at the next unnamed
	///     Parse/Bind, and no explicit Close message is needed. If you later
	///     want prepared-statement reuse across calls, that needs a named
	///     statement and an explicit Close - out of scope here.
	///
	///     All parameter and result format codes are text (0), matching how
	///     every read path in this file expects text-format wire data.
	///     Parameter type OIDs are left unspecified (numParamTypes = 0), so
	///     Postgres infers parameter types from query context - this is fine
	///     for ordinary INSERT/UPDATE/WHERE-clause parameters bound against a
	///     known column, but a query with no inferable context for a given
	///     $n (e.g. a bare `SELECT $1`) would need an explicit cast in the
	///     SQL text itself (`SELECT $1::text`), since there's no client-side
	///     FieldType-to-OID map here to fall back on.
	///
	///     <paramref name="values"/> and <paramref name="columns"/> must be
	///     the same length and in $1.. order - one Column per bind parameter,
	///     not the full table schema (unlike AppendRecordData, this does not
	///     skip SearchableColumn/TimeZoneColumn entries; filter those out
	///     before calling if your columns array can include them).
	///
	///     Depends on FrontendMessageCode having Parse ('P'), Bind ('B'),
	///     Execute ('E') and Sync ('S') members alongside the existing Query.
	/// </summary>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static void SendExtendedQuery(this NetworkStream stream, ReadOnlySpan<char> sql, int sqlByteCount, Encoding encoding, byte[] sqlSendBuffer, string?[] values, Column[] columns)
	{
		// values.Length && columns.Length should be equal!
		// Pass 1: figure out the wire byte length of each parameter value up
		// front, so the total message length (and therefore which buffer
		// tier to use below) is known before anything is written.
		// ByteArray columns hold Base64 text client-side (see
		// AppendRecordData/ParseByteaHexToBase64) but go over the wire as
		// Postgres bytea hex text ('\x' + 2 hex chars/byte), so those get
		// decoded once here and the raw bytes kept for the write pass below,
		// rather than decoding twice.
		var paramLengths = ArrayPool<int>.Shared.Rent(values.Length);
		byte[]?[]? byteaBytes = null; // lazily allocated only if a ByteArray parameter is present
		try
		{
			for (var i = 0; i < values.Length; i++)
			{
				var value = values[i];
				if (value is null) { paramLengths[i] = -1; continue; }

				if (columns[i].FieldType == FieldType.ByteArray)
				{
					byteaBytes ??= new byte[values.Length][];
					var raw = Convert.FromBase64String(value);
					byteaBytes[i] = raw;
					paramLengths[i] = 2 + raw.Length * 2; // '\x' + 2 hex chars per byte
				}
				else
				{
					// Postgres's boolean/date/etc. text-input parsers accept the same
					// invariant-culture strings Record already stores fields as (e.g.
					// "True"/"False" is a valid case-insensitive boolean literal), so
					// every non-bytea FieldType is passed through verbatim as text.
					paramLengths[i] = encoding.GetByteCount(value);
				}
			}

			const int emptyCStringLength = 1; // just the NUL terminator, for the unnamed statement/portal
			const int executeLength = 4 + emptyCStringLength + 4; // length field + unnamed portal + maxRows
			const int syncLength = 4; // length field only, no body

			var parseLength = 4 + emptyCStringLength + sqlByteCount + 1 + 2; // length field + stmt NUL + query + query NUL + numParamTypes(0)

			var bindParamsLength = 0;
			for (var i = 0; i < values.Length; i++)	bindParamsLength += 4 + (paramLengths[i] > 0 ? paramLengths[i] : 0);
			var bindLength = 4 + emptyCStringLength + emptyCStringLength + 2 + 2 + 2 + bindParamsLength + 2 + 2;

			var totalLength = 1 + parseLength + 1 + bindLength + 1 + executeLength + 1 + syncLength;

			if (totalLength <= SmallMessageStackAllocThreshold)
			{
				Span<byte> buffer = stackalloc byte[totalLength];
				WriteExtendedQueryMessages(buffer, sql, sqlByteCount, encoding, values, columns, paramLengths, byteaBytes, parseLength, bindLength);
				stream.Write(buffer);
			}
			else if (totalLength <= sqlSendBuffer.Length)
			{
				var buf = sqlSendBuffer.AsSpan(0, totalLength);
				WriteExtendedQueryMessages(buf, sql, sqlByteCount, encoding, values, columns, paramLengths, byteaBytes, parseLength, bindLength);
				stream.Write(buf);
			}
			else
			{
				var rented = ArrayPool<byte>.Shared.Rent(totalLength);
				try
				{
					var buf = rented.AsSpan(0, totalLength);
					WriteExtendedQueryMessages(buf, sql, sqlByteCount, encoding, values, columns, paramLengths, byteaBytes, parseLength, bindLength);
					stream.Write(buf);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rented);
				}
			}
			stream.Flush();
		}
		finally
		{
			ArrayPool<int>.Shared.Return(paramLengths);
		}
	}


	/// <summary>
	///     Sends a frontend Simple Query ('Q') message: Int32 length (self-inclusive) followed by the null-terminated query string. 
	///     The server always replies using the text wire format for this message type, regardless of column type, which is what makes flattening every
	///     value straight to string safe here.
	/// </summary>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static void SendQuery(this NetworkStream stream, ReadOnlySpan<char> sql, int sqlByteCount, Encoding encoding, byte[] sqlSendBuffer)
	{
		// Code size: 307 (0x133) - no allocations
		// Exact byte count either way - see GetSqlByteCount.
		// type + length + encoding.GetByteCount(sql) + null terminator
		var messageLength = 1 + 4 + sqlByteCount + 1;

		if (messageLength <= SmallMessageStackAllocThreshold)
		{
			// ── Case 1: very short query, small enough to fit on the stack.
			// stackalloc is bounded by a constant known at JIT time, so the stack frame is never unbounded. SkipLocalsInit means we don't zero the whole buffer before filling it,
			// saving ~15 ns on a 120-byte alloc. Every byte is explicitly written before use.
			Span<byte> buffer = stackalloc byte[messageLength];
			buffer[0] = (byte)FrontendMessageCode.Query;
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1, 4), messageLength - 1);
			encoding.GetBytes(sql, buffer[5..^1]);
			buffer[^1] = 0; // null terminator
			stream.Write(buffer);
		}
		else if (messageLength <= sqlSendBuffer.Length)
		{
			// ── Case 2: per-connection preallocated heap buffer
			// One allocation at connection open time; reused for every query that fits. Always write the type byte - it is NOT guaranteed to
			// be present from a previous call (bug fix: previous version only set it in the constructor).
			var buf = sqlSendBuffer.AsSpan(0, messageLength);
			buf[0] = (byte)FrontendMessageCode.Query;
			BinaryPrimitives.WriteInt32BigEndian(buf.Slice(1, 4), messageLength - 1);
			encoding.GetBytes(sql, buf.Slice(5, sqlByteCount));
			buf[messageLength - 1] = 0; // NUL terminator
			stream.Write(buf);
		}
		else
		{
			// ── Case 3: ArrayPool rent for oversized queries
			// Pooled instead of `new byte[]` to avoid LOH fragmentation on queries large enough to escape the preallocated buffer (rare in practice, but important for correctness when they do occur).
			// ArrayPool.Rent may return a larger array than requested; pass an explicit slice to Write so we never send trailing garbage bytes.
			var rented = ArrayPool<byte>.Shared.Rent(messageLength);
			try
			{
				rented[0] = (byte)FrontendMessageCode.Query;
				BinaryPrimitives.WriteInt32BigEndian(rented.AsSpan(1, 4), messageLength - 1);
				encoding.GetBytes(sql, rented.AsSpan(5, sqlByteCount));
				rented[messageLength - 1] = 0; // NUL terminator
				stream.Write(rented.AsSpan(0, messageLength));
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
		stream.Flush();
	}

	internal static async ValueTask<(int? BackendPid, int? BackendSecret)> WaitUntilReadyAsync(this NetworkStream stream, CancellationToken cancellationToken = default)
	{
		int? pid = null;
		int? secret = null;
		while (true)
		{
			var (code, body) = await stream.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
			switch ((BackendMessageCode)code)
			{
				case BackendMessageCode.BackendKeyData:
					if (body.Length >= 8)
					{
						pid = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(0, 4));
						secret = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(4, 4));
					}
					continue;
				case BackendMessageCode.ParameterStatus:
				case BackendMessageCode.NoticeResponse:
					continue;
				case BackendMessageCode.ReadyForQuery:
					return (pid, secret);
				case BackendMessageCode.ErrorResponse:
					//throw ParseErrorResponse(body);
					break;
				default:
					break;
			}
		}
	}

	#region private methods

	// Writes Parse+Bind+Execute+Sync into destination, which must be exactly
	// (1 + parseLength) + (1 + bindLength) + (1 + executeLength) + (1 + syncLength)
	// bytes long - the three call sites in SendExtendedQuery size it that way
	// regardless of which buffer tier they picked, so this method itself
	// doesn't care whether destination came from the stack, the per-connection
	// buffer, or ArrayPool.
	private static void WriteExtendedQueryMessages(Span<byte> destination, ReadOnlySpan<char> sql, int sqlByteCount, Encoding encoding, string?[] values, Column[] columns, ReadOnlySpan<int> paramLengths, byte[]?[]? byteaBytes, int parseLength, int bindLength)
	{
		var offset = 0;

		// ---- Parse ----
		destination[offset++] = (byte)FrontendMessageCode.Parse;
		BinaryPrimitives.WriteInt32BigEndian(destination.Slice(offset, 4), parseLength);
		offset += 4;
		destination[offset++] = 0; // unnamed statement
		offset += encoding.GetBytes(sql, destination.Slice(offset, sqlByteCount));
		destination[offset++] = 0; // query NUL terminator
		BinaryPrimitives.WriteInt16BigEndian(destination.Slice(offset, 2), 0); // numParamTypes = 0, server infers types
		offset += 2;

		// ---- Bind ----
		destination[offset++] = (byte)FrontendMessageCode.Bind;
		BinaryPrimitives.WriteInt32BigEndian(destination.Slice(offset, 4), bindLength);
		offset += 4;
		destination[offset++] = 0; // unnamed portal
		destination[offset++] = 0; // unnamed statement - must match the Parse above
		BinaryPrimitives.WriteInt16BigEndian(destination.Slice(offset, 2), 1); // one format code, applies to all params
		offset += 2;
		BinaryPrimitives.WriteInt16BigEndian(destination.Slice(offset, 2), 0); // text
		offset += 2;
		BinaryPrimitives.WriteInt16BigEndian(destination.Slice(offset, 2), (short)values.Length);
		offset += 2;

		for (var i = 0; i < values.Length; i++)
		{
			var len = paramLengths[i];
			BinaryPrimitives.WriteInt32BigEndian(destination.Slice(offset, 4), len);
			offset += 4;
			if (len <= 0) continue; // NULL (-1) or empty string (0): no value bytes follow either way

			if (columns[i].FieldType == FieldType.ByteArray)
				WriteByteaHex(byteaBytes![i]!, destination.Slice(offset, len));
			else
				encoding.GetBytes(values[i]!, destination.Slice(offset, len));
			offset += len;
		}

		BinaryPrimitives.WriteInt16BigEndian(destination.Slice(offset, 2), 1); // one result format code, applies to all columns
		offset += 2;
		BinaryPrimitives.WriteInt16BigEndian(destination.Slice(offset, 2), 0); // text - matches every read path in this file
		offset += 2;

		// ---- Execute ----
		destination[offset++] = (byte)FrontendMessageCode.Execute;
		BinaryPrimitives.WriteInt32BigEndian(destination.Slice(offset, 4), 9); // length field + unnamed portal + maxRows
		offset += 4;
		destination[offset++] = 0; // unnamed portal
		BinaryPrimitives.WriteInt32BigEndian(destination.Slice(offset, 4), 0); // maxRows = 0: no limit
		offset += 4;

		// ---- Sync ----
		destination[offset++] = (byte)FrontendMessageCode.Sync;
		BinaryPrimitives.WriteInt32BigEndian(destination.Slice(offset, 4), 4);
	}

	// Mirror image of the read-side ParseByteaHexToBase64: Record stores
	// byte[] fields as Base64 text, so a ByteArray bind parameter needs
	// decoding from Base64 (done once in SendExtendedQuery's pass 1) and
	// re-encoding to Postgres's bytea hex text format here, not a plain
	// UTF8 GetBytes of the original Base64 string.
	private static void WriteByteaHex(ReadOnlySpan<byte> raw, Span<byte> destination)
	{
		destination[0] = (byte)'\\';
		destination[1] = (byte)'x';
		var offset = 2;
		foreach (var b in raw)
		{
			destination[offset++] = HexNibbles[b >> 4];
			destination[offset++] = HexNibbles[b & 0x0F];
		}
	}

	private static readonly byte[] HexNibbles =
	{
		(byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
		(byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f'
	};

	// Consumes and discards bodyLength bytes from the socket without allocating
	// a buffer sized to the message - needed so ReadMessage(errorOnly: true)
	// stays byte-aligned with the stream for messages it doesn't care about.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SkipBody(this NetworkStream stream, int bodyLength)
	{
		// Code size: 82 (0x52)
		if (bodyLength <= 0) return;
		Span<byte> scratch = stackalloc byte[Math.Min(bodyLength, 128)];
		var remaining = bodyLength;
		while (remaining > 0)
		{
			var chunk = scratch[..Math.Min(remaining, scratch.Length)];
			var n = stream.Read(chunk);
			if (n == 0) ThrowConnectionWasClosedByServer();
			remaining -= n;
		}
	}

	/// <summary>
	///     <paramref name="rowCount"/> is trusted, not just a hint: verified
	///     against what actually arrives (throws on mismatch instead of
	///     silently returning a wrong-shaped result - see
	///     <see cref="AppendDataRowExact"/>), but never grown or copied.
	///     One allocation total.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string?[] ReadRetrieveRecordsExact(NetworkStream stream, ref byte transactionStatus, Encoding encoding, Table table, int rowCount)
	{
		// Code size: 331 (0x14b)
		var results = new string?[table.RecordSize * rowCount];
		var count = 0;

		while (true)
		{
			var (code, body) = stream.ReadMessage(false);

			switch (code)
			{
				case (byte)BackendMessageCode.DataRow:
					AppendRecordData(body, encoding, null, table, ref results, count);
					count+= table.RecordSize; // items per row, including tracker slot
					break;
				case (byte)BackendMessageCode.ReadyForQuery:
					transactionStatus = body.Length > 0 ? body[0] : (byte)TransactionStatus.Idle;
					if (count != results.Length)
						throw new InvalidOperationException(
							$"ReadRetrieveRecords was told to expect {rowCount} rows ({results.Length} values, including tracker slots) but received {count} values before ReadyForQuery.");
					return results;
				case (byte)BackendMessageCode.ErrorResponse:
					{
						//var error = AuthenticationHelper.ParseErrorResponse(body);
						// Drain remaining messages so the connection is left in a
						// clean, known state (ReadyForQuery) before surfacing the error.
						byte drainCode;
						byte[] drainBody;
						do
						{
							(drainCode, drainBody) = stream.ReadMessage(false);
						} while (drainCode != (byte)BackendMessageCode.ReadyForQuery);
						transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)TransactionStatus.Idle;
						//throw error;
					}
					break;
				// Informational / already-implied-by-DataRow messages; nothing to do.
				// ParseComplete/BindComplete/NoData/ParameterDescription only ever
				// appear on the Extended Query path (SendExtendedQuery), since we
				// don't send a Describe message that would otherwise trigger one.
				case (byte)BackendMessageCode.RowDescription:
				case (byte)BackendMessageCode.CommandComplete:
				case (byte)BackendMessageCode.EmptyQueryResponse:
				case (byte)BackendMessageCode.NoticeResponse:
				case (byte)BackendMessageCode.ParameterStatus:
				case (byte)BackendMessageCode.NotificationResponse:
				case (byte)BackendMessageCode.ParseComplete:
				case (byte)BackendMessageCode.BindComplete:
				case (byte)BackendMessageCode.NoData:
				case (byte)BackendMessageCode.ParameterDescription:
					break;
				default:
					UnexpectedProviderMessage(code);
					break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string?[] ReadRetrieveRecordsPooled(NetworkStream stream, ref byte transactionStatus, Encoding encoding, Table table)
	{
		// Code size: 298 (0x12a)
		var pool = ArrayPool<string?>.Shared;
		var initialCapacity = table.RecordSize * InitialRowCapacityHint;
		var buffer = pool.Rent(initialCapacity);
		var count = 0;

		try
		{
			while (true)
			{
				var (code, body) = stream.ReadMessage(false);
				switch (code)
				{
					case (byte)BackendMessageCode.DataRow:
						AppendRecordData(body, encoding, pool, table, ref buffer, count);
						count += table.RecordSize; // items per row, including tracker slot
						break;
					case (byte)BackendMessageCode.ReadyForQuery:
						{
							transactionStatus = body.Length > 0 ? body[0] : (byte)TransactionStatus.Idle;
							var results = new string?[count];
							Array.Copy(buffer, results, count);
							return results;
						}
					case (byte)BackendMessageCode.ErrorResponse:
						{
							//var error = AuthenticationHelper.ParseErrorResponse(body);
							// Drain remaining messages so the connection is left in a
							// clean, known state (ReadyForQuery) before surfacing the error.
							byte drainCode;
							byte[] drainBody;
							do
							{
								(drainCode, drainBody) = stream.ReadMessage(false);
							} while (drainCode != (byte)BackendMessageCode.ReadyForQuery);
							transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)TransactionStatus.Idle;
							//throw error;
						}
						break;
					// Informational / already-implied-by-DataRow messages; nothing to do.
					// ParseComplete/BindComplete/NoData/ParameterDescription only ever
					// appear on the Extended Query path (SendExtendedQuery), since we
					// don't send a Describe message that would otherwise trigger one.
					case (byte)BackendMessageCode.RowDescription:
					case (byte)BackendMessageCode.CommandComplete:
					case (byte)BackendMessageCode.EmptyQueryResponse:
					case (byte)BackendMessageCode.NoticeResponse:
					case (byte)BackendMessageCode.ParameterStatus:
					case (byte)BackendMessageCode.NotificationResponse:
					case (byte)BackendMessageCode.ParseComplete:
					case (byte)BackendMessageCode.BindComplete:
					case (byte)BackendMessageCode.NoData:
					case (byte)BackendMessageCode.ParameterDescription:
						break;
					default:
						UnexpectedProviderMessage(code);
						break;
				}
			}
		}
		finally
		{
			pool.Return(buffer, clearArray: true);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AppendRecordData(byte[] body, Encoding encoding, ArrayPool<string?>? pool, Table table, ref string?[] buffer, int count)
	{
		// Code size: 281 (0x119) - no virtual calls, no allocations except for the buffer if it needs to grow
		var offset = 0;
		var required = count + table.RecordSize;
		var columns = new ReadOnlySpan<Column>(table.Columns);
		offset += 2;

		// EnsureCapacity
		if (required > buffer.Length && pool is not null) EnsureCapacity(pool, ref buffer, count, required);

		foreach (var column in columns)
		{
			if (column.Type == EntityType.SearchableColumn) continue;
			var valueLength = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(offset));
			var index = column.RecordIndex + count;

			offset += 4;
			if (valueLength < 0)
			{
				buffer[index] = null; // SQL NULL - no value bytes follow, offset stays put
				continue;
			}

			if (column.Type == EntityType.TimeZoneColumn)
			{
				// TODO: timezone-specific handling goes here (e.g. combine with
				// the paired DateTimeOffset column). Until then, still consume
				// the value bytes so offset stays aligned with the next column's
				// length prefix - skipping this was desyncing every column that
				// follows a TimeZoneColumn for the rest of the row.
				offset += valueLength;
				continue;
			}

			if (column.FieldType == FieldType.ByteArray)
			{
				// Postgres text-mode bytea arrives hex-encoded ('\x' + 2 hex
				// chars/byte); Record.GetField(out byte[]?) expects Base64, so
				// re-encode here rather than storing Postgres' own wire format.
				buffer[index] = body.ParseByteaHexToBase64(offset, valueLength);
			}
			else
			{
				buffer[index] = encoding.GetString(body, offset, valueLength);
				if (column.FieldType == FieldType.Boolean)
					buffer[index] = string.Equals(PostGreTrue, buffer[index], StringComparison.OrdinalIgnoreCase) ? BooleanTrue : BooleanFalse;
			}
			offset += valueLength;
		}
		buffer[count + table.RecordSize - 1] = null; // Record's dirty-tracker slot: a freshly loaded row is never dirty
	}

	private static void EnsureCapacity(ArrayPool<string?> pool, ref string?[] buffer, int usedCount, int required)
	{
		// Code size: 41 (0x29)
		var grown = pool.Rent(Math.Max(buffer.Length * 2, required));
		Array.Copy(buffer, grown, usedCount);
		pool.Return(buffer, clearArray: true); // clearArray: true to avoid leaking references to the pooled array!
		buffer = grown;
	}

	// Async counterpart to SkipBody. Reads bodyLength bytes from the socket
	// without allocating a buffer sized to the message - stackalloc can't
	// cross an await, so this rents a small scratch buffer instead.
	private static async ValueTask SkipBodyAsync(this NetworkStream stream, int bodyLength, CancellationToken cancellationToken)
	{
		if (bodyLength <= 0) return;
		var scratch = ArrayPool<byte>.Shared.Rent(SkipBodyChunkSize);
		try
		{
			var remaining = bodyLength;
			while (remaining > 0)
			{
				var n = await stream.ReadAsync(scratch.AsMemory(0, Math.Min(remaining, SkipBodyChunkSize)), cancellationToken).ConfigureAwait(false);
				if (n == 0) ThrowConnectionWasClosedByServer();
				remaining -= n;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(scratch);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowConnectionWasClosedByServer() => // Code size: 17 (0x11)
		throw new EndOfStreamException(ResourceHelper.GetMessage(ResourceType.ConnectionClosedByServer));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void UnexpectedProviderMessage(byte code) => // Code size: 33 (0x21)
		throw new InvalidOperationException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.UnexpectedProviderMessage), (char)code));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidMessageLength() => // Code size: 17 (0x11)
		throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.InvalidMessageLengthFromServer));
	
	#endregion
}
