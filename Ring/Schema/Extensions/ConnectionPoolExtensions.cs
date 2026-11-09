using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ring.Schema.Extensions;

internal static class ConnectionPoolExtensions
{
	private const int SqlSendBufferSize = 1024;
	private static int _connectionPoolId;

	internal static int GetId(this ConnectionPool? _) => Interlocked.Increment(ref _connectionPoolId);

	internal static async ValueTask InitAsync(this ConnectionPool connectionPool, IConnection initialConnection, CancellationToken cancellationToken = default)
	{
		Initialize(connectionPool, initialConnection);

		int minPoolSize = connectionPool.MinConnection;
		if (minPoolSize == 0) return;
		var connections = connectionPool.Connections;

		await Parallel.ForEachAsync(connections.Take(minPoolSize).OfType<IConnection>(), cancellationToken, async (conn, ct) =>
		{
			await conn.OpenAsync(ct).ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	internal static void Init(this ConnectionPool connectionPool, IConnection initialConnection)
	{
		// Code size: 58 (0x3a)
		Initialize(connectionPool, initialConnection);

		int minPoolSize = connectionPool.MinConnection;
		var connections = connectionPool.Connections;
		ref var baseRef = ref MemoryMarshal.GetArrayDataReference(connections);

		for (int i = 0; i < minPoolSize; ++i)
		{
			var conn = Unsafe.Add(ref baseRef, i);
			conn?.Open();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IConnection Get(this ConnectionPool connectionPool)
	{
		// Code size: 75 (0x4b)
		IConnection? result = null;
		SpinEnter(ref connectionPool.SpinLock);
		int cursor = connectionPool.Cursor;
		if (cursor >= 0)
		{
			var connections = connectionPool.Connections;
			result = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(connections), cursor);
			connectionPool.Cursor = cursor - 1;
		}
		SpinExit(ref connectionPool.SpinLock);
		return result ?? CreateConnection(connectionPool);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Put(this ConnectionPool connectionPool, IConnection connection)
	{
		// Code size: 137 (0x89)
		SpinEnter(ref connectionPool.SpinLock);
		int cursor = connectionPool.Cursor;

		if (cursor < connectionPool.LastIndex)
		{
			ushort putCount = (ushort)(connectionPool.PutRequestCount + 1);
			int newCursor = cursor + 1;
			int swapIndex = newCursor != 0 ? putCount % newCursor : 0;

			connectionPool.PutRequestCount = putCount;
			connectionPool.Cursor = newCursor;
			connectionPool.SwapIndex = swapIndex;

			var connections = connectionPool.Connections;
			ref var baseRef = ref MemoryMarshal.GetArrayDataReference(connections);
			ref var swapRef = ref Unsafe.Add(ref baseRef, swapIndex);
			ref var cursorRef = ref Unsafe.Add(ref baseRef, newCursor);

			cursorRef = swapRef;
			swapRef = connection;

			SpinExit(ref connectionPool.SpinLock);
			return;
		}

		SpinExit(ref connectionPool.SpinLock);
		DestroyConnectionAsync(connectionPool, connection);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ValueTask PutAsync(this ConnectionPool connectionPool, IConnection connection, CancellationToken cancellationToken = default)
	{
		SpinEnter(ref connectionPool.SpinLock);
		int cursor = connectionPool.Cursor;
		if (cursor < connectionPool.LastIndex)
		{
			ushort putCount = (ushort)(connectionPool.PutRequestCount + 1);
			int newCursor = cursor + 1;
			int swapIndex = newCursor != 0 ? putCount % newCursor : 0;

			connectionPool.PutRequestCount = putCount;
			connectionPool.Cursor = newCursor;
			connectionPool.SwapIndex = swapIndex;

			var connections = connectionPool.Connections;
			ref var baseRef = ref MemoryMarshal.GetArrayDataReference(connections);
			ref var swapRef = ref Unsafe.Add(ref baseRef, swapIndex);
			ref var cursorRef = ref Unsafe.Add(ref baseRef, newCursor);

			cursorRef = swapRef;
			swapRef = connection;

			SpinExit(ref connectionPool.SpinLock);
			return ValueTask.CompletedTask;
		}
		SpinExit(ref connectionPool.SpinLock);
		return DestroyConnectionAsync(connectionPool, connection, cancellationToken);
	}

	internal static bool Unloaded(this ConnectionPool connectionPool) => connectionPool.Cursor == int.MinValue || connectionPool.LastIndex == int.MinValue;

	internal static void Unload(this ConnectionPool connectionPool)
	{
		SpinEnter(ref connectionPool.SpinLock);
		int currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue;
		connectionPool.LastIndex = int.MinValue;
		SpinExit(ref connectionPool.SpinLock);

		var connections = connectionPool.Connections;
		ref var baseRef = ref MemoryMarshal.GetArrayDataReference(connections);
		int length = connections.Length;

		for (int i = 0; i < length; ++i)
		{
			ref var conn = ref Unsafe.Add(ref baseRef, i);
			if (conn is not null && conn.State != ConnectionState.Closed && i <= currentCursor)
			{
				conn.Close();
				conn.Dispose();
			}
			conn = null;
		}
	}

	internal static void CheckHealth(this ConnectionPool connectionPool)
	{
		// Verify connection liveness and creation time if needed
	}

	internal static ConnectionPool Resize(this ConnectionPool connectionPool, int minPoolSize, int maxPoolSize)
	{
		if (minPoolSize == connectionPool.MinConnection && maxPoolSize == connectionPool.MaxConnection) return connectionPool;

		SpinEnter(ref connectionPool.SpinLock);
		int currentCursor = connectionPool.Cursor;
		connectionPool.Cursor = int.MinValue;
		connectionPool.LastIndex = int.MinValue;
		SpinExit(ref connectionPool.SpinLock);

		var newPool = new ConnectionPool(GetId(connectionPool), minPoolSize, maxPoolSize, connectionPool.ResizeCount + 1, connectionPool.ConnectionString)
		{
			ConnectionCount = connectionPool.ConnectionCount,
			DestroyCount = connectionPool.DestroyCount,
			CreationCount = connectionPool.CreationCount,
		};

		int preserveCount = Math.Min(currentCursor, newPool.LastIndex);

		var oldConns = connectionPool.Connections;
		var newConns = newPool.Connections;
		ref var oldBase = ref MemoryMarshal.GetArrayDataReference(oldConns);
		ref var newBase = ref MemoryMarshal.GetArrayDataReference(newConns);

		int i = 0;
		// Preserve open connections
		for (i = 0; i <= preserveCount; ++i)
		{
			Unsafe.Add(ref newBase, i) = Unsafe.Add(ref oldBase, i);
		}

		var initialConnection = preserveCount >= 0 ? Unsafe.Add(ref newBase, 0) : null;

		// Expand pool if necessary
		for (; i < minPoolSize; ++i)
		{
			var conn = initialConnection?.CreateInstance(i + 1, SqlSendBufferSize);
			conn?.Open();
			Unsafe.Add(ref newBase, i) = conn;
		}

		// Close surplus connections safely if shrunk
		for (i = preserveCount + 1; i <= currentCursor; ++i)
		{
			var conn = Unsafe.Add(ref oldBase, i);
			if (conn is not null)
			{
				if (conn.State != ConnectionState.Closed) conn.Close();
				conn.Dispose();
			}
		}

		if (preserveCount > newPool.Cursor)
			newPool.Cursor = preserveCount;

		connectionPool.ClearConnections();
		return newPool;
	}

	#region private methods 

	private static void DestroyConnectionAsync(this ConnectionPool connectionPool, IConnection connection)
	{
		// Code size: 81 (0x51)
		if (connection.State != ConnectionState.Closed)
		{
			ThreadPool.QueueUserWorkItem(static state =>
			{
				var conn = (IConnection)state!;
				conn.Close();
				conn.Dispose();
			}, connection, preferLocal: false);
		}
		else
		{
			connection.Dispose();
		}

		Interlocked.Decrement(ref connectionPool.ConnectionCount);
		Interlocked.Increment(ref connectionPool.DestroyCount);
	}

	private static IConnection CreateConnection(this ConnectionPool connectionPool)
	{
		var id = Interlocked.Increment(ref connectionPool.ConnectionCount);
		Interlocked.Increment(ref connectionPool.CreationCount);
		var connections = connectionPool.Connections;
		var connection = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(connections), 0);

		if (connection is not null)
		{
			var newConnection = connection.CreateInstance(id, SqlSendBufferSize);
			newConnection.Open();
			return newConnection;
		}
		throw new NotSupportedException(ResourceHelper.GetMessage(ResourceType.InitConnectionInPoolIsNull));
	}

	private static void Initialize(this ConnectionPool connectionPool, IConnection initialConnection)
	{
		connectionPool.ConnectionCount = 0;

		if (initialConnection.State == ConnectionState.Open) initialConnection.Close();

		var provider = initialConnection.ProviderId.ToDatabaseProvider();
		int minPoolSize = connectionPool.MinConnection;

		switch (provider)
		{
			case DatabaseProvider.SqlLite:
			case DatabaseProvider.Oracle:
			case DatabaseProvider.PostgreSql:
				var connections = connectionPool.Connections;
				ref var baseRef = ref MemoryMarshal.GetArrayDataReference(connections);

				for (int i = 0; i < minPoolSize; ++i)
				{
					Unsafe.Add(ref baseRef, i) = initialConnection.CreateInstance(i + 1, SqlSendBufferSize);
				}

				connectionPool.ConnectionCount = minPoolSize;
				connectionPool.CreationCount = minPoolSize;
				break;
			default:
				throw new NotSupportedException();
		}
	}

	private static void ClearConnections(this ConnectionPool connectionPool)
	{
		var connections = connectionPool.Connections;
		Array.Clear(connections, 0, connections.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SpinEnter(ref int spinLock)
	{
		var spinWait = new SpinWait();
		while (Interlocked.CompareExchange(ref spinLock, 1, 0) != 0)
		{
			spinWait.SpinOnce();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SpinExit(ref int spinLock) => Volatile.Write(ref spinLock, 0);

	private static async ValueTask DestroyConnectionAsync(this ConnectionPool connectionPool, IConnection connection, CancellationToken cancellationToken)
	{
		if (connection.State != ConnectionState.Closed) await connection.CloseAsync(cancellationToken).ConfigureAwait(false);
		connection.Dispose();
		Interlocked.Decrement(ref connectionPool.ConnectionCount);
		Interlocked.Increment(ref connectionPool.DestroyCount);
	}

	#endregion
}