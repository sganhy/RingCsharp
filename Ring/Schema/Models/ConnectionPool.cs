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
	internal int ConnectionCount;
	internal long CreationCount;
	internal long DestroyCount;
	internal readonly int LastIndex;
	internal readonly IConnection[] Connections;
	internal readonly string ConnectionString;
	internal readonly object SyncRoot;


	/// <summary>
	/// 	Ctor
	/// </summary>
	internal ConnectionPool(int id, int minPoolSize, int maxPoolSize, string connectionString)
	{
		Id = id;
		ConnectionCount = 0;
		CreationCount = 0L;
		DestroyCount = 0L;
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
