using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Helpers;
using Ring.Schema;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL;

public sealed class Connection : IConnection
{
	private static readonly NetworkStream ClosedStream = CreateClosedStream();

	// Simple Query protocol message type bytes. Not all of these exist on
	private const byte MsgSimpleQuery = (byte)'Q';
	private const byte MsgRowDescription = (byte)'T';
	private const byte MsgDataRow = (byte)'D';
	private const byte MsgCommandComplete = (byte)'C';
	private const byte MsgEmptyQueryResponse = (byte)'I';
	private const byte MsgReadyForQuery = (byte)'Z';
	private const byte MsgErrorResponse = (byte)'E';
	private const byte MsgNoticeResponse = (byte)'N';
	private const byte MsgParameterStatus = (byte)'S';
	private const byte MsgNotificationResponse = (byte)'A';

	// Extended Query protocol (Parse/Bind/Execute/Sync) message type bytes.
	private const byte MsgParseComplete = (byte)'1';
	private const byte MsgBindComplete = (byte)'2';
	private const byte MsgNoData = (byte)'n';
	private const byte MsgParameterDescription = (byte)'t';

	// Transaction status as last reported by the server's ReadyForQuery
	// message: 'I' = idle (no transaction), 'T' = in transaction block,
	// 'E' = in a failed transaction block. Starts 'I' since a freshly
	// opened connection has no transaction in progress.
	private byte _transactionStatus = (byte)'I';

	private readonly long _id;
	private readonly DateTime _creationTime;
	private readonly DateTime? _lastConnectionTime;
	private readonly string _host;
	private readonly int _port;
	private readonly ConnectionParameters _parameters;
	private ConnectionState _state;

	// tcp connection
	private readonly int _timeout; // milliseconds
	private readonly Encoding _encoding;
	private readonly int _sqlSendBufferSize;
	private readonly byte[] _sqlSendBuffer;

	// Never null: defaults to ClosedStream so every code path that forgot to
	// check _state first hits a well-defined (if slightly odd) stream state
	// instead of a NullReferenceException. Every real call site already
	// checks _state before touching _stream, so this sentinel is never
	// intentionally read from or written to - it exists purely so the field
	// itself is never null. Dispose sites must compare against ClosedStream
	// by reference before disposing (see e.g. DisposeStreamAsync) since it's
	// one instance shared by every Connection.
	private NetworkStream _stream = ClosedStream;

	// Kept alongside _stream purely so IsConnectionAlive() can poll it -
	// NetworkStream owns and disposes this Socket (ownsSocket: true in Open()),
	// this field is never disposed independently.
	private Socket? _socket;
	private int _backendPid;
	private int _backendSecret;

	public long Id => _id;
	public DateTime CreationTime => _creationTime;
	public DateTime? LastConnectionTime => _lastConnectionTime;
	public Encoding ClientEncoding => _encoding;
	public ConnectionState State => _state;

	// build ConnectionParameters from connection string
	public Connection(string connectionString) : this(connectionString.ToConnectionParameters()) {}
	
	internal Connection(ConnectionParameters parameters)
	{
		_parameters = parameters;
		_id = this.GetId(parameters.GetHashCode());
		_creationTime = DateTime.Now;
		_state = ConnectionState.Undefined;
		_lastConnectionTime = null;
		_timeout = parameters.TimeOut;
		_host = parameters.Host;
		_port = parameters.Port;
		_encoding = Encoding.GetEncoding(parameters.ClientEncoding);
		_sqlSendBufferSize = parameters.SqlSendBufferSize;
		if (_sqlSendBufferSize > 0)
		{
			_sqlSendBuffer = new byte[_sqlSendBufferSize];
			_sqlSendBuffer[0] = MsgSimpleQuery;
		}
		else _sqlSendBuffer = Array.Empty<byte>();
	}


	public void BeginTransaction()
	{
		if (_state != ConnectionState.Open)
			throw new InvalidOperationException("The connection is not open.");
		if (_transactionStatus != (byte)'I')
			throw new InvalidOperationException("A transaction is already in progress.");

		try
		{
			SendSimpleCommand("BEGIN");
		}
		catch (PgOperationalError)
		{
			// Server-side error; the connection itself is still usable.
			throw;
		}
		catch
		{
			// I/O failure or protocol desync leaves the stream in an
			// unknown state - don't let the connection be reused as-is.
			_state = ConnectionState.Broken;
			throw;
		}
	}

	public bool IsConnectionAlive()
	{
		// Code size: 67 (0x43)
		if (_state != ConnectionState.Open || _socket is null) return false;
		try
		{
			var readable = _socket.Poll(0, SelectMode.SelectRead);
			return !(readable && _socket.Available == 0);
		}
		catch (SocketException) { return false; }
		catch (ObjectDisposedException) { return false; }
	}

	public void Close()
	{
		// If not open, nothing to do
		if (_state != ConnectionState.Open && _state != ConnectionState.Connecting)
		{
			_state = ConnectionState.Closed;
			_stream.Dispose();
			_stream = ClosedStream;
			_socket = null;
			_backendPid = 0;
			_backendSecret = 0;
			return;
		}

		try
		{
			// Send Terminate message: type 'X', length = 4
			if (_stream.CanWrite)
			{
				var buffer = new byte[1 + 4];
				buffer[0] = (byte)'X';
				BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), 4);
				try
				{
					_stream.Write(buffer);
					_stream.Flush();
				}
				catch
				{
					// Ignore write failures during close; proceed to dispose
				}
			}

			_stream.Dispose();
			_stream = ClosedStream;
			_socket = null;
			_backendPid = 0;
			_backendSecret = 0;
			_state = ConnectionState.Closed;
		}
		catch
		{
			// If disposing failed, mark connection as broken
			try { _stream.Dispose(); } catch { }
			_stream = ClosedStream;
			_socket = null;
			_state = ConnectionState.Broken;
			_backendPid = 0;
			_backendSecret = 0;
			throw;
		}
	}

	public Task OpenAsync(CancellationToken cancellationToken) => OpenAsyncImpl(cancellationToken);
	public Task CloseAsync(CancellationToken cancellationToken = default) => CloseAsyncImpl(cancellationToken);

	/// <summary>
	///     Sends a Simple Query "COMMIT" and waits for it to complete.
	///     Refuses up front if the driver's tracked transaction status shows
	///     no transaction is active - the wire protocol alone can't
	///     distinguish "committed successfully" from "COMMIT with nothing to
	///     commit" (Postgres treats the latter as a harmless no-op with just
	///     a NoticeResponse), so this check is what actually catches the
	///     caller's mistake. If the transaction is in the failed/aborted
	///     state ('E'), COMMIT is still sent through: Postgres implicitly
	///     rolls back an aborted transaction on COMMIT rather than erroring.
	/// </summary>
	public void Commit()
	{
		if (_state != ConnectionState.Open)
			throw new InvalidOperationException("The connection is not open.");
		if (_transactionStatus == (byte)'I')
			throw new InvalidOperationException("Commit() was called but no transaction is currently active.");

		try
		{
			SendSimpleCommand("COMMIT");
		}
		catch (PgOperationalError)
		{
			// Server-side error; the connection itself is still usable.
			throw;
		}
		catch
		{
			// I/O failure or protocol desync leaves the stream in an
			// unknown state - don't let the connection be reused as-is.
			_state = ConnectionState.Broken;
			throw;
		}
	}

	public IConnection CreateInstance(int id, int sqlSendBufferSize) => new Connection(_parameters.Set(id, sqlSendBufferSize));
	
	public void Dispose()
	{
		throw new NotImplementedException();
	}

	public string?[] Execute()
	{
		//Table table, RetrieveQueryType type, IDqlBuilder builder, int parentQueryId
		//var retrieveQuery = new RetrieveQuery("SELECT * FROM pg_catalog.pg_tables;");
		var retrieveQuery = new RetrieveQuery();
		return Execute(retrieveQuery);
	}

	public string?[] Execute(in RetrieveQuery query)
	{
		if (_state != ConnectionState.Open)
			throw new InvalidOperationException("The connection is not open.");

		// Filters/sorting/paging need RetrieveFilter/RetrieveSort/PageInfo -> SQL
		// translation that isn't wired up yet. Fail loudly instead of silently
		// returning an unfiltered result set.
		if (query.Filters.Count > 0)
			throw new NotSupportedException("Execute(RetrieveQuery) does not yet support filters.");
		/*
	if (query.Sorts.HasValue)
		throw new NotSupportedException("Execute(RetrieveQuery) does not yet support sorting.");
	if (query.Page.HasValue)
		throw new NotSupportedException("Execute(RetrieveQuery) does not yet support paging.");
		*/
		//var sql = query.Builder.SelectFrom(query.Table);

		try
		{
			SendQuery("SELECT * FROM pg_catalog.pg_tables;", _encoding.GetByteCount("SELECT * FROM pg_catalog.pg_tables;"));
			return ReadRetrieveResults();
		}
		catch (PgOperationalError)
		{
			// Server-side error (bad SQL, constraint violation, etc.); the
			// connection itself is still usable for subsequent commands.
			throw;
		}
		catch
		{
			// Anything else (I/O failure, protocol desync) leaves the stream
			// in an unknown state - don't let the connection be reused as-is.
			_state = ConnectionState.Broken;
			throw;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void Execute(in AlterQuery query, ReadOnlySpan<char> sql, int sqlByteCount) 
	{ 
		SendQuery(sql, sqlByteCount);
		DrainToReadyForQuery();
	}
	public ValueTask ExecuteAsync(in AlterQuery query, ReadOnlySpan<char> sql, int sqlByteCount, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public long Execute(in SaveQuery query)
	{
		throw new NotImplementedException();
	}


	public void Open()
	{
		if (_state == ConnectionState.Open)
			throw new InvalidOperationException("The connection is already open.");

		// Delegate to the async implementation and block. The sync auth path
		// (AuthenticationHelper.HandleAuthentication) was a non-functional stub
		// that re-sent the StartupMessage instead of reading and responding to
		// the server's AuthenticationRequest, which caused the server to reject
		// the connection. Reusing the async implementation avoids maintaining
		// two copies of the wire-protocol logic.
		OpenAsyncImpl(CancellationToken.None).GetAwaiter().GetResult();
	}

	public int ProviderId() => (int)_parameters.DatabaseProvider;

	public void Rollback()
	{
		if (_state != ConnectionState.Open)
			throw new InvalidOperationException("The connection is not open.");
		if (_transactionStatus == (byte)'I')
			throw new InvalidOperationException("Rollback() was called but no transaction is currently active.");

		try
		{
			SendSimpleCommand("ROLLBACK");
		}
		catch (PgOperationalError)
		{
			// Server-side error; the connection itself is still usable.
			throw;
		}
		catch
		{
			// I/O failure or protocol desync leaves the stream in an
			// unknown state - don't let the connection be reused as-is.
			_state = ConnectionState.Broken;
			throw;
		}
	}

	#region private methods

	private async ValueTask DisposeStreamAsync()
	{
		var stream = _stream;
		_stream = ClosedStream;
		_socket = null;

		if (!ReferenceEquals(stream, ClosedStream))
			await stream.DisposeAsync().ConfigureAwait(false);
	}


	/// <summary>
	///     Sends a Simple Query for a command that produces no result rows
	///     (BEGIN/COMMIT/ROLLBACK) and drains the response through
	///     ReadyForQuery, updating <see cref="_transactionStatus"/> from the
	///     status byte it carries.
	/// </summary>
	private void SendSimpleCommand(string sql)
	{
		SendQuery(sql.AsSpan(), _encoding.GetByteCount(sql));
		DrainToReadyForQuery();
	}


	/// <summary>
	///     DDL-specific variant of <see cref="DrainToReadyForQuery"/>, used
	///     only by <see cref="Execute(in AlterQuery, ReadOnlySpan{char}, int)"/>.
	///     On ErrorResponse, reads only the SQLSTATE ('C' field) as a slice of
	///     the existing buffer via <see cref="AuthenticationHelper.ReadSqlStateBytes"/>
	///     - no string allocation - to check it against 42P07 (duplicate_table).
	///     A CREATE TABLE/INDEX/etc. against something that already exists is
	///     treated as a harmless no-op rather than thrown, since the desired
	///     end state already holds; the full field parse (and its allocations)
	///     only happens for codes other than 42P07, right before throwing.
	///     BEGIN/COMMIT/ROLLBACK go through <see cref="DrainToReadyForQuery"/>
	///     instead and always throw on any error - this relaxed handling is
	///     specific to DDL.
	/// </summary>
	private void DrainDdlToReadyForQuery()
	{
		while (true)
		{
			var (code, body) = ReadMessage(_stream);

			if (code == MsgReadyForQuery)
			{
				_transactionStatus = body.Length > 0 ? body[0] : (byte)'I';
				return;
			}

			if (code == MsgErrorResponse)
			{
				var isDuplicateTable = body.ReadSqlStateBytes();
				var error = isDuplicateTable.Length>1 ? null : AuthenticationHelper.ParseErrorResponse(body);

				// Drain remaining messages so the connection is left in a
				// clean, known state (ReadyForQuery) before returning/throwing.
				byte drainCode;
				byte[] drainBody;
				do
				{
					(drainCode, drainBody) = ReadMessage(_stream);
				} while (drainCode != MsgReadyForQuery);
				_transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)'I';

				if (error != null) throw error;
				return;
			}

			// CommandComplete, NoticeResponse, ParameterStatus, RowDescription,
			// DataRow, etc. - nothing to do for a fire-and-forget command.
		}
	}



	/// <summary>
	///     Reads backend messages until ReadyForQuery, updating
	///     <see cref="_transactionStatus"/> from its status byte
	///     ('I'/'T'/'E'). Used for commands where the row payload (if any)
	///     is irrelevant - BEGIN/COMMIT/ROLLBACK always reply with just
	///     CommandComplete, but this also tolerates other message types
	///     defensively.
	/// </summary>
	private void DrainToReadyForQuery()
	{
		//eg. SQL Error[42P07]: ERREUR: la relation « @meta » existe déjà
		while (true)
		{
			var (code, body) = ReadMessage(_stream);

			if (code == MsgReadyForQuery)
			{
				_transactionStatus = body.Length > 0 ? body[0] : (byte)'I';
				return;
			}

			if (code == MsgErrorResponse)
			{
				var error = AuthenticationHelper.ParseErrorResponse(body);
				// Drain remaining messages so the connection is left in a
				// clean, known state (ReadyForQuery) before surfacing the error.
				byte drainCode;
				byte[] drainBody;
				do
				{
					(drainCode, drainBody) = ReadMessage(_stream);
				} while (drainCode != MsgReadyForQuery);
				_transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)'I';
				throw error;
			}

			// CommandComplete, NoticeResponse, ParameterStatus, RowDescription,
			// DataRow, etc. - nothing to do for a fire-and-forget command.
		}
	}

	/// <summary>
	///     Sends a frontend Simple Query ('Q') message: Int32 length (self-
	///     inclusive) followed by the null-terminated query string. The server
	///     always replies using the text wire format for this message type,
	///     regardless of column type, which is what makes flattening every
	///     value straight to string safe here.
	/// </summary>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private void SendQuery(ReadOnlySpan<char> sql, int sqlByteCount)
	{
		// Code size: 325 (0x145)
		// Exact byte count either way - see GetSqlByteCount.
		var encoding = _encoding;
		var stream = _stream;

		// type + length + encoding.GetByteCount(sql) + null terminator
		var messageLength = 1 + 4 + sqlByteCount + 1;

		if (messageLength <= 128)
		{
			// case 1: very short query, small enough to fit on the stack.
			Span<byte> buffer = stackalloc byte[messageLength];
			buffer[0] = MsgSimpleQuery;
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1, 4), messageLength - 1);
			encoding.GetBytes(sql, buffer[5..^1]);
			buffer[^1] = 0; // null terminator
			stream.Write(buffer);
		}
		else if (messageLength <= _sqlSendBufferSize)
		{
			// case 2: query fits in the preallocated send buffer.
			var buffer = _sqlSendBuffer.AsSpan(0, messageLength);
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1, 4), messageLength - 1);
			encoding.GetBytes(sql, buffer[5..^1]);
			buffer[^1] = 0; // null terminator
			stream.Write(buffer);
		}
		else
		{
			// case 3: query is too large for the preallocated send buffer.
			var buffer = new byte[messageLength];
			buffer[0] = MsgSimpleQuery;
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1, 4), messageLength - 1);
			encoding.GetBytes(sql, buffer.AsSpan(5..^1));
			stream.Write(buffer);
		}
		stream.Flush();
	}

	/// <summary>
	///     Sends one round of the Extended Query protocol for a parameterized
	///     statement: Parse (unnamed statement) + Bind (unnamed portal) +
	///     Execute (unlimited rows) + Sync, all flushed together in a single
	///     write. No Describe message is sent - it would only add a
	///     RowDescription/NoData reply that ReadRetrieveResults already
	///     ignores, since column values are parsed generically regardless of
	///     their declared type.
	///     All parameter values are sent in text format (format code 0),
	///     which lets Postgres apply its normal implicit-cast rules the same
	///     way it would for literals typed directly into SQL - no client-side
	///     type-OID mapping required. The tradeoff is that binary-sensitive
	///     types (e.g. large bytea payloads) pay a text-encoding cost; that
	///     can be revisited later by sending format code 1 for specific
	///     parameters once Ring has typed binary encoders.
	///     Sized and written the same way as SendQuery: exact byte counts up
	///     front, then one contiguous buffer (stack/pooled/heap, same three
	///     tiers) written directly - no per-message helper, no intermediate
	///     MemoryStream or per-message byte[].
	/// </summary>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private void SendExtendedQuery(string sql, ReadOnlySpan<string?> parameters)
	{
		var encoding = _encoding;
		var stream = _stream;

		var sqlByteCount = encoding.GetByteCount(sql);

		// Byte length per value, cached so each string is only ever encoded
		// once below - same reasoning as SendQuery's own buffer sizing.
		Span<int> paramLengths = parameters.Length <= 32
			? stackalloc int[parameters.Length]
			: new int[parameters.Length];
		var paramBytesTotal = 0;
		for (var i = 0; i < parameters.Length; i++)
		{
			var paramLength = parameters[i] is { } value ? encoding.GetByteCount(value) : -1;
			paramLengths[i] = paramLength;
			paramBytesTotal += Math.Max(paramLength, 0);
		}

		// Parse ('P'): Int32 length + empty stmt name + sql + null term + Int16 param-type count (0)
		var parseLength = 4 + 1 + sqlByteCount + 1 + 2;
		// Bind ('B'): Int32 length + empty portal + empty stmt name + Int16 fmt-code count (0)
		//             + Int16 param count + per-param (Int32 length + bytes) + Int16 result-fmt count (0)
		var bindLength = 4 + 1 + 1 + 2 + 2 + parameters.Length * 4 + paramBytesTotal + 2;
		// Execute ('E'): Int32 length + empty portal + Int32 max-rows (0)
		const int executeLength = 4 + 1 + 4;
		// Sync ('S'): Int32 length only
		const int syncLength = 4;

		var totalLength = 1 + parseLength + 1 + bindLength + 1 + executeLength + 1 + syncLength;

		if (totalLength <= 128)
		{
			// case 1: short statement/params, small enough to fit on the stack.
			Span<byte> buffer = stackalloc byte[totalLength];

			buffer[0] = (byte)'P';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1, 4), parseLength);
			var offset = 5;
			buffer[offset++] = 0; // empty (unnamed) statement name
			encoding.GetBytes(sql, buffer.Slice(offset, sqlByteCount));
			offset += sqlByteCount;
			buffer[offset++] = 0; // null terminator for query text
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), 0); // no parameter type OIDs supplied
			offset += 2;

			buffer[offset] = (byte)'B';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset + 1, 4), bindLength);
			offset += 5;
			buffer[offset++] = 0; // empty (unnamed) portal name
			buffer[offset++] = 0; // empty (unnamed) statement name
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), 0); // 0 parameter format codes = all text
			offset += 2;
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), (short)parameters.Length);
			offset += 2;
			for (var i = 0; i < parameters.Length; i++)
			{
				var value = parameters[i];
				var valueLength = paramLengths[i];
				BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset, 4), valueLength); // -1 for SQL NULL
				offset += 4;
				if (value is null)
					continue;
				encoding.GetBytes(value, buffer.Slice(offset, valueLength));
				offset += valueLength;
			}
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), 0); // 0 result format codes = all text
			offset += 2;

			buffer[offset] = (byte)'E';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset + 1, 4), executeLength);
			offset += 5;
			buffer[offset++] = 0; // empty (unnamed) portal name
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset, 4), 0); // 0 = no row limit
			offset += 4;

			buffer[offset] = (byte)'S';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset + 1, 4), syncLength);

			stream.Write(buffer);
		}
		else if (totalLength <= _sqlSendBufferSize)
		{
			// case 2: fits in the preallocated send buffer (shared with SendQuery -
			// only one command is ever in flight per connection at a time).
			var buffer = _sqlSendBuffer.AsSpan(0, totalLength);

			buffer[0] = (byte)'P';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(1, 4), parseLength);
			var offset = 5;
			buffer[offset++] = 0;
			encoding.GetBytes(sql, buffer.Slice(offset, sqlByteCount));
			offset += sqlByteCount;
			buffer[offset++] = 0;
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), 0);
			offset += 2;

			buffer[offset] = (byte)'B';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset + 1, 4), bindLength);
			offset += 5;
			buffer[offset++] = 0;
			buffer[offset++] = 0;
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), 0);
			offset += 2;
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), (short)parameters.Length);
			offset += 2;
			for (var i = 0; i < parameters.Length; i++)
			{
				var value = parameters[i];
				var valueLength = paramLengths[i];
				BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset, 4), valueLength);
				offset += 4;
				if (value is null)
					continue;
				encoding.GetBytes(value, buffer.Slice(offset, valueLength));
				offset += valueLength;
			}
			BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(offset, 2), 0);
			offset += 2;

			buffer[offset] = (byte)'E';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset + 1, 4), executeLength);
			offset += 5;
			buffer[offset++] = 0;
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset, 4), 0);
			offset += 4;

			buffer[offset] = (byte)'S';
			BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(offset + 1, 4), syncLength);

			stream.Write(buffer);
		}
		else
		{
			// case 3: statement/params too large for the preallocated send buffer.
			var buffer = new byte[totalLength];

			buffer[0] = (byte)'P';
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1, 4), parseLength);
			var offset = 5;
			buffer[offset++] = 0;
			encoding.GetBytes(sql, buffer.AsSpan(offset, sqlByteCount));
			offset += sqlByteCount;
			buffer[offset++] = 0;
			BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset, 2), 0);
			offset += 2;

			buffer[offset] = (byte)'B';
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset + 1, 4), bindLength);
			offset += 5;
			buffer[offset++] = 0;
			buffer[offset++] = 0;
			BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset, 2), 0);
			offset += 2;
			BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset, 2), (short)parameters.Length);
			offset += 2;
			for (var i = 0; i < parameters.Length; i++)
			{
				var value = parameters[i];
				var valueLength = paramLengths[i];
				BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset, 4), valueLength);
				offset += 4;
				if (value is null)
					continue;
				encoding.GetBytes(value, buffer.AsSpan(offset, valueLength));
				offset += valueLength;
			}
			BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset, 2), 0);
			offset += 2;

			buffer[offset] = (byte)'E';
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset + 1, 4), executeLength);
			offset += 5;
			buffer[offset++] = 0;
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset, 4), 0);
			offset += 4;

			buffer[offset] = (byte)'S';
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset + 1, 4), syncLength);

			stream.Write(buffer);
		}
		stream.Flush();
	}

	/// <summary>
	///     Reads backend messages until ReadyForQuery, flattening every
	///     DataRow's column values (row-major) into a single string?[].
	///     SQL NULL becomes a null array element. Assumes a single-statement
	///     query; a multi-statement Simple Query would produce more than one
	///     RowDescription/CommandComplete pair, whose rows would simply be
	///     concatenated here.
	/// </summary>
	private string?[] ReadRetrieveResults()
	{
		var results = new List<string?>();

		while (true)
		{
			var (code, body) = ReadMessage(_stream);

			if (code == MsgDataRow)
			{
				AppendDataRow(body, results);
			}
			else if (code == MsgReadyForQuery)
			{
				_transactionStatus = body.Length > 0 ? body[0] : (byte)'I';
				return results.ToArray();
			}
			else if (code == MsgErrorResponse)
			{
				var error = AuthenticationHelper.ParseErrorResponse(body);
				// Drain remaining messages so the connection is left in a
				// clean, known state (ReadyForQuery) before surfacing the error.
				byte drainCode;
				byte[] drainBody;
				do
				{
					(drainCode, drainBody) = ReadMessage(_stream);
				} while (drainCode != MsgReadyForQuery);
				_transactionStatus = drainBody.Length > 0 ? drainBody[0] : (byte)'I';
				throw error;
			}
			else if (code == MsgRowDescription
				|| code == MsgCommandComplete
				|| code == MsgEmptyQueryResponse
				|| code == MsgNoticeResponse
				|| code == MsgParameterStatus
				|| code == MsgNotificationResponse
				|| code == MsgParseComplete
				|| code == MsgBindComplete
				|| code == MsgNoData
				|| code == MsgParameterDescription)
			{
				// Informational / already-implied-by-DataRow messages; nothing to do.
				// ParseComplete/BindComplete/NoData/ParameterDescription only ever
				// appear on the Extended Query path (SendExtendedQuery), since we
				// don't send a Describe message that would otherwise trigger one.
			}
			else
			{
				throw new InvalidOperationException($"Unexpected message '{(char)code}' received while executing a query.");
			}
		}
	}

	private Task OpenAsyncImpl(CancellationToken cancellationToken)
	{
		return Task.Run(async () =>
		{
			if (_state == ConnectionState.Open) throw new InvalidOperationException("The connection is already open.");

			_state = ConnectionState.Connecting;
			try
			{
				// Run the synchronous connect on the threadpool to avoid blocking the caller
				var socket = await Task.Run(() => SocketHelper.ConnectSocket(_host, _port, _timeout), cancellationToken).ConfigureAwait(false);
				socket.NoDelay = true;

				_stream = new NetworkStream(socket, ownsSocket: true);
				_socket = socket;

				// Startup and authentication may perform network I/O; run on threadpool to avoid blocking
				await _stream.SendStartupAsync(_parameters, cancellationToken).ConfigureAwait(false);

				var (pid, secret) = await Task.Run(() => AuthenticationHelper.HandleAuthenticationAsync(_stream, _parameters.UserName, _parameters.Password), cancellationToken).ConfigureAwait(false);
				_backendPid = pid ?? 0;
				_backendSecret = secret ?? 0;
				_state = ConnectionState.Open;
			}
			catch (OperationCanceledException)
			{
				_stream.Dispose(); // also disposes underlying socket
				_stream = ClosedStream;
				_socket = null;
				_state = ConnectionState.Undefined;
				throw;
			}
			catch
			{
				_stream.Dispose(); // also disposes underlying socket
				_stream = ClosedStream;
				_socket = null;
				_state = ConnectionState.Undefined;
				throw;
			}
		}, cancellationToken);
	}

	private async Task CloseAsyncImpl(CancellationToken cancellationToken)
	{
		// If not open, nothing to do beyond releasing any lingering resources
		if (_state != ConnectionState.Open && _state != ConnectionState.Connecting)
		{
			_state = ConnectionState.Closed;
			await DisposeStreamAsync().ConfigureAwait(false);
			_backendPid = 0;
			_backendSecret = 0;
			return;
		}

		var canceled = false;

		if (_stream.CanWrite)
		{
			var buffer = new byte[1 + 4];
			buffer[0] = (byte)'X';
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), 4);
			try
			{
				await _stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Don't bail out here: we still want to release the socket below.
				// Re-thrown once cleanup has completed.
				canceled = true;
			}
			catch
			{
				// Ignore write failures during close (server may already be gone); proceed to dispose.
			}
		}

		try
		{
			await DisposeStreamAsync().ConfigureAwait(false);
		}
		catch
		{
			// If disposing failed, mark connection as broken
			_stream = ClosedStream;
			_socket = null;
			_state = ConnectionState.Broken;
			_backendPid = 0;
			_backendSecret = 0;
			throw;
		}

		_backendPid = 0;
		_backendSecret = 0;
		_state = ConnectionState.Closed;

		if (canceled) throw new OperationCanceledException(cancellationToken);
	}


	/// <summary>
	///     Parses a DataRow message body: Int16 column count, then per column
	///     an Int32 length (-1 = SQL NULL) followed by that many bytes of
	///     UTF8 text, and appends each column's value to <paramref name="results"/>.
	/// </summary>
	private static void AppendDataRow(byte[] body, List<string?> results)
	{
		var offset = 0;
		var columnCount = BinaryPrimitives.ReadInt16BigEndian(body.AsSpan(offset));
		offset += 2;

		for (var i = 0; i < columnCount; i++)
		{
			var valueLength = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(offset));
			offset += 4;

			if (valueLength < 0)
			{
				results.Add(null); // SQL NULL
				continue;
			}

			results.Add(Encoding.UTF8.GetString(body, offset, valueLength));
			offset += valueLength;
		}
	}

	/// <summary>
	///     Synchronously reads one length-prefixed backend message:
	///     1-byte type code + Int32 length (self-inclusive) + body.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (byte Code, byte[] Body) ReadMessage(Stream stream)
	{
		// Code size: 160 (0xa0)
		const int headerSize = 5;
		Span<byte> header = stackalloc byte[headerSize];
		var read = 0;

		// ReadFully(stream, header);
		while (read < headerSize)
		{
			var n = stream.Read(header[read..]);
			if (n == 0) ThrowConnectionWasClosedByServer();
			read += n;
		}

		var code = header[0];
		var length = BinaryPrimitives.ReadInt32BigEndian(header[1..]);
		var bodyLength = length - 4;

		if (bodyLength <= 0)
			return (code, Array.Empty<byte>());

		var body = new byte[bodyLength]; // heap allocation
		var bodySpan = new Span<byte>(body);

		// ReadFully(stream, body);
		read = 0;
		while (read < bodyLength)
		{
			var n = stream.Read(bodySpan[read..]);
			if (n == 0) ThrowConnectionWasClosedByServer();
			read += n;
		}
		return (code, body);
	}

	// NetworkStream's constructor requires a genuinely connected Stream-type
	// socket (it throws IOException "not connected on non-connected sockets"
	// otherwise), so a sentinel can't be built from a bare, never-connected
	// Socket. Instead this spins up a throwaway TCP loopback pair, wraps one
	// end in a NetworkStream, then disposes everything immediately. What's
	// left is a real, fully-disposed NetworkStream: never connected to
	// anything meaningful, and any accidental read/write on it now throws
	// ObjectDisposedException rather than the misleading "not connected" error.
	private static NetworkStream CreateClosedStream()
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

	// exceptions 
	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowConnectionWasClosedByServer() => throw new EndOfStreamException("The connection was closed by the server while reading a message.");

		
	#endregion

}