using Bogus;
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
    internal void Put_Connection_AvoidCrash()
    {
        // arrange 
        var maxConnectionCount = 100;
        var connectionString = _faker.Random.String();
        var pool = new ConnectionPool(_faker.Random.Number(), 10, maxConnectionCount, connectionString);
        pool.Init(new ConnectionMock(1, ConnectionState.Open, DatabaseProvider.SqlLite, connectionString));
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
            hashSet.Add(conn.Id);
        }
        Assert.Equal(255, pool.CreationCount);
        Assert.Equal(155, pool.DestroyCount);
    }

    public ConnectionPoolExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

}
