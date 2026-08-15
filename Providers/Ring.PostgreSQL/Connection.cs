using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Helpers;
using Ring.Util.Builders.PostgreSQL;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL;

public sealed class Connection : IConnection
{
	private static readonly NetworkStream ClosedStream = NetworkStreamExtensions.CreateClosedStream(null);
	private static readonly DdlBuilder _ddlBuilder = new();

	// Transaction status as last reported by the server's ReadyForQuery
	// message: 'I' = idle (no transaction), 'T' = in transaction block,
	// 'E' = in a failed transaction block. Starts 'I' since a freshly
	// opened connection has no transaction in progress.
	private byte _transactionStatus = (byte)TransactionStatus.Idle;

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
			_sqlSendBuffer[0] = (byte)FrontendMessageCode.Query;
		}
		else _sqlSendBuffer = Array.Empty<byte>();
	}


	public void BeginTransaction()
	{
		if (_state != ConnectionState.Open)	throw new InvalidOperationException("The connection is not open.");
		if (_transactionStatus != (byte)TransactionStatus.Idle)	throw new InvalidOperationException("A transaction is already in progress.");

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
		if (_transactionStatus == (byte)TransactionStatus.Idle)
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
			var sql = "SELECT schemaname, tablename, tableowner, hasindexes FROM pg_catalog.pg_tables";
			
			// fire-and-forget, no Describe/RowDescription needed for generic parsing; 
			// keep it synchronous to avoid async overhead for a single round trip
			_stream.SendQuery(sql, _encoding.GetByteCount(sql), _encoding, _sqlSendBuffer); 

			return _stream.ReadRetrieveRecords(ref _transactionStatus, _encoding, 4);
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
	public OperationalError? Execute(in AlterQuery query, ReadOnlySpan<char> sql, int sqlByteCount) 
	{
		// Code size: 54 (0x36) - no virtual call
		_stream.SendQuery(sql, sqlByteCount, _encoding, _sqlSendBuffer);
		var returnValue = _stream.DrainToReadyForQuery(ref _transactionStatus);
		returnValue?.Set(query, _ddlBuilder);
		return returnValue;
	}
	public async ValueTask<OperationalError?> ExecuteAsync(AlterQuery query, string sql, int sqlByteCount, CancellationToken cancellationToken = default)
	{
		// Code size: 88 (0x58) - no virtual call
		_stream.SendQuery(sql, sqlByteCount, _encoding, _sqlSendBuffer);
		(var returnValue, var drainedBody) = await _stream.DrainToReadyForQueryAsync(cancellationToken).ConfigureAwait(false);
		if (returnValue is not null)
		{
			_transactionStatus = drainedBody.Length > 0 ? drainedBody[0] : (byte)TransactionStatus.Idle;
			returnValue.Set(query, _ddlBuilder);
		}
		return returnValue;
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
		if (_transactionStatus == (byte)TransactionStatus.Idle)
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
		_stream.SendQuery(sql.AsSpan(), _encoding.GetByteCount(sql), _encoding, _sqlSendBuffer);
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
		// Code size: 112 (0x70)
		//eg. SQL Error[42P07]: ERREUR: la relation « @meta » existe déjà
		while (true)
		{
			var (code, body) = _stream.ReadMessage(true);
			
			if (code == (byte)BackendMessageCode.ReadyForQuery)
			{
				_transactionStatus = body.Length > 0 ? (byte)body[0] : (byte)TransactionStatus.Idle;
				return;
			}

			if (code == (byte)BackendMessageCode.ErrorResponse)
			{
				//var error = AuthenticationHelper.ParseErrorResponse(body);
				// Drain remaining messages so the connection is left in a
				// clean, known state (ReadyForQuery) before surfacing the error.
				byte drainCode;
				byte[] drainBody;
				
				do 
					(drainCode, drainBody) = _stream.ReadMessage(false);	
				while (drainCode != (byte)BackendMessageCode.ReadyForQuery);

				_transactionStatus = drainBody.Length > 0 ? (byte)drainBody[0] : (byte)TransactionStatus.Idle;
				//throw error;
			}

			// CommandComplete, NoticeResponse, ParameterStatus, RowDescription,
			// DataRow, etc. - nothing to do for a fire-and-forget command.
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

	#endregion

}