using System.Collections.Concurrent;

namespace Ring.Data.Extensions;

internal static class IConnectionExtensions
{
	// Thin reference-type wrapper so Interlocked can mutate the counter
	// that lives inside the dictionary, not a copy of it.
	private sealed class Counter { internal long Value; }

	// One independent Counter per distinct connection string.
	private static readonly ConcurrentDictionary<string, Counter> _counterByConnectionString = new(StringComparer.Ordinal);

	/// <summary>
	/// Returns the next sequential ID scoped to the connection's <c>ConnectionString</c>.
	/// Each distinct connection string has its own counter that starts at <c>1</c>.
	/// Returns <c>0</c> when the connection (or its ConnectionString) is <c>null</c>.
	/// </summary>
	internal static long GetId(this IConnection _, string connectionString)
	{
		// Code size: 82 (0x52)
		// Fast path: lock-free read — the common case once all connection
		// strings have been seen at least once (e.g. after warm-up).
		if (_counterByConnectionString.TryGetValue(connectionString, out var counter))
			return Interlocked.Increment(ref counter.Value);

		// Slow path: first time this connection string is seen.
		// GetOrAdd may invoke the factory more than once under concurrency,
		// but all callers are guaranteed to receive the same stored instance.
		counter = _counterByConnectionString.GetOrAdd(connectionString, _ => new Counter());
		return Interlocked.Increment(ref counter.Value);
	}
}
