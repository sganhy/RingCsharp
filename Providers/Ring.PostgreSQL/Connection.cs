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
	private const int KeepAliveTimeSeconds = 60;
	private const int KeepAliveIntervalSeconds = 10;
	private const int KeepAliveRetryCount = 3;
	private const int MaxSqlLength = 0x3FFFFFFF - 16; // PostgreSQL caps a message at 1 GB - 1 (the length field counts its own 4 bytes).
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

	private Socket? _socket;
	private NetworkStream? _networkStream;

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
	public int ProviderId => (int)_parameters.DatabaseProvider;

	/// <summary>
	/// Maximum duration of one command (retrieve and async execute). Infinite by default.
	/// On expiry the command is cancelled and the connection is marked Broken.
	/// </summary>
	public TimeSpan CommandTimeout { get; set; } = Timeout.InfiniteTimeSpan;

	// ConnectionState is a flags enum (Open | Executing): test bits, never compare with == / !=.
	private bool IsOpenOrConnecting => (_state & (ConnectionState.Open | ConnectionState.Connecting)) != 0;

	private bool HasPipeline => !ReferenceEquals(_writer, ClosedPipeWriter.Instance);

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
		ThrowIfDisposed();
		OpenAsyncImpl(CancellationToken.None).GetAwaiter().GetResult();
	}

	public Task OpenAsync(CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		return OpenAsyncImpl(cancellationToken);
	}

	public string?[] Execute(in RetrieveQuery query, ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		var token = AcquireToken(default, out var linked);
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
		_state = ConnectionState.Open | ConnectionState.Executing;
		try
		{
			_writer.SendQuery(sql);
			var error = _reader.DrainToReadyForQuery();
			_state = ConnectionState.Open;
			return error;
		}
		catch (PgOperationalError)
		{
			_state = ConnectionState.Open;
			throw;
		}
		catch
		{
			_state = ConnectionState.Broken;
			throw;
		}
	}

	public OperationalError? Execute(in AlterQuery query, ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		try
		{
			_writer.SendQuery(sql);
			var error = _reader.DrainToReadyForQuery();
			error?.Set(query);
			_state = ConnectionState.Open;
			return error;
		}
		catch (PgOperationalError)
		{
			_state = ConnectionState.Open;
			throw;
		}
		catch
		{
			_state = ConnectionState.Broken;
			throw;
		}
	}

	// Pooled builder: no state-machine allocation when the call suspends.
	// Callers must await the returned ValueTask exactly once (never store it or await it twice).
	[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
	public async ValueTask<OperationalError?> ExecuteAsync(AlterQuery query, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		var token = AcquireToken(cancellationToken, out var linked);
		try
		{
			await _writer.SendQueryAsync(sql, token).ConfigureAwait(false);
			var error = await _reader.DrainToReadyForQueryAsync(token).ConfigureAwait(false);
			error?.Set(query);
			_state = ConnectionState.Open;
			return error;
		}
		catch (PgOperationalError)
		{
			_state = ConnectionState.Open;
			throw;
		}
		catch
		{
			// includes cancellation and timeout: the response stream is half consumed, the connection cannot be reused
			_state = ConnectionState.Broken;
			throw;
		}
		finally
		{
			ReleaseToken(linked);
		}
	}

	public OperationalError? Execute(in SaveQuery query, ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		try
		{
			_writer.SendExtendedQuery(sql, query, _encoding);
			var error = _reader.DrainToReadyForQuery();
			_state = ConnectionState.Open;
			return error;
		}
		catch (PgOperationalError)
		{
			_state = ConnectionState.Open;
			throw;
		}
		catch
		{
			// A format/overflow error can happen after Parse/Bind bytes were already advanced in the writer:
			// those bytes would be sent in front of the next query, so the connection must not be reused.
			_state = ConnectionState.Broken;
			throw;
		}
	}

	public void Close()
	{
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

	public Task CloseAsync(CancellationToken cancellationToken = default) => CloseAsyncImpl(cancellationToken);

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

	// Applies CommandTimeout. Reuses one CancellationTokenSource when the caller passes no token;
	// a linked source is only created when both a caller token and a timeout exist.
	private CancellationToken AcquireToken(CancellationToken external, out CancellationTokenSource? linked)
	{
		linked = null;
		var timeout = CommandTimeout;
		if (timeout <= TimeSpan.Zero) return external;

		if (external.CanBeCanceled)
		{
			linked = CancellationTokenSource.CreateLinkedTokenSource(external);
			linked.CancelAfter(timeout);
			return linked.Token;
		}

		var cts = _commandCts ??= new CancellationTokenSource();
		cts.CancelAfter(timeout);
		return cts.Token;
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

	private async Task OpenAsyncImpl(CancellationToken cancellationToken)
	{
		if ((_state & ConnectionState.Open) == ConnectionState.Open) ThrowConnectionAlreadyOpen();
		_state = ConnectionState.Connecting;
		try
		{
			var socket = await SocketHelper.ConnectSocketAsync(_host, _port, _timeout, cancellationToken).ConfigureAwait(false);
			_socket = socket;
			socket.NoDelay = true;
			ConfigureKeepAlive(socket);

			_networkStream = new NetworkStream(socket, ownsSocket: true);
			_writer = PipeWriter.Create(_networkStream, new StreamPipeWriterOptions(minimumBufferSize: WriterMinimumBufferSize, leaveOpen: true));
			_reader = PipeReader.Create(_networkStream, new StreamPipeReaderOptions(bufferSize: ReaderBufferSize, minimumReadSize: ReaderMinimumReadSize, leaveOpen: true, useZeroByteReads: true));

			await _writer.SendStartupAsync(_parameters, cancellationToken).ConfigureAwait(false);
			var (pid, secret) = await AuthenticationHelper.HandleAuthenticationAsync(_reader, _writer, _host, _parameters.UserName, _parameters.Password, string.Empty, cancellationToken).ConfigureAwait(false);

			_backendPid = pid ?? 0;
			_backendSecret = secret ?? 0;
			_lastConnectionTime = DateTime.Now;
			_state = ConnectionState.Open;
		}
		catch
		{
			DisposePipeline();
			_state = ConnectionState.None;
			throw;
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
			try
			{
				_writer.Write(TerminateMessage);
				await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
			catch { }
		}

		DisposePipeline();
		_state = ConnectionState.Closed;
	}

	// Detects silently dropped connections (cable pulled, firewall idle timeout, crashed server host).
	private static void ConfigureKeepAlive(Socket socket)
	{
		try
		{
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, KeepAliveTimeSeconds);
			socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, KeepAliveIntervalSeconds);
			socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, KeepAliveRetryCount);
		}
		catch (SocketException) { }
		catch (PlatformNotSupportedException) { }
		catch (ArgumentException) { }
	}

	// Every step is independent: one failing Complete/Dispose never prevents the others from running.
	private void DisposePipeline()
	{
		var writer = _writer;
		var reader = _reader;
		var stream = _networkStream;
		var socket = _socket;

		_writer = ClosedPipeWriter.Instance;
		_reader = ClosedPipeReader.Instance;
		_networkStream = null;
		_socket = null;
		_backendPid = 0;
		_backendSecret = 0;

#pragma warning disable CA1031 // Do not catch general exception types
		try { writer.Complete(); } catch { }
		try { reader.Complete(); } catch { }
		try { stream?.Dispose(); } catch { }
		try { socket?.Dispose(); } catch { }
#pragma warning restore CA1031 // Do not catch general exception types
	}

	private void ReleaseResources()
	{
		DisposePipeline();
		_state = ConnectionState.Closed;

		_commandCts?.Dispose();
		_commandCts = null;
		_syncGate.Dispose();
	}

	private void ThrowIfDisposed()
	{
		if (_disposed) ThrowDisposed();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowDisposed() =>	throw new ObjectDisposedException(nameof(Connection));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowConnectionAlreadyOpen() =>	throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.ConnectionAlreadyOpen));

	#endregion
}
