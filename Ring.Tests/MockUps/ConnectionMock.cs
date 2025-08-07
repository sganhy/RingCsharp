using Bogus.DataSets;
using Newtonsoft.Json;
using Ring.Data.Models;
using Ring.Schema.Enums;
using System.Data;
using System.Linq.Expressions;

namespace Ring.Tests.MockUps;

internal class ConnectionMock : IConnection
{
    private readonly int _id;
    private ConnectionState _connectionState;
    private DateTime? _lastConnectionTime;
    private DatabaseProvider _databaseProvider;
    private string _connectionString;

    public ConnectionMock(int id, ConnectionState connectionState, DatabaseProvider databaseProvider, string connectionString)
    {
        _connectionState = connectionState;
        _databaseProvider = databaseProvider;
        _id = id;
        _connectionString = connectionString;
    }

    public int Id => _id;
    public string ConnectionString => _connectionString;
    public DateTime CreationTime => DateTime.Now;
    public DateTime? LastConnectionTime => _lastConnectionTime;
    public ConnectionState State => _connectionState;
    public void BeginTransaction() => Expression.Empty();
    public void Close() => Expression.Empty();
    public Task CloseAsync(CancellationToken cancellationToken) => 
        Task.Run(() =>
        {
            // do nothing 
        });

    public void Commit() => Expression.Empty();
    public IConnection CreateInstance(int id) => new ConnectionMock(id, _connectionState, _databaseProvider, _connectionString);
    public void Dispose() => Expression.Empty();

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
        _lastConnectionTime = DateTime.Now;
    }

    public Task OpenAsync(CancellationToken cancellationToken) => 
        Task.Run(() =>
        {
            _lastConnectionTime = DateTime.Now;
        });

    public int ProviderId() => (int)_databaseProvider;

    public void Rollback()
    {
        throw new NotImplementedException();
    }

    public sealed override string ToString() => $"{Id}";

}
