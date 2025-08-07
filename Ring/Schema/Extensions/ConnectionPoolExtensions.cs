using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Data;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{

    internal static void Init(this ConnectionPool connectionPool, IConnection initialConnection)
    {
        connectionPool.Cursor = connectionPool.MinConnection - 1;     // cursor on min last element 
        connectionPool.ConnectionCount = 0;

        // close reference connection
        if (initialConnection.State == ConnectionState.Open) initialConnection.Close();
        var provider = initialConnection.ProviderId().ToDatabaseProvider();
        var minPoolSize = connectionPool.MinConnection;

        switch (provider)
        {
            case DatabaseProvider.SqlLite:
            case DatabaseProvider.Oracle:
            case DatabaseProvider.PostgreSql: 
                for (var i = 0; i < minPoolSize; ++i) connectionPool.Connections[i] = initialConnection.CreateInstance(i+1);
                connectionPool.ConnectionCount = minPoolSize;
                connectionPool.CreationCount = minPoolSize;
                break;
            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Retrieves an item from the pool. 
    /// </summary>
    /// <returns>The item retrieved from the pool.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IConnection Get(this ConnectionPool connectionPool)
    {
        // Code size: 72 (0x48)
        Monitor.Enter(connectionPool.SyncRoot); // start lock 
        if (connectionPool.Cursor >= 0)
        {
            var result = connectionPool.Connections[connectionPool.Cursor];
            --connectionPool.Cursor;
            Monitor.Exit(connectionPool.SyncRoot); // end lock 
            return result;
        }
        Monitor.Exit(connectionPool.SyncRoot); // end lock 
        return CreateConnection(connectionPool);
    }

    /// <summary>
    /// Places an item in the pool.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Put(this ConnectionPool connectionPool, IConnection connection)
    {
        // Code size: 154 (0x9a)
        ++connectionPool.PutRequestCount;
        Monitor.Enter(connectionPool.SyncRoot);     // start lock to lock before comparison (_cursor < _lastIndex) 
        if (connectionPool.Cursor < connectionPool.LastIndex)
        {
            
            ++connectionPool.Cursor;
            connectionPool.SwapIndex = connectionPool.Cursor != 0 ? connectionPool.PutRequestCount % connectionPool.Cursor : 0;
            // swap 
            connectionPool.Connections[connectionPool.Cursor] = connectionPool.Connections[connectionPool.SwapIndex];
            connectionPool.Connections[connectionPool.SwapIndex] = connection;
            Monitor.Exit(connectionPool.SyncRoot); // end lock 
            return;
        }
        Monitor.Exit(connectionPool.SyncRoot); // end lock 
        DestroyConnection(connectionPool, connection);
    }

    #region private methods 

    private static void DestroyConnection(this ConnectionPool connectionPool, IConnection connection)
    {
        // Code size: 50 (0x32)
        if (connection.State != ConnectionState.Closed) connection.Close();
        --connectionPool.ConnectionCount;
        ++connectionPool.DestroyCount;
        connection.Dispose();
    }

    private static IConnection CreateConnection(this ConnectionPool connectionPool)
    {
        // Code size: 59 (0x3b)
        var id = connectionPool.ConnectionCount + 1;
        var connection = connectionPool.Connections[0].CreateInstance(id);
        ++connectionPool.ConnectionCount;
        ++connectionPool.CreationCount;
        connection.Open();
        return connection;
    }

    #endregion 

}
