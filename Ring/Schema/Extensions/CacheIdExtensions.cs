using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ring.Schema.Extensions;

internal static class CacheIdExtensions
{
	/// <summary>
	/// 	Reserves <paramref name="count"/> ids without a lock.
	/// 	Returns false when the cache cannot serve them, which means a database reservation is needed.
	/// 	The reserved ids are firstId, firstId + Increment, ... firstId + (count - 1) * Increment.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryReserve(this CacheId cache, int count, out long firstId)
	{
		// Code size: 78 (0x4e) - no virual calls!
		long span = (long)count * cache.Increment;

		while (true)
		{
			long max = Volatile.Read(ref cache.MaximumId);   // maximum BEFORE current
			long cur = Volatile.Read(ref cache.CurrentId);

			if (span > max - cur)                            // not enough ids left: query needed
			{
				firstId = 0;
				return false;
			}

			if (Interlocked.CompareExchange(ref cache.CurrentId, cur + span, cur) == cur)
			{
				firstId = cur + cache.Increment;
				return true;                                 // served from cache, no query
			}
		}
	}

	/// <summary>
	/// 	Publishes a range read from the database. Only one publisher per CacheId at a time,
	/// 	and the new range must be above the previous one.
	/// </summary>
	internal static void Publish(this CacheId cache, long currentId, long maximumId, int reservedRange)
	{
		Volatile.Write(ref cache.ReservedRange, reservedRange); // informational, not read by the hot path
		Volatile.Write(ref cache.CurrentId, currentId);         // current BEFORE maximum
		Volatile.Write(ref cache.MaximumId, maximumId);
	}

/* Use case
	if (cache.TryReserve(count, out long first)) return first;   // 1. fast path, may fail
	await gate.WaitAsync();                                      // 2. one refill at a time per table
	try
	{
		if (cache.TryReserve(count, out first)) return first;    // 3. re-check: another thread may have refilled

		// 4. only now is a query really needed
		(long start, long max, int range) = await ReserveRangeInDatabase(table, Math.Max(count, defaultRange));
		cache.Publish(start, max, range);

		if (cache.TryReserve(count, out first)) return first;
		throw new InvalidOperationException("Refill returned too few ids.");
	}
	finally { gate.Release(); }
*/	
}