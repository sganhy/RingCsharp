using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Ring.Logging;

/// <summary>
/// Lightweight publish-subscribe service for log events.
/// QoS 1: At-most-once delivery - fire and forget, no buffering, no guarantees.
/// </summary>
internal sealed class LogSubscriptionService : IDisposable
{
	private readonly ConcurrentDictionary<int, SubscriptionEntry> _subscriptions;
	private int _subscriptionIdCounter;
	private int _disposed;

	internal LogSubscriptionService()
	{
		_subscriptions = new ConcurrentDictionary<int, SubscriptionEntry>();
		_subscriptionIdCounter = 0;
		_disposed = 0;
	}

	/// <summary>
	/// Gets the number of active subscriptions.
	/// </summary>
	internal int SubscriptionCount => _subscriptions.Count;

	/// <summary>
	/// Returns true if there are any active subscribers.
	/// </summary>
	internal bool HasSubscribers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => !_subscriptions.IsEmpty;
	}

	/// <summary>
	/// Subscribes a log subscriber.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal IDisposable Subscribe(ILogSubscriber subscriber)
	{
		var id = Interlocked.Increment(ref _subscriptionIdCounter);
		_subscriptions.TryAdd(id, new SubscriptionEntry(id, subscriber));
		return new SubscriptionToken(this, id);
	}

	/// <summary>
	/// Subscribes using a delegate.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal IDisposable Subscribe(Action<LogEvent> handler, LogLevel minLevel = LogLevel.Trace, string? categoryFilter = null)
	{
		return Subscribe(new DelegateLogSubscriber(handler, minLevel, categoryFilter));
	}

	/// <summary>
	/// Publishes a log event to all subscribers. Fire and forget - no buffering.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Publish(LogEvent logEvent, string category)
	{
		foreach (var kvp in _subscriptions)
		{
			var subscriber = kvp.Value.Subscriber;
			if (logEvent.Level >= subscriber.MinLevel &&
				(subscriber.CategoryFilter is null || category.StartsWith(subscriber.CategoryFilter, StringComparison.Ordinal)))
			{
				try { subscriber.OnLogEvent(logEvent); } catch { }
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Unsubscribe(int id) => _subscriptions.TryRemove(id, out _);

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
		_subscriptions.Clear();
	}

	#region nested types

	private readonly struct SubscriptionEntry
	{
		internal readonly int Id;
		internal readonly ILogSubscriber Subscriber;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal SubscriptionEntry(int id, ILogSubscriber subscriber)
		{
			Id = id;
			Subscriber = subscriber;
		}
	}

	private sealed class SubscriptionToken : IDisposable
	{
		private readonly LogSubscriptionService _service;
		private readonly int _id;
		private int _disposed;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal SubscriptionToken(LogSubscriptionService service, int id)
		{
			_service = service;
			_id = id;
			_disposed = 0;
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
			_service.Unsubscribe(_id);
		}
	}

	private sealed class DelegateLogSubscriber : ILogSubscriber
	{
		private readonly Action<LogEvent> _handler;
		public LogLevel MinLevel { get; }
		public string? CategoryFilter { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal DelegateLogSubscriber(Action<LogEvent> handler, LogLevel minLevel, string? categoryFilter)
		{
			_handler = handler;
			MinLevel = minLevel;
			CategoryFilter = categoryFilter;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnLogEvent(LogEvent logEvent) => _handler(logEvent);
	}

	#endregion
}
