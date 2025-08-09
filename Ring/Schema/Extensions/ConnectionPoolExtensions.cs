using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Data;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{
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
		// Code size: 77 (0x4d) - no callvirt
		Monitor.Enter(connectionPool.SyncRoot); // start lock 
		if (connectionPool.Cursor >= 0)
		{
			var result = connectionPool.Connections[connectionPool.Cursor];
			--connectionPool.Cursor;
			Monitor.Exit(connectionPool.SyncRoot); // end lock as soon as possible!
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
		// Code size: 155 (0x9b) - no callvirt
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
		Monitor.Enter(connectionPool.SyncRoot);	 // start lock to lock before comparison (_cursor < _lastIndex) 
		var currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue;		// avoid to stack new connections & finalize last executions
		connectionPool.LastIndex = int.MinValue;
		Monitor.Exit(connectionPool.SyncRoot);	  // end lock 

		var span = new Span<IConnection?>(connectionPool.Connections);
		for (var i=0; i< span.Length; ++i)
		{
			var conn = span[i];
			if (conn!=null && conn.State!=ConnectionState.Closed && i< currentCursor)
			{
				conn.Close();
				conn.Dispose();
			}
			span[i] = null;
		}
	}

	public static void CheckHealth(this ConnectionPool connectionPool)
	{
		// verify connection & creation time

	}

	public static ConnectionPool Resize(this ConnectionPool connectionPool, int minPoolSize, int maxPoolSize)
	{
		// verify connection & creation time
		return new ConnectionPool(connectionPool.Id, minPoolSize, maxPoolSize, connectionPool.ConnectionString);
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
		// Code size: 141 (0x8d)
		connectionPool.Cursor = connectionPool.MinConnection - 1; // cursor on min last element 
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

	#endregion

}
