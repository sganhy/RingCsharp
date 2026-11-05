namespace Ring.Schema.Models;

internal sealed class CacheId
{
	// Size per instance: 40 bytes as a class on 64-bit (16-byte object header + method table pointer, 24 bytes of fields), no padding.
	// Fields: CurrentId 8 + MaximumId 8 + ReservedRange 4 + Increment 4 = 24 bytes, all naturally aligned.
	// No separate lock object: the monitor uses the instance's own header, so the full graph is also 40 bytes.
	internal long CurrentId;
	internal long MaximumId;
	internal int ReservedRange; // cache a range of id 
	readonly internal int Increment;

	internal CacheId(int idIncrement) : this(0L, long.MaxValue, 1, idIncrement) { }
	internal CacheId(long currentId, long maximumId, int reservedRange, int idIncrement)
	{
		CurrentId = currentId;
		MaximumId = maximumId;
		ReservedRange = reservedRange;
		Increment = idIncrement;
	}

#if DEBUG
	public override string ToString() => $"{CurrentId} / {MaximumId} (reserved: {ReservedRange}, increment: {Increment})";
#endif
}
