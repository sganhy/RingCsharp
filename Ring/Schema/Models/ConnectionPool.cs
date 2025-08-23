namespace Ring.Schema.Models;

internal sealed class ConnectionPool
{
	internal readonly int Id;
	internal readonly int MinConnection; // min 1
	internal readonly int MaxConnection; // min 1
	internal int Cursor;
	internal int SwapIndex;
	internal bool Disposed;
	internal ushort PutRequestCount;
	internal long CreationCount;
	internal long DestroyCount;
	internal int ConnectionCount;
	internal int LastIndex;
	internal readonly int ResizeCount;
	internal readonly IConnection?[] Connections;
	internal readonly string ConnectionString;
	internal readonly DateTime CreationTime;
	internal readonly object SyncRoot;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal ConnectionPool(int id, int minPoolSize, int maxPoolSize, int resizeCount, string connectionString)
	{
		Id = id;
		ConnectionCount = 0;
		CreationCount = 0L;
		DestroyCount = 0L;
		ResizeCount = resizeCount;
		CreationTime = DateTime.Now;
		SyncRoot = new object();
		MinConnection = minPoolSize;
		MaxConnection = maxPoolSize;
		Connections = new IConnection[maxPoolSize];
		Cursor = minPoolSize - 1; // cursor on min last element
		LastIndex = maxPoolSize - 1;
		SwapIndex = 0;
		ConnectionString = connectionString;
		PutRequestCount = 0;
		Disposed = false;
	}

}
