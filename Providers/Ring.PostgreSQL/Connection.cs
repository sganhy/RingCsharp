using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.PostgreSQL.Extensions;
using System.Net.Sockets;

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
	private NetworkStream? _stream;
	


	public long Id => _id;
	public string ConnectionString => _connectionString;
	public DateTime CreationTime => _creationTime;
	public DateTime? LastConnectionTime => _lastConnectionTime;
	public ConnectionState State => _state;

	public Connection(string connectionString) : this (connectionString, connectionString.ToConnectionParameters()) {} 
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
		throw new NotImplementedException();
	}

	public Task CloseAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
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
		_state = ConnectionState.Connecting;
		var tcp = new TcpClient();
		/*
		if (!tcp.ConnectAsync(Host, Port).Wait(_timeout))
		{
			throw new PgOperationalError(
				$"Connection to {Host}:{Port} timed out after {_timeout} ms.",
				"08001", "FATAL", "", "");
		}

		tcp.ReceiveTimeout = _timeout;
		tcp.SendTimeout = _timeout;
		_stream = tcp.GetStream();

		try
		{
			SendStartup();
			HandleAuth();
		}
		catch
		{
			_stream.Close();
			_stream = null;
			throw;
		}
		*/
	}

	public Task OpenAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public int ProviderId()
	{
		throw new NotImplementedException();
	}

	public void Rollback()
	{
		throw new NotImplementedException();
	}
}
