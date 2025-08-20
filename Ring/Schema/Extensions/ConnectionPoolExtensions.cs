using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Data;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{
	private static int _connectionPoolId = 1;

	internal static int GetId(this ConnectionPool? _) => _connectionPoolId++; // Code size: 14 (0xe)

	internal static Task InitAsync(this ConnectionPool connectionPool, IConnection initialConnection, CancellationToken cancellationToken=default)
	{
		// Code size: 62 (0x3e)
		Initialize(connectionPool, initialConnection); // sync
		var minPoolSize = connectionPool.MinConnection;
		var tasks = new Task [minPoolSize];
		for (var i = 0; i < minPoolSize; ++i)
		{
			var conn = connectionPool.Connections[i];
			if (conn != null) tasks[i]= conn.OpenAsync(cancellationToken);
		}
		return Task.WhenAll(tasks);
	}

	internal static void Init(this ConnectionPool connectionPool, IConnection initialConnection)
	{
		// Code size: 46 (0x2e)
		Initialize(connectionPool, initialConnection); // sync
		var minPoolSize = connectionPool.MinConnection;
		for (var i = 0; i < minPoolSize; ++i)
		{
			var conn = connectionPool.Connections[i];
			conn?.Open();
		}
	}

	/// <summary>
	/// 	Retrieves an item from the pool.
	/// </summary>
	/// <returns>The item retrieved from the pool.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IConnection Get(this ConnectionPool connectionPool)
	{
        // Code size: 77 (0x4d) -  no virtual call
        Monitor.Enter(connectionPool.SyncRoot); // start lock 
		if (connectionPool.Cursor >= 0)
		{
			var result = connectionPool.Connections[connectionPool.Cursor];
			--connectionPool.Cursor;
			Monitor.Exit(connectionPool.SyncRoot); // end lock as fast as possible!
			return result;
		}
		Monitor.Exit(connectionPool.SyncRoot); // end lock 
		return CreateConnection(connectionPool);
	}

	/// <summary>
	/// 	Places an item in the pool. semi async destroy is computed async
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Put(this ConnectionPool connectionPool, IConnection connection)
	{
        // Code size: 155 (0x9b) -  no virtual call
        ++connectionPool.PutRequestCount; // out of lock!!!
		Monitor.Enter(connectionPool.SyncRoot);	 // start lock to lock before comparison (_cursor < _lastIndex) 
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
		DestroyConnectionAsync(connectionPool, connection);
	}

	public static bool Unloaded(this ConnectionPool connectionPool) => connectionPool.Cursor == int.MinValue || connectionPool.LastIndex == int.MinValue; // Code size: 29 (0x1d)

	public static void Unload(this ConnectionPool connectionPool)
	{
		// Code size: 126 (0x7e)
		Monitor.Enter(connectionPool.SyncRoot);	 // start lock to lock before comparison (_cursor < _lastIndex) 
		var currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue;		// avoid to stack new connections & finalize last executions
		connectionPool.LastIndex = int.MinValue;
		Monitor.Exit(connectionPool.SyncRoot);	  // end lock 

		var span = new Span<IConnection?>(connectionPool.Connections);
		for (var i=0; i< span.Length; ++i)
		{
			var conn = span[i];
			if (conn!=null && conn.State!=ConnectionState.Closed && i<= currentCursor)
			{
				conn.Close();
				conn.Dispose();
			}
		}
		connectionPool.ClearConnections();
	}

	public static void CheckHealth(this ConnectionPool connectionPool)
	{
		// verify connection & creation time

	}

	public static ConnectionPool Resize(this ConnectionPool connectionPool, int minPoolSize, int maxPoolSize)
	{
		// Code size: 301 (0x12d)
		if (minPoolSize == connectionPool.MinConnection && maxPoolSize == connectionPool.MaxConnection) return connectionPool;
		Monitor.Enter(connectionPool.SyncRoot); // start lock to lock before comparison (_cursor < _lastIndex) 
		var currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue; // avoid to stack new connections & finalize last executions
		connectionPool.LastIndex = int.MinValue;
		Monitor.Exit(connectionPool.SyncRoot); // end lock 
		var newPool = new ConnectionPool(GetId(connectionPool), minPoolSize, maxPoolSize, connectionPool.ResizeCount + 1, connectionPool.ConnectionString)
		{
			ConnectionCount = connectionPool.ConnectionCount,
			DestroyCount = connectionPool.DestroyCount,
			CreationCount = connectionPool.CreationCount,
		};
		
		var preserveCount = Math.Min(currentCursor, newPool.LastIndex); // close as less as possible connections

		// preserve current openned connections
		var i = 0;
		for (i = 0; i <= preserveCount; ++i) newPool.Connections[i] = connectionPool.Connections[i];
		var initialConnection = newPool.Connections[0];
		// bigger pool ?
		for (; i < minPoolSize; ++i)
		{
			var conn = initialConnection?.CreateInstance(i+1);
			conn?.Open();
			newPool.Connections[i] = conn;
		}
		// smaller pool ?
		for (i= preserveCount + 1; i<= currentCursor; ++i)
		{
			var conn = connectionPool.Connections[i];
			conn?.Close();
		}
		if (preserveCount > newPool.Cursor) newPool.Cursor = preserveCount;
		connectionPool.ClearConnections();
		return newPool;
	}

	#region private methods 

	private static void DestroyConnectionAsync(this ConnectionPool connectionPool, IConnection connection)
	{
		// Code size: 50 (0x32)
		if (connection.State != ConnectionState.Closed)
		{
			ThreadPool.QueueUserWorkItem((state) => connection.Close()); // crash ??
			//connection.Close();
		}
		--connectionPool.ConnectionCount;
		++connectionPool.DestroyCount;
		connection.Dispose();
	}

	private static IConnection CreateConnection(this ConnectionPool connectionPool)
	{
		// Code size: 70 (0x46)
		var id = connectionPool.ConnectionCount + 1;
		var connection = connectionPool.Connections[0];
		if (connection != null)
		{
			var newConnection = connection.CreateInstance(id);
			++connectionPool.ConnectionCount;
			++connectionPool.CreationCount;
			newConnection.Open();
			return newConnection;
		}
		throw new NotSupportedException();
	}

	private static void Initialize(this ConnectionPool connectionPool, IConnection initialConnection)
	{
		// Code size: 102 (0x66)
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
				for (var i = 0; i < minPoolSize; ++i) connectionPool.Connections[i] = initialConnection.CreateInstance(i + 1);

				connectionPool.ConnectionCount = minPoolSize;
				connectionPool.CreationCount = minPoolSize;
				break;
			default:
				throw new NotSupportedException();
		}
	}

	private static void ClearConnections(this ConnectionPool connectionPool)
	{
		// Code size: 42 (0x2a)
		var span = new Span<IConnection?>(connectionPool.Connections);
		for (var i = 0; i < span.Length; ++i) span[i] = null;
	}

	#endregion

}
