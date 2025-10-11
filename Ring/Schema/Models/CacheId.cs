namespace Ring.Schema.Models;

internal sealed class CacheId
{
	internal readonly object SyncRoot;
	internal long CurrentId;
	internal long MaximumId;
	internal int ReservedRange; // cache a range of id 

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal CacheId()
	{
		CurrentId = 0L;
		MaximumId = long.MaxValue;
		ReservedRange = 1;
		SyncRoot = new object();
	}
	internal CacheId(long currentId, long maximumId, int reservedRange)
	{
		CurrentId = currentId;
		MaximumId = maximumId;
		ReservedRange = reservedRange;
		SyncRoot = new object();
	}

#if DEBUG
	public override string ToString() => $"{CurrentId} / {MaximumId} (reserved: {ReservedRange})";
#endif
}
