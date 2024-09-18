using Ring.Data;
using System.Data;

namespace Ring.Schema.Models;

internal sealed class ConnectionPool
{
	internal readonly int Id;
	internal readonly int MinConnection; // min 1
	internal readonly int MaxConnection; // min 1
	internal int Cursor;
	internal int SwapIndex;
	internal int LastIndex;
	internal ushort PutRequestCount;
	internal long CreationCount; // db connection creation count 
	internal readonly IRingConnection[] Connections;
	internal readonly string ConnectionString;
	internal readonly object SyncRoot;


	/// <summary>
	///     Ctor
	/// </summary>
	internal ConnectionPool(int id, int minPoolSize, int maxPoolSize, string connectionString)
	{
		Id = id;
		CreationCount = 0L;
		SyncRoot = new object();
		MinConnection = minPoolSize;
		MaxConnection = maxPoolSize;
		Connections = new IRingConnection[maxPoolSize];
		Cursor = minPoolSize - 1;     // cursor on min last element 
		LastIndex = maxPoolSize - 1;
		SwapIndex = 0;
		ConnectionString = connectionString;
		PutRequestCount = 0;
	}
}
