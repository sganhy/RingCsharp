namespace Ring.Logging;

/// <summary>
/// Interface for logger implementations.
/// </summary>
internal interface ILogger
{
	/// <summary>
	/// Logs a message at the specified level with an optional exception.
	/// </summary>
	/// <param name="logLevel">The log level.</param>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">Optional exception to log.</param>
	void Log(LogLevel logLevel, string message, Exception? exception = null);

	/// <summary>
	/// Logs a formatted message at the specified level.
	/// </summary>
	/// <param name="logLevel">The log level.</param>
	/// <param name="message">The message template.</param>
	/// <param name="args">Arguments for formatting.</param>
	void Log(LogLevel logLevel, string message, params object?[] args);
}
