using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Tests.MockUps;
using System.Data;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Ring.Tests.Schema.Extensions;

public sealed class ConnectionPoolExtensionsTest : BaseTest
{
    [Fact]
    internal void Put_Connection_CloseConnections()
    {
        // arrange 
        var maxConnectionCount = 5;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 3, maxConnectionCount, connectionString);
        var connList = new List<IConnection>();
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));

        for (var i=0; i < 8; ++i)
        {
            var conn = pool.Get();
            connList.Add(conn);
        }

        foreach (var conn in connList)
        {
            // act 
            pool.Put(conn);
        }

        // assert
        Thread.Sleep(100); // wait 100 milliseconds - status updated async -->
        Assert.Equal(ConnectionState.Closed, connList[5].State);
        Assert.Equal(ConnectionState.Closed, connList[6].State);
        Assert.Equal(ConnectionState.Closed, connList[7].State);
    }

    [Fact]
    internal void Put_Connection_AvoidCrash()
    {
        // arrange 
        var maxConnectionCount = 100;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 10, maxConnectionCount, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var connList = new List<IConnection>();
        var hashSet = new HashSet<int>(maxConnectionCount*2);

        for (var i = 0; i < 255; ++i)
        {
            var conn = pool.Get();
            connList.Add(conn);
        }

        for (var i = 254; i >=0; --i)
        {
            // act 
            pool.Put(connList[i]);
        }

        // assert
        Assert.NotNull(pool.Connections[pool.Connections.Length-1]);
        Assert.Equal(maxConnectionCount-1, pool.Cursor);
        // id is unique ??
        foreach (var conn in pool.Connections)
        {
            Assert.NotNull(conn);
            Assert.Equal(ConnectionState.Open, conn.State);
            hashSet.Add(conn.Id);
        }
        Assert.Equal(255, pool.CreationCount);
        Assert.Equal(155, pool.DestroyCount);
    }

    [Fact]
    internal void Get_None_DifferentConnectionId()
    {
        // arrange 
        var maxConnectionCount = 64;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 15, maxConnectionCount, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var hashSet = new HashSet<int>(maxConnectionCount * 2);

        for (var i = 0; i < 15; ++i)
        {
            // act 
            var conn = pool.Get();
            pool.Put(conn);

            // assert
            Assert.DoesNotContain(conn.Id, hashSet);
            Assert.Equal(ConnectionState.Open, conn.State);
            hashSet.Add(conn.Id);
        }
    }

    [Fact]
    internal void Init_InitialConnectionObject_DifferentConnectionId()
    {
        // arrange 
        var maxConnectionCount = 64;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 15, maxConnectionCount, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var hashSet = new HashSet<int>(maxConnectionCount * 2);

        for (var i = 0; i < 15; ++i)
        {
            // act 
            var conn = pool.Get();
            pool.Put(conn);

            // assert
            Assert.DoesNotContain(conn.Id, hashSet);
            Assert.Equal(ConnectionState.Open, conn.State);
            hashSet.Add(conn.Id);
        }
        Assert.Equal(14, pool.Cursor);
        Assert.Equal(63, pool.LastIndex);
    }

    [Fact]
    internal async Task InitAsync_InitialConnectionObject_DifferentConnectionId()
    {
        // arrange 
        var maxConnectionCount = 128;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 64, maxConnectionCount, connectionString);
        await pool.InitAsync(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var hashSet = new HashSet<int>(maxConnectionCount * 2);

        for (var i = 0; i < 15; ++i)
        {
            // act 
            var conn = pool.Get();
            pool.Put(conn);

            // assert
            Assert.DoesNotContain(conn.Id, hashSet);
            Assert.Equal(ConnectionState.Open, conn.State);
            hashSet.Add(conn.Id);
        }
        Assert.Equal(63, pool.Cursor);
        Assert.Equal(127, pool.LastIndex);
    }

    [Fact]
    internal async Task Unload_None_AllConnectionDestroyed()
    {
        // arrange 
        var maxConnectionCount = 8;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 8, maxConnectionCount, connectionString);
        await pool.InitAsync(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var lstConns = new List<IConnection>(maxConnectionCount);
        foreach (var connection in pool.Connections) // copy  connections; to test it later
            if (connection != null) lstConns.Append(connection); 

        // act 
        var conn = pool.Get();
        pool.Unload();
        pool.Put(conn);

        // assert
        Assert.Equal(int.MinValue, pool.Cursor);
        Assert.Equal(int.MinValue, pool.LastIndex);
        Assert.Equal(ConnectionState.Closed, conn.State);
        foreach (var connection in pool.Connections) Assert.Null(connection);
        foreach (var connection in lstConns) Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.True(pool.Unloaded());
    }


    public ConnectionPoolExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

}
