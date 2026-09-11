namespace Ring.Logging;

/// <summary>
/// Interface for log event subscribers.
/// QoS 1: At-most-once delivery - events are fired directly, no buffering.
/// </summary>
public interface ILogSubscriber
{
	/// <summary>
	/// Called when a log event is published. Must be thread-safe.
	/// </summary>
	void OnLogEvent(LogEvent logEvent);

	/// <summary>
	/// Minimum log level this subscriber receives.
	/// </summary>
	LogLevel MinLevel { get; }

	/// <summary>
	/// Optional category filter prefix. Null = receive all.
	/// </summary>
	string? CategoryFilter { get; }
}
