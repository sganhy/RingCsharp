using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Tests.MockUps;
using Ring.Data;
using System.Linq.Expressions;

namespace Ring.Tests.Schema.Extensions;

public sealed class ConnectionPoolExtensionsTest : BaseTest
{
	public ConnectionPoolExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
    internal void Put_Connection_CloseConnections()
    {
        // arrange 
        var maxConnectionCount = 5;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 3, maxConnectionCount, 0, connectionString);
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
        Thread.Sleep(500); // wait 500 milliseconds - status updated async -->
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
        var pool = new ConnectionPool(_faker.Random.Number(), 10, maxConnectionCount, 0, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var connList = new List<IConnection>();
        var hashSet = new HashSet<long>(maxConnectionCount*2);

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
        Assert.Equal(99, pool.LastIndex);
        Assert.Equal(99, pool.Cursor);
    }

    [Fact]
    internal void Get_None_DifferentConnectionId()
    {
        // arrange 
        var maxConnectionCount = 64;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 15, maxConnectionCount, 0, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var hashSet = new HashSet<long>(maxConnectionCount * 2);

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
	internal void Get_MaxConnectionEqualTo1()
	{
		// arrange 
		var maxConnectionCount = 1;
		var connectionString = _faker.Random.String();
		var pool = new ConnectionPool(_faker.Random.Number(), 1, maxConnectionCount, 0, connectionString);
		pool.Init(new ConnectionMock(_faker.Random.Int(), DatabaseProvider.Oracle, connectionString));

		// act 
		var conn1 = pool.Get();
		var conn2 = pool.Get();
		pool.Put(conn2);
		pool.Put(conn1);
        var conn3 = pool.Get();
		pool.Put(conn3);

		// assert
		Assert.Equal(0, pool.Cursor);
		Assert.Equal(0, pool.LastIndex);
		Assert.Equal(1, conn1.Id);
		Assert.Equal(2, conn2.Id);
		Assert.Equal(2, conn3.Id);
        // wait end of destroy async
        Thread.Sleep(500);
		Assert.Equal(ConnectionState.Closed, conn1.State); // conn1 should be destroyed!
	}

	[Fact]
    internal void Init_InitialConnectionObject_DifferentConnectionId()
    {
        // arrange 
        var maxConnectionCount = 64;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 15, maxConnectionCount, 0, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString));
        var hashSet = new HashSet<long>(maxConnectionCount * 2);

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
        var pool = new ConnectionPool(_faker.Random.Number(), 64, maxConnectionCount, 0, connectionString);
        await pool.InitAsync(new ConnectionMock(1, DatabaseProvider.SqlLite, connectionString), cancellationToken: TestContext.Current.CancellationToken);
        var hashSet = new HashSet<long>(maxConnectionCount * 2);

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
    internal void Unload_None_AllConnectionDestroyed()
    {
        // arrange 
        var maxConnectionCount = 8;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 8, maxConnectionCount, 0, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.Oracle, connectionString));
        var lstConns = new List<IConnection>(maxConnectionCount);
        for (var i = 0; i < pool.Connections.Length; ++i)
        {
            // copy  connections; to test it later
            var connection = pool.Connections[i];
            if (connection is not null) lstConns.Add(connection);
        }

        // act 
        var conn = pool.Get();
        pool.Unload();
        pool.Put(conn); 
        Thread.Sleep(500); // async close wait here 500 ms

        // assert
        Assert.Equal(int.MinValue, pool.Cursor);
        Assert.Equal(int.MinValue, pool.LastIndex);
        Assert.Equal(ConnectionState.Closed, conn.State);
        Assert.Equal(8, lstConns.Count);
        foreach (var connection in pool.Connections) Assert.Null(connection);
        foreach (var connection in lstConns) Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.True(pool.Unloaded());
    }

    [Fact]
    internal void Resize_ConnectionPool_ToBiggerConnectionPool()
    {
        // arrange 
        var maxConnectionCount = 6;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(ConnectionPoolExtensions.GetId(null), 5, maxConnectionCount, 0, connectionString);
        LogArrange($"Min pool size: {pool.MinConnection}; Max pool size: {pool.MaxConnection}");
        pool.Init(new ConnectionMock(1, DatabaseProvider.PostgreSql, connectionString));
        var lstConns = new List<IConnection>(maxConnectionCount);
        for (var i = 0; i < pool.Connections.Length; ++i)
        {
            // copy  connections; to test it later
            var connection = pool.Connections[i];
            if (connection is not null) lstConns.Add(connection);
        }

        // act 
        var conn1 = pool.Get();
        LogAct($"conn1 <== pool.Get()");
        var conn2 = pool.Get();
        LogAct($"conn2 <== pool.Get()");
        var newConnPool = pool.Resize(8, 10);
        LogAct($"pool.Resize() to: Min pool size: {newConnPool.MinConnection}; Max pool size: {newConnPool.MaxConnection}");
        pool.Put(conn2);
        pool.Put(conn1);
        Thread.Sleep(500); // async close wait here 500 ms

        // assert
        LogAssert($"Starting...");
        Assert.Equal(int.MinValue, pool.Cursor); // 1) check 'pool'
        Assert.Equal(int.MinValue, pool.LastIndex);
        Assert.Equal(ConnectionState.Closed, conn1.State);
        Assert.Equal(ConnectionState.Closed, conn2.State);
        Assert.Equal(2, pool.DestroyCount);
        Assert.Equal(5, lstConns.Count);
        foreach (var connection in pool.Connections) Assert.Null(connection);
        Assert.True(ReferenceEquals(lstConns[0], newConnPool.Connections[0])); // 2) check 'newConnPool'
        Assert.False(ReferenceEquals(lstConns[0], newConnPool.Connections[1]));
        Assert.False(ReferenceEquals(lstConns[0], newConnPool.Connections[2]));
        Assert.False(ReferenceEquals(lstConns[1], newConnPool.Connections[0]));
        Assert.True(ReferenceEquals(lstConns[1], newConnPool.Connections[1]));
        Assert.True(ReferenceEquals(lstConns[2], newConnPool.Connections[2]));
        Assert.Equal(7, newConnPool.Cursor);
        Assert.NotNull(newConnPool.Connections[3]);
        Assert.NotNull(newConnPool.Connections[4]);
        Assert.NotNull(newConnPool.Connections[5]);
        Assert.NotNull(newConnPool.Connections[6]);
        Assert.NotNull(newConnPool.Connections[7]);
        Assert.Null(newConnPool.Connections[8]);
        Assert.NotEqual(pool.Id, newConnPool.Id);
        Assert.Equal(1, newConnPool.ResizeCount);
    }


    [Fact]
    internal void Resize_ConnectionPool_ToSmallerConnectionPool()
    {
        // arrange 
        var maxConnectionCount = 11;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(ConnectionPoolExtensions.GetId(null), 8, maxConnectionCount, 0, connectionString);
        pool.Init(new ConnectionMock(1, DatabaseProvider.PostgreSql, connectionString));
        var lstConns = new List<IConnection>(maxConnectionCount);
        for (var i = 0; i < pool.Connections.Length; ++i)
        {
            // copy  connections; to test it later
            var connection = pool.Connections[i];
            if (connection is not null) lstConns.Add(connection);
        }

        // act 
        var conn1 = pool.Get();
        var newConnPool = pool.Resize(3, 5);
        pool.Put(conn1);
        Thread.Sleep(500); // async close wait here 500 ms

        // assert
        Assert.Equal(int.MinValue, pool.Cursor); // 1) check 'pool'
        Assert.Equal(int.MinValue, pool.LastIndex);
        Assert.Equal(ConnectionState.Closed, lstConns[5].State); // 3 connections closed
        Assert.Equal(ConnectionState.Closed, lstConns[6].State);
        Assert.Equal(ConnectionState.Closed, lstConns[7].State);
        Assert.Equal(1, pool.DestroyCount);
        Assert.Equal(8, lstConns.Count);
        foreach (var connection in pool.Connections) Assert.Null(connection);
        Assert.True(ReferenceEquals(lstConns[0], newConnPool.Connections[0])); // 2) check 'newConnPool'
        Assert.True(ReferenceEquals(lstConns[1], newConnPool.Connections[1])); // kept 5 first connection from previous ConnectionPool
        Assert.True(ReferenceEquals(lstConns[2], newConnPool.Connections[2]));
        Assert.True(ReferenceEquals(lstConns[3], newConnPool.Connections[3]));
        Assert.True(ReferenceEquals(lstConns[4], newConnPool.Connections[4]));
        Assert.Equal(4, newConnPool.Cursor);
    }

}
