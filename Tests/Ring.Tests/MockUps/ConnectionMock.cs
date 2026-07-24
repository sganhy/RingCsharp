using Ring.Data;
using Ring.Data.Models;
using Ring.Schema.Enums;
using System.Linq.Expressions;
using System.Text;

namespace Ring.Tests.MockUps;

internal class ConnectionMock : IConnection
{
    private readonly long _id;
    private ConnectionState _connectionState;
    private DateTime? _lastConnectionTime;
    private DatabaseProvider _databaseProvider;
    private string _connectionString;

    public ConnectionMock(int id, DatabaseProvider databaseProvider, string connectionString)
    {
        _connectionState = ConnectionState.Closed;
        _databaseProvider = databaseProvider;
        _id = id;
        _connectionString = connectionString;
    }

    public long Id => _id;
    public string ConnectionString => _connectionString;
    public DateTime CreationTime => DateTime.Now;
    public DateTime? LastConnectionTime => _lastConnectionTime;
    public ConnectionState State => _connectionState;
    public Encoding ClientEncoding => Encoding.UTF8;
	public void BeginTransaction() => Expression.Empty();
    public void Close() => _connectionState = ConnectionState.Closed;
    public Task CloseAsync(CancellationToken cancellationToken) => 
        Task.Run(() =>
        {
            _connectionState = ConnectionState.Closed;
            // do nothing 
        });

    public void Commit() => Expression.Empty();
    public IConnection CreateInstance(int id, int sqlSendBufferSize) => new ConnectionMock(id, _databaseProvider, _connectionString);
    public void Dispose() => Expression.Empty();

    public string?[] Execute(in RetrieveQuery query)
    {
        throw new NotImplementedException();
    }

    public ConnectionOperationalError? Execute(in AlterQuery query, ReadOnlySpan<char> sql, int sqlByteCount)
    {
        throw new NotImplementedException();
    }

    public long Execute(in SaveQuery query)
    {
        throw new NotImplementedException();
    }

    public ValueTask ExecuteAsync(in AlterQuery query, ReadOnlySpan<char> sql, int sqlByteCount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool IsConnectionAlive() => true;

	public void Open()
    {
        _lastConnectionTime = DateTime.Now;
        _connectionState = ConnectionState.Open;
    }

    public Task OpenAsync(CancellationToken cancellationToken) => 
        Task.Run(() =>
        {
            _lastConnectionTime = DateTime.Now;
            _connectionState = ConnectionState.Open;
        });

    public int ProviderId() => (int)_databaseProvider;

    public void Rollback()
    {
        throw new NotImplementedException();
    }

    public sealed override string ToString() => $"{Id} - {_connectionState}";

}
