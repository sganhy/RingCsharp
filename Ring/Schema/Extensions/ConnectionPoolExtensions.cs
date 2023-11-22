using Ring.Schema.Models;
using System.Data;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{
    internal static void Init(this ConnectionPool connectionPool)
    {
        for (var i=0; i < connectionPool.MinConnection; ++i)
            connectionPool.Connections[i] = connectionPool.CreateConnection();
    }

    /// <summary>
    /// Retrieves an item from the pool. 
    /// </summary>
    /// <returns>The item retrieved from the pool.</returns>
    internal static IDbConnection Get(this ConnectionPool connectionPool)
    {
        // no lock here !!!!
        Monitor.Enter(connectionPool.SyncRoot); // start lock 
        if (connectionPool.Cursor >= 0)
        {
            var result = connectionPool.Connections[connectionPool.Cursor];
            --connectionPool.Cursor;
            Monitor.Exit(connectionPool.SyncRoot); // end lock 
            return result;
        }
        Monitor.Exit(connectionPool.SyncRoot); // end lock 
        return connectionPool.CreateConnection();
    }

    /// <summary>
    /// Places an item in the pool.
    /// </summary>
    public static void Put(this ConnectionPool connectionPool, IDbConnection connection)
    {
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
    private static IDbConnection CreateConnection(this ConnectionPool connectionPool)
    {
        ++connectionPool.CreationCount;
        var instance = Activator.CreateInstance(connectionPool.ConnectionType);
        if (instance!=null)
        {
            var newConn = (IDbConnection)instance;
            newConn.ConnectionString = connectionPool.ConnectionString;
            newConn.Open();
            return newConn;
        }
        throw new ArgumentException($"Impossible to create IDbConnection instance from type ({connectionPool.ConnectionType.FullName})");
    }

    private static void DestroyConnection(IDbConnection connection)
    {
        if (connection.State != ConnectionState.Closed) connection.Close();
        connection.Dispose();
    }

    #endregion 

}
