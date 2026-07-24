using Ring.Data;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class NetworkStreamExtensions
{

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

	internal static async Task<(byte Code, byte[] Body)> ReadMessageAsync(this NetworkStream stream, CancellationToken cancellationToken = default)
	{
		var header = new byte[5];
		await stream.ReadExactlyAsync(header.AsMemory(), cancellationToken).ConfigureAwait(false);

		var code = header[0];
		var bodyLength = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(1)) - 4;
		if (bodyLength < 0)
			throw new InvalidOperationException("Invalid message length received from server.");

		var body = bodyLength > 0 ? new byte[bodyLength] : Array.Empty<byte>();
		if (bodyLength > 0)
			await stream.ReadExactlyAsync(body.AsMemory(), cancellationToken).ConfigureAwait(false);

		// combined buffer for logging
		var combined = new byte[5 + body.Length];
		Array.Copy(header, 0, combined, 0, 5);
		if (body.Length > 0) Array.Copy(body, 0, combined, 5, body.Length);
		LogHex($"ServerMessage (code={(char)code})", combined);

		return (code, body);
	}

	internal static async ValueTask<(int? BackendPid, int? BackendSecret)> WaitUntilReadyAsync(this NetworkStream stream, CancellationToken cancellationToken = default)
	{
		int? pid = null;
		int? secret = null;
		while (true)
		{
			var (code, body) = await stream.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
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

	/// <summary>
	///     Synchronously reads one length-prefixed backend message:
	///     1-byte type code + Int32 length (self-inclusive) + body.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (byte code, byte[] message) ReadMessage(this NetworkStream stream, bool errorOnly)
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
		var length = BinaryPrimitives.ReadInt32BigEndian(header[1..]);
		var bodyLength = length - 4;

		if (errorOnly && code != (byte)BackendMessageCode.ErrorResponse)
		{
			SkipBody(stream, bodyLength);
			return (code, Array.Empty<byte>());
		}

		if (bodyLength <= 0)
			return (code, Array.Empty<byte>());

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ConnectionOperationalError? DrainToReadyForQuery(this NetworkStream stream, string? tableName)
	{
		// Code size: 112 (0x70)
		//eg. SQL Error[42P07]: ERREUR: la relation « @meta » existe déjà
		ConnectionOperationalError? operationalError = null;
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
				
				operationalError = body.ParseErrorResponse(tableName);

				do
					(drainCode, drainBody) = stream.ReadMessage(false); 
				while (drainCode != (byte)BackendMessageCode.ReadyForQuery);


//				_transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)'I';
//throw error;
			}

			// CommandComplete, NoticeResponse, ParameterStatus, RowDescription,
			// DataRow, etc. - nothing to do for a fire-and-forget command.
		}
	}

	#region private methods

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
	///     Throws an exception indicating the connection was closed by the server.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowConnectionWasClosedByServer() => throw new EndOfStreamException("The connection was closed by the server while reading a message.");

	#endregion
}
