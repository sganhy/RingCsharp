namespace Ring.Schema.Models;

internal struct CacheId
{
	internal readonly object SyncRoot;
	internal long CurrentId;
	internal long MaximumId;
	internal int ReservedRange;  // cache a range of id 

	/// <summary>
	///     Ctor
	/// </summary>
	public CacheId()
	{
		CurrentId = 0L;
		MaximumId = long.MaxValue;
		ReservedRange = 1;
		SyncRoot = new object();
	}
}