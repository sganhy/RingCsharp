using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Helpers;
using System.Buffers.Binary;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace Ring.PostgreSQL;

public sealed class Connection : IConnection
{
	// Simple Query protocol message type bytes. Not all of these exist on
	// BackendMessageCode (that enum currently only covers the authentication subset), so these are declared locally rather than guessed onto the enum.
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

	// interface 
	private readonly long _id;
	private readonly string _connectionString;
	private readonly DateTime _creationTime;
	private readonly DateTime? _lastConnectionTime;
	private readonly string _host;
	private readonly int _port;
	private readonly ConnectionParameters _initialParameters;

	private ConnectionState _state;

	// Transaction status as last reported by the server's ReadyForQuery
	// message: 'I' = idle (no transaction), 'T' = in transaction block,
	// 'E' = in a failed transaction block. Starts 'I' since a freshly
	// opened connection has no transaction in progress.
	private byte _transactionStatus = (byte)'I';

	// tcp connection
	private readonly int _timeout; // milliseconds

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

	private static readonly NetworkStream ClosedStream = CreateClosedStream();


	public long Id => _id;
	public string ConnectionString => _connectionString;
	public DateTime CreationTime => _creationTime;
	public DateTime? LastConnectionTime => _lastConnectionTime;
	public ConnectionState State => _state;
	public Connection(string connectionString) : this(connectionString, connectionString.ToConnectionParameters()) { }
	internal Connection(string connectionString, ConnectionParameters parameters)
	{
		_connectionString = connectionString;
		_initialParameters = parameters;
		_id = this.GetId(connectionString);
		_creationTime = DateTime.Now;
		_state = ConnectionState.Undefined;
		_lastConnectionTime = null;
		_timeout = parameters.TimeOut;
		_host = parameters.Host;
		_port = parameters.Port;
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

	public IConnection CreateInstance(int id)
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		throw new NotImplementedException();
	}

#if DEBUG
	public string?[] Execute()
	{
		//Table table, RetrieveQueryType type, IDqlBuilder builder, int parentQueryId
		//var retrieveQuery = new RetrieveQuery("SELECT * FROM pg_catalog.pg_tables;");
		var retrieveQuery = new RetrieveQuery();
		return Execute(retrieveQuery);
	}
#endif



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
			SendQuery("SELECT * FROM pg_catalog.pg_tables;");
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

	/// <summary>
	///     Executes <paramref name="sql"/> using the Extended Query protocol
	///     (Parse/Bind/Execute/Sync) with bind variables, instead of the
	///     Simple Query protocol used by <see cref="Execute(in RetrieveQuery)"/>.
	///     Placeholders in the SQL text are referenced positionally as
	///     $1, $2, ... matching the order of <paramref name="parameters"/>.
	///     Unlike Simple Query, parameter values are never interpolated into
	///     the SQL text, so this is the safe way to pass user-supplied values.
	/// </summary>
	public string?[] Execute(string sql, params object?[] parameters)
	{
		if (_state != ConnectionState.Open)
			throw new InvalidOperationException("The connection is not open.");

		try
		{
			SendExtendedQuery(sql, parameters);
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

	public long Execute(in AlterQuery query)
	{
		throw new NotImplementedException();
	}

	public long Execute(in SaveQuery query)
	{
		throw new NotImplementedException();
	}

	public ValueTask<int> ExecuteAsync(in AlterQuery query, CancellationToken cancellationToken = default)
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

	public int ProviderId() => (int)_initialParameters.DatabaseProvider;

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
		SendQuery(sql);
		DrainToReadyForQuery();
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
	private void SendQuery(string sql)
	{
		var sqlBytes = Encoding.UTF8.GetBytes(sql);
		var length = 4 + sqlBytes.Length + 1; // length field + query text + null terminator
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'Q';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);
		sqlBytes.CopyTo(buffer.AsSpan(5));
		// buffer[^1] is already 0 (null terminator) - byte[] is zero-initialized

		_stream.Write(buffer);
		_stream.Flush();
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
	/// </summary>
	private void SendExtendedQuery(string sql, object?[] parameters)
	{
		using var ms = new MemoryStream();
		WriteParseMessage(ms, sql, parameters.Length);
		WriteBindMessage(ms, parameters);
		WriteExecuteMessage(ms);
		WriteSyncMessage(ms);

		var bytes = ms.ToArray();
		_stream.Write(bytes);
		_stream.Flush();
	}

	/// <summary>
	///     Parse (F): statement name (empty = unnamed), query text, then the
	///     parameter type OIDs. We always send zero OIDs regardless of
	///     <paramref name="paramCount"/> - Postgres will infer each
	///     parameter's type from context (e.g. from the column it's compared
	///     against), which is sufficient since values are sent as text below.
	/// </summary>
	private static void WriteParseMessage(Stream target, string sql, int paramCount)
	{
		var sqlBytes = Encoding.UTF8.GetBytes(sql);
		var body = 1 + sqlBytes.Length + 1 + 2; // empty stmt name + query + null term + Int16 param-type count (0)
		var length = 4 + body;

		var buffer = new byte[1 + length];
		buffer[0] = (byte)'P';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);

		var offset = 5;
		buffer[offset++] = 0; // empty (unnamed) statement name
		sqlBytes.CopyTo(buffer.AsSpan(offset));
		offset += sqlBytes.Length;
		buffer[offset++] = 0; // null terminator for query text
		BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset), 0); // no parameter type OIDs supplied

		target.Write(buffer);
	}

	/// <summary>
	///     Bind (F): binds parameter values to the unnamed portal/statement.
	///     Zero parameter-format codes and zero result-format codes both mean
	///     "text format for all", per the protocol's shorthand.
	/// </summary>
	private static void WriteBindMessage(Stream target, object?[] parameters)
	{
		var encoded = new byte[parameters.Length][];
		for (var i = 0; i < parameters.Length; i++)
			encoded[i] = FormatParameterValue(parameters[i]);

		var body = 1 + 1 + 2 + 2; // empty portal + empty stmt name + 0 format codes + param count
		foreach (var value in encoded)
			body += 4 + (value?.Length ?? 0); // Int32 length (-1 for NULL) + bytes
		body += 2; // 0 result-format codes

		var length = 4 + body;
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'B';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);

		var offset = 5;
		buffer[offset++] = 0; // empty (unnamed) portal name
		buffer[offset++] = 0; // empty (unnamed) statement name
		BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset), 0); // 0 parameter format codes = all text
		offset += 2;
		BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset), (short)encoded.Length);
		offset += 2;
		foreach (var value in encoded)
		{
			if (value is null)
			{
				BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset), -1); // SQL NULL
				offset += 4;
				continue;
			}
			BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset), value.Length);
			offset += 4;
			value.CopyTo(buffer.AsSpan(offset));
			offset += value.Length;
		}
		BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset), 0); // 0 result format codes = all text

		target.Write(buffer);
	}

	/// <summary>Execute (F): run the unnamed portal, with no row-count limit.</summary>
	private static void WriteExecuteMessage(Stream target)
	{
		const int length = 4 + 1 + 4; // length field + empty portal name + Int32 max-rows
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'E';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);
		buffer[5] = 0; // empty (unnamed) portal name
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(6), 0); // 0 = no row limit
		target.Write(buffer);
	}

	/// <summary>Sync (F): closes out the Extended Query round, returning ReadyForQuery.</summary>
	private static void WriteSyncMessage(Stream target)
	{
		Span<byte> buffer = stackalloc byte[5];
		buffer[0] = (byte)'S';
		BinaryPrimitives.WriteInt32BigEndian(buffer[1..], 4);
		target.Write(buffer);
	}

	/// <summary>
	///     Converts a bind-parameter value to Postgres' text wire format
	///     (the same textual representation you'd use as a SQL literal, minus
	///     quoting). Returns null for a SQL NULL, which callers encode as
	///     length -1.
	/// </summary>
	private static byte[]? FormatParameterValue(object? value)
	{
		return value switch
		{
			null => null,
			string s => Encoding.UTF8.GetBytes(s),
			bool b => Encoding.UTF8.GetBytes(b ? "t" : "f"),
			byte[] bytes => Encoding.UTF8.GetBytes("\\x" + Convert.ToHexString(bytes).ToLowerInvariant()),
			DateTime dt => Encoding.UTF8.GetBytes(dt.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture)),
			DateTimeOffset dto => Encoding.UTF8.GetBytes(dto.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz", CultureInfo.InvariantCulture)),
			Guid g => Encoding.UTF8.GetBytes(g.ToString()),
			IFormattable f => Encoding.UTF8.GetBytes(f.ToString(null, CultureInfo.InvariantCulture)),
			_ => Encoding.UTF8.GetBytes(value.ToString() ?? string.Empty),
		};
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
				SendStartup();
				var (pid, secret) = await Task.Run(() => AuthenticationHelper.HandleAuthenticationAsync(_stream, _initialParameters.UserName, _initialParameters.Password), cancellationToken).ConfigureAwait(false);
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

		if (canceled)
			throw new OperationCanceledException(cancellationToken);
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
	private static (byte Code, byte[] Body) ReadMessage(Stream stream)
	{
		Span<byte> header = stackalloc byte[5];
		ReadFully(stream, header);

		var code = header[0];
		var length = BinaryPrimitives.ReadInt32BigEndian(header[1..]);
		var bodyLength = length - 4;

		if (bodyLength <= 0)
			return (code, Array.Empty<byte>());

		var body = new byte[bodyLength];
		ReadFully(stream, body);
		return (code, body);
	}

	private static void ReadFully(Stream stream, Span<byte> buffer)
	{
		var read = 0;
		while (read < buffer.Length)
		{
			var n = stream.Read(buffer[read..]);
			if (n == 0)
				throw new EndOfStreamException("The connection was closed by the server while reading a message.");
			read += n;
		}
	}


	/// <summary>
	///     Send the StartupMessage: Int32 protocol version followed by
	///     null-terminated name/value pairs, terminated by a zero byte.
	///     Unlike every other frontend message, this one has no leading
	///     type byte.
	/// </summary>
	private void SendStartup()
	{
		if (string.IsNullOrEmpty(_initialParameters.UserName))
			throw new InvalidOperationException("Connection string does not specify a username.");

		const int ProtocolVersion3 = 0x00030000;

		var parameters = new List<(string Name, string Value)>(3)
		{
			("user", _initialParameters.UserName),
			("client_encoding", "UTF8"),
		};
		if (!string.IsNullOrEmpty(_initialParameters.DatabaseName))
			parameters.Add(("database", _initialParameters.DatabaseName));

		var length = 4 + 4 + 1; // length field + protocol version + trailing terminator
		foreach (var (name, value) in parameters)
			length += Encoding.UTF8.GetByteCount(name) + 1 + Encoding.UTF8.GetByteCount(value) + 1;

		var buffer = new byte[length];
		var span = buffer.AsSpan();

		BinaryPrimitives.WriteInt32BigEndian(span, length);
		BinaryPrimitives.WriteInt32BigEndian(span[4..], ProtocolVersion3);

		var offset = 8;
		foreach (var (name, value) in parameters)
		{
			offset += Encoding.UTF8.GetBytes(name, span[offset..]);
			buffer[offset++] = 0;
			offset += Encoding.UTF8.GetBytes(value, span[offset..]);
			buffer[offset++] = 0;
		}
		buffer[offset] = 0; // trailing terminator

		_stream.Write(buffer);
		_stream.Flush();
	}

	private static NetworkStream CreateClosedStream()
	{
		var ipAddr = new AddressFamily();
		var socket = new Socket(ipAddr, SocketType.Stream, ProtocolType.Tcp);
		socket.Dispose();
		return new NetworkStream(socket);
	}

	#endregion

}