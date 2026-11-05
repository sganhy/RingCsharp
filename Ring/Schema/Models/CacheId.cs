namespace Ring.Schema.Models;

internal sealed class CacheId
{
	internal readonly object SyncRoot;
	internal long CurrentId;
	internal long MaximumId;
	internal int ReservedRange; // cache a range of id 
	readonly internal int Increment;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal CacheId(int idIncrement) : this (0L, long.MaxValue, 1, idIncrement) {}
	internal CacheId(long currentId, long maximumId, int reservedRange, int idIncrement)
	{
		CurrentId = currentId;
		MaximumId = maximumId;
		ReservedRange = reservedRange;
		Increment = idIncrement;
		SyncRoot = new object();
	}

#if DEBUG
	public override string ToString() => $"{CurrentId} / {MaximumId} (reserved: {ReservedRange}, increment: {Increment})";
#endif
}
