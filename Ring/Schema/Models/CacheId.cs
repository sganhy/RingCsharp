﻿namespace Ring.Schema.Models;

internal sealed class CacheId
{
	internal readonly object SyncRoot;
	internal long CurrentId;
	internal long MaximumId;
	internal int ReservedRange;  // cache a range of id 

	public CacheId()
	{
		CurrentId = 0L;
		MaximumId = long.MaxValue;
		ReservedRange = 1;
		SyncRoot = new object();
	}
}