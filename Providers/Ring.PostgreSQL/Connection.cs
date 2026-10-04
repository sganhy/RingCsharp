using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Helpers;
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

public sealed class Connection : IConnection
{
	private const int MinTimeOut = 5000;
	private static readonly byte[] TerminateMessage = { (byte)FrontendMessageCode.Terminate, 0, 0, 0, 4 };
	private byte _transactionStatus = (byte)TransactionStatus.Idle;

	private readonly long _id;
	private readonly DateTime _creationTime;
	private readonly DateTime? _lastConnectionTime;
	private readonly string _host;
	private readonly int _port;
	private readonly ConnectionParameters _parameters;
	private ConnectionState _state;

	private readonly int _timeout;
	private readonly Encoding _encoding;

	private Socket? _socket;
	private NetworkStream? _networkStream;
	private PipeWriter? _writer;
	private PipeReader? _reader;

	private int _backendPid;
	private int _backendSecret;
	private bool _disposed;

	public long Id => _id;
	public DateTime CreationTime => _creationTime;
	public DateTime? LastConnectionTime => _lastConnectionTime;
	public Encoding ClientEncoding => _encoding;
	public ConnectionState State => _state;
	public int ProviderId => (int)_parameters.DatabaseProvider;

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
	}

	public bool IsConnectionAlive()
	{
		if (_state != ConnectionState.Open || _socket is null) return false;
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
		if ((_state & ConnectionState.Open) == ConnectionState.Open) ThrowConnectionAlreadyOpen();
		OpenAsyncImpl(CancellationToken.None).GetAwaiter().GetResult();
	}

	public Task OpenAsync(CancellationToken cancellationToken) => OpenAsyncImpl(cancellationToken);

	public string?[] Execute(in RetrieveQuery query, ReadOnlySpan<byte> sql)
	{
		try
		{
			_writer!.SendQuery(sql);
			return ReadRetrieveRecordsSync(_reader!, query.Table);
		}
		catch (PgOperationalError)
		{
			throw;
		}
		catch
		{
			_state = ConnectionState.Broken;
			throw;
		}
	}

	public OperationalError? Execute(ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		_writer!.SendQuery(sql);
		var returnValue = DrainToReadyForQuerySync(_reader!);
		_state = ConnectionState.Open;
		return returnValue;
	}

	public OperationalError? Execute(in AlterQuery query, ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		_writer!.SendQuery(sql);
		var returnValue = DrainToReadyForQuerySync(_reader!);
		returnValue?.Set(query);
		_state = ConnectionState.Open;
		return returnValue;
	}

	public async ValueTask<OperationalError?> ExecuteAsync(AlterQuery query, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default)
	{
		await _writer!.SendQueryAsync(sql, cancellationToken).ConfigureAwait(false);
		(var returnValue, var drainedBody) = await _reader!.DrainToReadyForQueryAsync(cancellationToken).ConfigureAwait(false);
		if (returnValue is not null)
		{
			_transactionStatus = drainedBody.Length > 0 ? drainedBody[0] : (byte)TransactionStatus.Idle;
			returnValue.Set(query);
		}
		return returnValue;
	}

	public OperationalError? Execute(in SaveQuery query, ReadOnlySpan<byte> sql)
	{
		_state = ConnectionState.Open | ConnectionState.Executing;
		byte[]? rentedPayload = null;
		try
		{
			var payloadSize = query.GetVariablesPayloadSize(_encoding);
			rentedPayload = ArrayPool<byte>.Shared.Rent(payloadSize);
			var actualPayloadSize = query.WriteVariablesPayload(rentedPayload, _encoding);

			_writer!.SendExtendedQuery(sql, rentedPayload.AsSpan(0, actualPayloadSize));

			var returnValue = DrainToReadyForQuerySync(_reader!);
			_state = ConnectionState.Open;
			return returnValue;
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
		finally
		{
			if (rentedPayload is not null) ArrayPool<byte>.Shared.Return(rentedPayload);
		}
	}

	public void Close()
	{
		if (_state != ConnectionState.Open && _state != ConnectionState.Connecting)
		{
			_state = ConnectionState.Closed;
			DisposePipeline();
			return;
		}

		try
		{
			if (_writer is not null)
			{
				_writer.Write(TerminateMessage);
				_writer.FlushAsync().AsTask().GetAwaiter().GetResult();
			}
			DisposePipeline();
			_state = ConnectionState.Closed;
		}
		catch
		{
			_state = ConnectionState.Broken;
			DisposePipeline();
			throw;
		}
	}

	public Task CloseAsync(CancellationToken cancellationToken = default) => CloseAsyncImpl(cancellationToken);

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		if (_state == ConnectionState.Open || _state == ConnectionState.Connecting)
		{
			try { Close(); } catch { }
		}

		DisposePipeline();
		GC.SuppressFinalize(this);
	}

	public IConnection CreateInstance(int id, int sqlSendBufferSize) => new Connection(_parameters.Set(id, sqlSendBufferSize));

	#region Private Methods

	private string?[] ReadRetrieveRecordsSync(PipeReader reader, Table table)
	{
		Span<byte> txHolder = stackalloc byte[1];
		txHolder[0] = _transactionStatus;
		var task = reader.ReadRetrieveRecordsAsync(txHolder.ToArray(), _encoding, table).AsTask();
		var results = task.GetAwaiter().GetResult();
		_transactionStatus = txHolder[0];
		return results;
	}

	private OperationalError? DrainToReadyForQuerySync(PipeReader reader)
	{
		var (error, drainedBody) = reader.DrainToReadyForQueryAsync().AsTask().GetAwaiter().GetResult();
		if (drainedBody.Length > 0)
		{
			_transactionStatus = drainedBody[0];
		}
		return error;
	}

	private async Task OpenAsyncImpl(CancellationToken cancellationToken)
	{
		if ((_state & ConnectionState.Open) == ConnectionState.Open) ThrowConnectionAlreadyOpen();
		_state = ConnectionState.Connecting;
		try
		{
			var socket = await Task.Run(() => SocketHelper.ConnectSocket(_host, _port, _timeout), cancellationToken).ConfigureAwait(false);
			socket.NoDelay = true;

			_socket = socket;
			_networkStream = new NetworkStream(socket, ownsSocket: true);
			_writer = PipeWriter.Create(_networkStream, new StreamPipeWriterOptions(leaveOpen: true));
			_reader = PipeReader.Create(_networkStream, new StreamPipeReaderOptions(leaveOpen: true));

			await _writer.SendStartupAsync(_parameters, cancellationToken).ConfigureAwait(false);
			var (pid, secret) = await AuthenticationHelper.HandleAuthenticationAsync(_reader, _writer, _parameters.UserName, _parameters.Password, cancellationToken).ConfigureAwait(false);

			_backendPid = pid ?? 0;
			_backendSecret = secret ?? 0;
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
		if (_state != ConnectionState.Open && _state != ConnectionState.Connecting)
		{
			_state = ConnectionState.Closed;
			DisposePipeline();
			return;
		}

		if (_writer is not null)
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

	private void DisposePipeline()
	{
		_writer?.Complete();
		_reader?.Complete();
		_networkStream?.Dispose();
		_socket?.Dispose();

		_writer = null;
		_reader = null;
		_networkStream = null;
		_socket = null;
		_backendPid = 0;
		_backendSecret = 0;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowConnectionAlreadyOpen() =>
		throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.ConnectionAlreadyOpen));

	#endregion
}