using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Data;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{

    internal static void Init(this ConnectionPool connectionPool, IConnection initialConnection)
    {
        connectionPool.Cursor = connectionPool.MinConnection - 1;     // cursor on min last element 
        connectionPool.CreationCount = 0;
        connectionPool.LastIndex = connectionPool.MaxConnection - 1;

        // close reference connection
        if (initialConnection.State == ConnectionState.Open) initialConnection.Close();
        var provider = initialConnection.ProviderId().ToDatabaseProvider();
        var minPoolSize = connectionPool.MinConnection;

        switch (provider)
        {
            case DatabaseProvider.SqlLite:
            case DatabaseProvider.Oracle:
            case DatabaseProvider.PostgreSql: 
                for (var i = 0; i < minPoolSize; ++i) 
                    connectionPool.Connections[i] = initialConnection.CreateInstance();
                break;
            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Retrieves an item from the pool. 
    /// </summary>
    /// <returns>The item retrieved from the pool.</returns>
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
        return null; //connectionPool.CreateConnection();
    }

    /// <summary>
    /// Places an item in the pool.
    /// </summary>
    public static void Put(this ConnectionPool connectionPool, IConnection connection)
    {
        // Code size: 154 (0x9a)
        Monitor.Enter(connectionPool.SyncRoot);     // start lock to lock before comparison (_cursor < _lastIndex) 
        if (connectionPool.Cursor < connectionPool.LastIndex)
        {
            ++connectionPool.PutRequestCount;
            ++connectionPool.Cursor;
            connectionPool.SwapIndex = connectionPool.Cursor != 0 ? connectionPool.PutRequestCount % connectionPool.Cursor : 0;
            // swap 
            connectionPool.Connections[connectionPool.Cursor] = connectionPool.Connections[connectionPool.SwapIndex];
            connectionPool.Connections[connectionPool.SwapIndex] = connection;
            Monitor.Exit(connectionPool.SyncRoot); // end lock 
            return;
        }
        Monitor.Exit(connectionPool.SyncRoot); // end lock 
        DestroyConnection(connection);
    }

    #region private methods 

    private static void DestroyConnection(IConnection connection)
    {
        if (connection.State != ConnectionState.Closed) connection.Close();
        connection.Dispose();
    }

    #endregion 

}
