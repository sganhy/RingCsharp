using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Helpers;
using Ring.PostgreSQL.Models;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL;

/// <summary>
/// A single PostgreSQL connection. Not thread-safe: one operation at a time (the pool guarantees exclusive access).
/// </summary>
public sealed class Connection : IConnection, IAsyncDisposable
{
	private const int MinTimeOut = 5000;
	private const int ReaderBufferSize = 64 * 1024;
	private const int ReaderMinimumReadSize = 8 * 1024;
	private const int WriterMinimumBufferSize = 16 * 1024;
	private static readonly byte[] TerminateMessage = { (byte)FrontendMessageCode.Terminate, 0, 0, 0, 4 };
	private readonly long _id;
	private readonly DateTime _creationTime;
	private DateTime? _lastConnectionTime;
	private readonly string _host;
	private readonly int _port;
	private readonly ConnectionParameters _parameters;
	private ConnectionState _state;
	private readonly int _timeout;
	private readonly Encoding _encoding;
	private bool _hasDeclaredStmts;
	private Socket? _socket;
	private NetworkStream? _networkStream;
	private Stream? _stream; // what the pipes sit on: the NetworkStream, or the SslStream wrapped around it when TLS was negotiated

	// Never null: the shared "closed" instances stand in until the connection is opened (and again after it is closed).
	private PipeWriter _writer = ClosedPipeWriter.Instance;
	private PipeReader _reader = ClosedPipeReader.Instance;

	private int _backendPid;
	private int _backendSecret;
	private bool _disposed;

	// Reused for every command: a connection runs one command at a time, so nothing is allocated per query.
	private readonly ManualResetEventSlim _syncGate = new(false);
	private readonly Action _signal;
	private CancellationTokenSource? _commandCts;
	public long Id => _id;
	public DateTime CreationTime => _creationTime;
	public DateTime? LastConnectionTime => _lastConnectionTime;
	public Encoding ClientEncoding => _encoding;
	public ConnectionState State => _state;
	public int BackendPid => _backendPid;
	public int BackendSecret => _backendSecret;
	public int ProviderId => (int)_parameters.DatabaseProvider;

	/// <summary>
	/// Maximum duration of one command (retrieve and async execute). Infinite by default.
	/// On expiry the command is cancelled and the connection is marked Broken.
	/// </summary>
	public TimeSpan CommandTimeout { get; set; } = Timeout.InfiniteTimeSpan;

	// ConnectionState is a flags enum (Open | Executing): test bits, never compare with == / !=.
	private bool IsOpenOrConnecting => (_state & (ConnectionState.Open | ConnectionState.Connecting)) != 0;
	private bool HasPipeline => !ReferenceEquals(_writer, ClosedPipeWriter.Instance);

	public bool HasDeclaredStmts => _hasDeclaredStmts;

	public Connection(string connectionString) : this(connectionString.ToConnectionParameters()) { }

	internal Connection(ConnectionParameters parameters)
	{
		_parameters = parameters;
		_disposed = false;
		_id = this.GetId(parameters.GetHashCode());
		_creationTime = DateTime.Now;
		_state = ConnectionState.None;
		_lastConnectionTime = null;
		_timeout = Math.Max(parameters.TimeOut, MinTimeOut);
		_host = parameters.Host;
		_port = parameters.Port;
		_encoding = Encoding.GetEncoding(parameters.ClientEncoding);
		_signal = _syncGate.Set;
		_hasDeclaredStmts = false;
	}

	public bool IsConnectionAlive()
	{
		if (_disposed || _state != ConnectionState.Open || _socket is null) return false;
		try
		{
			var readable = _socket.Poll(0, SelectMode.SelectRead);
			return !(readable && _socket.Available == 0);
		}
		catch (SocketException) { return false; }
		catch (ObjectDisposedException) { return false; }
	}

	public void Open()
	{
		// Code size: 159 (0x9f) - no virtual calls!
		// Fully synchronous: no sync-over - async, no thread - pool hop, no risk of deadlock under a SynchronizationContext.
		if ((_state & ConnectionState.Open) == ConnectionState.Open) ThrowConnectionAlreadyOpen();
		_state = ConnectionState.Connecting;
		CancellationTokenSource? deadline = null;
		CancellationTokenRegistration watchdog = default;
		try
		{
			AttachSocket(SocketHelper.ConnectSocket(_host, _port, _timeout));

			// One budget for everything after the TCP connect (TLS, startup, authentication): a server that accepts the connection and then
			// stays silent, or trickles bytes, must not hold the caller forever. Synchronous TLS and write calls cannot take a token,
			// so when the deadline expires the socket is aborted, which unblocks whatever is pending.
			deadline = new CancellationTokenSource(_timeout);
			var token = deadline.Token;
			watchdog = ArmWatchdog(token);

			AttachPipeline(TlsHelper.Negotiate(_networkStream!, _socket!, _host, _parameters.SslMode)); // ASSUMPTION: ConnectionParameters.SslMode (see SslMode.cs)
			_writer.SendStartup(_parameters);
			var (pid, secret) = AuthenticationHelper.HandleAuthentication(_reader, _writer, _host, _parameters.UserName, _parameters.Password, string.Empty, token);

			// The deadline must never kill a connection that just opened: disarm first (waits for a running callback), then re-check.
			watchdog.Dispose();
			token.ThrowIfCancellationRequested();
			CompleteOpen(pid, secret);
		}
		catch (Exception ex) when (deadline is { IsCancellationRequested: true })
		{
			DisposePipeline();
			_state = ConnectionState.None;
			throw HandshakeTimedOut(ex);
		}
		catch
		{
			DisposePipeline();
			_state = ConnectionState.None;
			throw;
		}
		finally
		{
			watchdog.Dispose();
			deadline?.Dispose();
		}
	}

	public ValueTask OpenAsync(CancellationToken cancellationToken) => new(OpenAsyncImpl(cancellationToken));

	public string?[] Execute(in RetrieveQuery query, ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		var token = CancellationTokenExtensions.AcquireToken(default, out var linked, CommandTimeout, _commandCts);
		try
		{
			_writer.SendQuery(sql);
			var result = ReadRetrieveRecordsSync(_reader, query.Table, token);
			_state = ConnectionState.Open;
			return result;
		}
		catch (PgOperationalError)
		{
			_state = ConnectionState.Open; // the stream was drained up to ReadyForQuery
			throw;
		}
		catch
		{
			_state = ConnectionState.Broken;
			throw;
		}
		finally
		{
			ReleaseToken(linked);
		}
	}

	public OperationalError? Execute(ReadOnlySpan<byte> sql)
	{
		// Code size: 94 (0x5e) - no virtual calls!
		_state = ConnectionState.Open | ConnectionState.Executing;
		_writer.SendQuery(sql);
		var error = _reader.DrainToReadyForQuery();
		_state = (error is not null && error.Value.Type != OperationalErrorType.PgError) ? ConnectionState.Broken : ConnectionState.Open;
		return error;
	}

	public OperationalError? Execute(in AlterQuery query, ReadOnlySpan<byte> sql)
	{
		// Code size: 109 (0x6d) - no virtual calls!
		_state = ConnectionState.Open | ConnectionState.Executing;
		_writer.SendQuery(sql);
		var error = _reader.DrainToReadyForQuery();
		if (error is not null)
		{
			error = error.Value.Set(query);
			_state = error.Value.Type != OperationalErrorType.PgError ? ConnectionState.Broken : ConnectionState.Open;
			return error;
		}
		_state = ConnectionState.Open;
		return null;
	}

	// Pooled builder: no state-machine allocation when the call suspends.
	// Callers must await the returned ValueTask exactly once (never store it or await it twice).
	[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
	public async ValueTask<OperationalError?> ExecuteAsync(AlterQuery query, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		CancellationTokenSource? linked = null;
		try
		{
			var token = cancellationToken.AcquireToken(out linked, CommandTimeout, _commandCts);
			await _writer.SendQueryAsync(sql, token).ConfigureAwait(false);
			var error = await _reader.DrainToReadyForQueryAsync(token).ConfigureAwait(false);

			if (error is not null)
			{
				if (error.Value.Type != OperationalErrorType.PgError)
				{
					_state = ConnectionState.Broken;
				}
				else
				{
					error = error.Value.Set(query);
					_state = ConnectionState.Open;
				}
				return error;
			}

			_state = ConnectionState.Open;
			return null;
		}
		finally
		{
			// Unconditional dispose on non-null local variable to satisfy CA2000
			linked?.Dispose();

			// Reset pooled CancellationTokenSource
			ResetPooledCts();
		}
	}

	public OperationalError? Execute(in SaveQuery query, ReadOnlySpan<byte> sql)
	{
		// Code size: 123 (0x7b) - no virtual calls!
		_state = ConnectionState.Open | ConnectionState.Executing;
		_writer.SendExtendedQuery(sql, query, _encoding);
		var error = _reader.DrainToReadyForQuery();
		if (error is not null)
		{
			error = error.Value.Set(query);
			_state = error.Value.Type != OperationalErrorType.PgError ? ConnectionState.Broken : ConnectionState.Open;
			return error;
		}
		_state = ConnectionState.Open;
		return null;
	}

	public void Close()
	{
		// Code size: 91 (0x5b) - no virtual calls!
		if (!IsOpenOrConnecting)
		{
			_state = ConnectionState.Closed;
			DisposePipeline();
			return;
		}

		try
		{
			if (HasPipeline)
			{
				_writer.Write(TerminateMessage);
				_writer.FlushBlocking();
			}
			_state = ConnectionState.Closed;
		}
		catch
		{
			_state = ConnectionState.Broken;
			throw;
		}
		finally
		{
			DisposePipeline();
		}
	}

	public ValueTask CloseAsync(CancellationToken cancellationToken = default) => new(CloseAsyncImpl(cancellationToken));

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		if (IsOpenOrConnecting)
		{
			try { Close(); } catch { }
		}

		ReleaseResources();
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;
		_disposed = true;

		if (IsOpenOrConnecting)
		{
			try { await CloseAsyncImpl(CancellationToken.None).ConfigureAwait(false); } catch { }
		}

		ReleaseResources();
	}

	public IConnection CreateInstance(int id, int sqlSendBufferSize) => new Connection(_parameters.Set(id, sqlSendBufferSize));

	#region Private Methods

	private string?[] ReadRetrieveRecordsSync(PipeReader reader, Table table, CancellationToken cancellationToken)
		=> WaitSync(reader.ReadRetrieveRecordsAsync(_encoding, table, -1, cancellationToken));

	// Blocks on a ValueTask without AsTask(): a cached callback and a reusable event, so no Task is allocated.
	private T WaitSync<T>(ValueTask<T> valueTask)
	{
		var awaiter = valueTask.GetAwaiter();
		if (!awaiter.IsCompleted)
		{
			_syncGate.Reset();
			awaiter.UnsafeOnCompleted(_signal);
			_syncGate.Wait();
		}
		return awaiter.GetResult();
	}

	// Synchronous I/O cannot take a token. When the deadline of a synchronous operation expires, the transport is aborted instead:
	// that unblocks whatever read or write is pending, and the connection is Broken (as documented for CommandTimeout).
	private CancellationTokenRegistration ArmWatchdog(CancellationToken token)
		=> token.CanBeCanceled ? token.Register(static state => ((Connection)state!).AbortTransport(), this) : default;

	private void AbortTransport()
	{
		var socket = _socket;
#pragma warning disable CA1031 // Do not catch general exception types
		try { socket?.Dispose(); } catch { }
#pragma warning restore CA1031
	}

	private void ReleaseToken(CancellationTokenSource? linked)
	{
		if (linked is not null)
		{
			linked.Dispose();
			return;
		}

		var cts = _commandCts;
		if (cts is not null && !cts.TryReset())
		{
			cts.Dispose();
			_commandCts = null;
		}
	}

	// The non-I/O part of opening, shared by Open() and OpenAsyncImpl().
	private void AttachSocket(Socket socket)
	{
		_socket = socket;
		socket.NoDelay = true;
		socket.ConfigureKeepAlive();

		_networkStream = new NetworkStream(socket, ownsSocket: true);
	}

	// The pipes are built only once the transport is final (plain or TLS). The writer uses a scrubbing pool: its buffers carry passwords
	// and SCRAM proofs, and are zeroed when the writer releases them instead of poking at memory after Advance.
	private void AttachPipeline(Stream stream)
	{
		_stream = stream;
		_writer = PipeWriter.Create(stream, new StreamPipeWriterOptions(pool: ScrubbingMemoryPool.Instance, minimumBufferSize: WriterMinimumBufferSize, leaveOpen: true));
		_reader = PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: ReaderBufferSize, minimumReadSize: ReaderMinimumReadSize, leaveOpen: true, useZeroByteReads: true));
	}

	private void CompleteOpen(int? pid, int? secret)
	{
		_backendPid = pid ?? 0;
		_backendSecret = secret ?? 0;
		_lastConnectionTime = DateTime.Now;
		_state = ConnectionState.Open;
	}

	private async Task OpenAsyncImpl(CancellationToken cancellationToken)
	{
		if ((_state & ConnectionState.Open) == ConnectionState.Open) ThrowConnectionAlreadyOpen();
		_state = ConnectionState.Connecting;
		CancellationTokenSource? deadline = null;
		try
		{
			AttachSocket(await SocketHelper.ConnectSocketAsync(_host, _port, _timeout, cancellationToken).ConfigureAwait(false));

			// One budget for everything after the TCP connect (TLS, startup, authentication), on top of the caller's token:
			// a server that accepts the connection and then stays silent must not hold the caller forever.
			deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			deadline.CancelAfter(_timeout);
			var token = deadline.Token;

			AttachPipeline(await TlsHelper.NegotiateAsync(_networkStream!, _socket!, _host, _parameters.SslMode, token).ConfigureAwait(false));
			await _writer.SendStartupAsync(_parameters, token).ConfigureAwait(false);
			var (pid, secret) = await AuthenticationHelper.HandleAuthenticationAsync(_reader, _writer, _host, _parameters.UserName, _parameters.Password, string.Empty, token).ConfigureAwait(false);
			CompleteOpen(pid, secret);
		}
		catch (Exception ex) when (deadline is { IsCancellationRequested: true } && !cancellationToken.IsCancellationRequested)
		{
			DisposePipeline();
			_state = ConnectionState.None;
			throw HandshakeTimedOut(ex);
		}
		catch
		{
			DisposePipeline();
			_state = ConnectionState.None;
			throw;
		}
		finally
		{
			deadline?.Dispose();
		}
	}

	private async Task CloseAsyncImpl(CancellationToken cancellationToken)
	{
		if (!IsOpenOrConnecting)
		{
			_state = ConnectionState.Closed;
			DisposePipeline();
			return;
		}
		if (HasPipeline)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				_writer.Write(TerminateMessage);
				await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
			catch { }
#pragma warning restore CA1031
		}
		DisposePipeline();
		_state = ConnectionState.Closed;
	}

	/// <summary>
	/// Every step is independent: one failing Complete/Dispose never prevents the others from running. 
	/// </summary>
	private void DisposePipeline()
	{
		// Code size: 134 (0x86)
		var writer = _writer;
		var reader = _reader;
		var tlsStream = _stream;
		var stream = _networkStream;
		var socket = _socket;

		_writer = ClosedPipeWriter.Instance;
		_reader = ClosedPipeReader.Instance;
		_stream = null;
		_networkStream = null;
		_socket = null;
		_backendPid = 0;
		_backendSecret = 0;

#pragma warning disable CA1031 // Do not catch general exception types
		try { writer.Complete(); } catch { }
		try { reader.Complete(); } catch { }
		try { tlsStream?.Dispose(); } catch { } // SslStream first (leaveInnerStreamOpen: the NetworkStream is disposed right after)
		try { stream?.Dispose(); } catch { }
		try { socket?.Dispose(); } catch { }
#pragma warning restore CA1031
	}

	private void ResetPooledCts()
	{
		var cts = _commandCts;
		if (cts is not null && !cts.TryReset())
		{
			cts.Dispose();
			_commandCts = null;
		}
	}

	private void ReleaseResources()
	{
		DisposePipeline();
		_state = ConnectionState.Closed;
		_commandCts?.Dispose();
		_commandCts = null;
		_syncGate.Dispose();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private TimeoutException HandshakeTimedOut(Exception inner)
		=> new($"Timed out after {_timeout} ms while negotiating TLS and authenticating with the server.", inner);

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowConnectionAlreadyOpen() => throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.ConnectionAlreadyOpen));

	#endregion
}