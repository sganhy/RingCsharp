using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{
	private static int _connectionPoolId;

	internal static int GetId(this ConnectionPool? _) => Interlocked.Increment(ref _connectionPoolId); // Code size: 11 (0xb) - first ConnectionPool id is equal to 1

	internal static Task InitAsync(this ConnectionPool connectionPool, IConnection initialConnection, CancellationToken cancellationToken=default)
	{
		// Code size: 62 (0x3e)
		// InitAsync() builds a Task[] with potentially null entries
		Initialize(connectionPool, initialConnection); // sync
		var minPoolSize = connectionPool.MinConnection;
		var tasks = new Task [minPoolSize];
		for (var i = 0; i < minPoolSize; ++i)
		{
			var conn = connectionPool.Connections[i];
			if (conn is not null) tasks[i]= conn.OpenAsync(cancellationToken);
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
		// Code size: 70 (0x46)
		IConnection? result = null;
		SpinEnter(ref connectionPool.SpinLock); // replacing Monitor.Enter(connectionPool.SyncRoot);
		if (connectionPool.Cursor >= 0)	result = connectionPool.Connections[connectionPool.Cursor--];
		SpinExit(ref connectionPool.SpinLock);
		return result ?? CreateConnection(connectionPool);
	}

	/// <summary>
	/// 	Places an item in the pool. semi async destroy is computed async
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Put(this ConnectionPool connectionPool, IConnection connection)
	{
		// Code size: 155 (0x9b)
		SpinEnter(ref connectionPool.SpinLock);
		if (connectionPool.Cursor < connectionPool.LastIndex)
		{
			++connectionPool.PutRequestCount;
			++connectionPool.Cursor;
			connectionPool.SwapIndex = connectionPool.Cursor != 0 ? connectionPool.PutRequestCount % connectionPool.Cursor : 0;
			connectionPool.Connections[connectionPool.Cursor] = connectionPool.Connections[connectionPool.SwapIndex];
			connectionPool.Connections[connectionPool.SwapIndex] = connection;
			SpinExit(ref connectionPool.SpinLock);
			return;
		}
		SpinExit(ref connectionPool.SpinLock);
		DestroyConnectionAsync(connectionPool, connection);
	}

	/// <summary>
	///     Places an item in the pool. If the pool is full, the connection is
	///     destroyed asynchronously. Returns a completed ValueTask on the hot path
	///     (pool has space) — no allocation in that case.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ValueTask PutAsync(this ConnectionPool connectionPool, IConnection connection, CancellationToken cancellationToken = default)
	{
		// Code size: 161 (0xa1)
		SpinEnter(ref connectionPool.SpinLock);
		if (connectionPool.Cursor < connectionPool.LastIndex)
		{
			++connectionPool.PutRequestCount;
			++connectionPool.Cursor;
			connectionPool.SwapIndex = connectionPool.Cursor != 0 ? connectionPool.PutRequestCount % connectionPool.Cursor : 0;
			connectionPool.Connections[connectionPool.Cursor] = connectionPool.Connections[connectionPool.SwapIndex];
			connectionPool.Connections[connectionPool.SwapIndex] = connection;
			SpinExit(ref connectionPool.SpinLock);
			return ValueTask.CompletedTask; // ← zero allocation, synchronous return
		}
		SpinExit(ref connectionPool.SpinLock);
		return DestroyConnectionAsync(connectionPool, connection, cancellationToken); // ← cold path only
	}


	internal static bool Unloaded(this ConnectionPool connectionPool) => connectionPool.Cursor == int.MinValue || connectionPool.LastIndex == int.MinValue; // Code size: 29 (0x1d)

	internal static void Unload(this ConnectionPool connectionPool)
	{
		// Code size: 126 (0x7e)
		SpinEnter(ref connectionPool.SpinLock);  // was: Monitor.Enter(connectionPool.SyncRoot)
		var currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue;
		connectionPool.LastIndex = int.MinValue;
		SpinExit(ref connectionPool.SpinLock);   // was: Monitor.Exit(connectionPool.SyncRoot)

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

	internal static void CheckHealth(this ConnectionPool connectionPool)
	{
		// verify connection & creation time

	}

	internal static ConnectionPool Resize(this ConnectionPool connectionPool, int minPoolSize, int maxPoolSize)
	{
		// Code size: 295 (0x127)
		if (minPoolSize == connectionPool.MinConnection && maxPoolSize == connectionPool.MaxConnection) return connectionPool;
		SpinEnter(ref connectionPool.SpinLock);  // was: Monitor.Enter(connectionPool.SyncRoot)
		var currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue;
		connectionPool.LastIndex = int.MinValue;
		SpinExit(ref connectionPool.SpinLock);   // was: Monitor.Exit(connectionPool.SyncRoot)
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
		// Code size: 82 (0x52)
		if (connection.State != ConnectionState.Closed)	ThreadPool.QueueUserWorkItem(_ => { connection.Close(); connection.Dispose(); });
		else connection.Dispose();
		Interlocked.Decrement(ref connectionPool.ConnectionCount);
		Interlocked.Increment(ref connectionPool.DestroyCount);
	}

	private static IConnection CreateConnection(this ConnectionPool connectionPool)
	{
		// Code size: 56 (0x38)
		var id = Interlocked.Increment(ref connectionPool.ConnectionCount);
		Interlocked.Increment(ref connectionPool.CreationCount);
		var connection = connectionPool.Connections[0];
		if (connection is not null)
		{
			var newConnection = connection.CreateInstance(id);
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SpinEnter(ref int spinLock)
	{
		// Code size: 19 (0x13)
		while (Interlocked.CompareExchange(ref spinLock, 1, 0) != 0) Thread.SpinWait(1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SpinExit(ref int spinLock) => Volatile.Write(ref spinLock, 0); // Code size: 8 (0x8)

	private static async ValueTask DestroyConnectionAsync(this ConnectionPool connectionPool, IConnection connection, CancellationToken cancellationToken)
	{
		// Code size: 71 (0x47)
		if (connection.State != ConnectionState.Closed)	await connection.CloseAsync(cancellationToken).ConfigureAwait(false);
		connection.Dispose(); // no DisposeAsync; may be should be implemented 
		Interlocked.Decrement(ref connectionPool.ConnectionCount); // stat value is decremented synchronously, even if the actual close/dispose is done asynchronously, to avoid over-provisioning of connections in the pool
		Interlocked.Increment(ref connectionPool.DestroyCount);
	}

	#endregion

}
