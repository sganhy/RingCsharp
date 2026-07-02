using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Helpers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace Ring.PostgreSQL;

public sealed class Connection : IConnection
{

	// interface 
	private readonly long _id;
	private readonly string _connectionString;
	private readonly DateTime _creationTime;
	private readonly DateTime? _lastConnectionTime;
	private readonly string _host;
	private readonly int _port;
	private readonly ConnectionParameters _initialParameters;

	private ConnectionState _state;

	// tcp connection
	private readonly int _timeout; // milliseconds
	private NetworkStream _stream;
	private int _backendPid;
	private int _backendSecret; 


	public long Id => _id;
	public string ConnectionString => _connectionString;
	public DateTime CreationTime => _creationTime;
	public DateTime? LastConnectionTime => _lastConnectionTime;
	public ConnectionState State => _state;

	// Backend process identification from server (set after successful authentication)
	public int? BackendPid => _backendPid;
	public int? BackendSecret => _backendSecret;

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
		throw new NotImplementedException();
	}

	public void Close()
	{
		// If not open, nothing to do
		if (_state != ConnectionState.Open && _state != ConnectionState.Connecting)
		{
			_state = ConnectionState.Closed;
			_stream?.Dispose();
			_stream = null;
			_backendPid = 0;
			_backendSecret = 0;
			return;
		}

		try
		{
			// Send Terminate message: type 'X', length = 4
			if (_stream != null && _stream.CanWrite)
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

			_stream?.Dispose();
			_stream = null;
			_backendPid = 0;
			_backendSecret = 0;
			_state = ConnectionState.Closed;
		}
		catch
		{
			// If disposing failed, mark connection as broken
			try { _stream?.Dispose(); } catch { }
			_stream = null;
			_state = ConnectionState.Broken;
			_backendPid = 0;
			_backendSecret = 0;
			throw;
		}
	}

	public Task CloseAsync(CancellationToken cancellationToken = default)
	{
		return CloseAsyncImpl(cancellationToken);
	}

	/// <summary>
	///     Unlike the previous implementation, this does not wrap itself in
	///     Task.Run: the underlying I/O (WriteAsync/DisposeAsync) is already
	///     natively asynchronous, so an extra thread-pool hop just adds
	///     overhead. Wrapping also had a correctness bug - Task.Run(Func&lt;Task&gt;,
	///     CancellationToken) can observe the token as already-canceled and
	///     complete the returned task WITHOUT ever invoking the delegate, so
	///     a caller who passed an already-canceled token (or one that fired
	///     right before the call) would leak the stream/socket entirely.
	///     Here, cancellation can only affect the "send Terminate" write;
	///     the stream is always disposed and the connection is always left
	///     in a well-defined state afterwards.
	/// </summary>
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

		if (_stream != null && _stream.CanWrite)
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
			_stream = null;
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

	private async ValueTask DisposeStreamAsync()
	{
		if (_stream is { } stream)
		{
			_stream = null;
			await stream.DisposeAsync().ConfigureAwait(false);
		}
	}

	public void Commit()
	{
		throw new NotImplementedException();
	}

	public IConnection CreateInstance(int id)
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		throw new NotImplementedException();
	}

	public string?[] Execute(in RetrieveQuery query)
	{
		throw new NotImplementedException();
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

	public Task OpenAsync(CancellationToken cancellationToken)
	{
		return OpenAsyncImpl(cancellationToken);
	}
	private Task OpenAsyncImpl(CancellationToken cancellationToken)
	{
		return Task.Run(async () =>
		{
			if (_state == ConnectionState.Open)
				throw new InvalidOperationException("The connection is already open.");

			_state = ConnectionState.Connecting;

			try
			{
				// Run the synchronous connect on the threadpool to avoid blocking the caller
				var socket = await Task.Run(() => SocketHelper.ConnectSocket(_host, _port, _timeout), cancellationToken).ConfigureAwait(false);

				socket.NoDelay = true;

				_stream = new NetworkStream(socket, ownsSocket: true);

				// Startup and authentication may perform network I/O; run on threadpool to avoid blocking
				SendStartup();
				var (pid, secret) = await Task.Run(() => AuthenticationHelper.HandleAuthenticationAsync(_stream, _initialParameters.UserName, _initialParameters.Password), cancellationToken).ConfigureAwait(false);
				_backendPid = pid ?? 0;
				_backendSecret = secret ?? 0;

				_state = ConnectionState.Open;
			}
			catch (OperationCanceledException)
			{
				_stream?.Dispose(); // also disposes underlying socket
				_stream = null;
				_state = ConnectionState.Undefined;
				throw;
			}
			catch
			{
				_stream?.Dispose(); // also disposes underlying socket
				_stream = null;
				_state = ConnectionState.Undefined;
				throw;
			}
		}, cancellationToken);
	}

	public int ProviderId()
	{
		throw new NotImplementedException();
	}

	public void Rollback()
	{
		throw new NotImplementedException();
	}

	#region private methods




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

		_stream!.Write(buffer);
		_stream.Flush();
	}


	#endregion

}
