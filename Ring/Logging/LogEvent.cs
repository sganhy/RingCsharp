using System.Runtime.CompilerServices;

namespace Ring.Logging;

/// <summary>
/// Immutable log event that captures all relevant information about a logging occurrence.
/// This is the data structure passed to subscribers in the pub/sub logging system.
/// </summary>
public sealed class LogEvent
{
	private static int _idCounter;

	/// <summary>
	/// Unique identifier for this log event.
	/// </summary>
	public int Id { get; }

	/// <summary>
	/// UTC timestamp when the event was created.
	/// </summary>
	public DateTime Timestamp { get; }

	/// <summary>
	/// Severity level of the log event.
	/// </summary>
	public LogLevel Level { get; }

	/// <summary>
	/// The logger category (typically the fully qualified type name).
	/// </summary>
	public string Category { get; }

	/// <summary>
	/// Optional schema identifier for multi-tenant scenarios.
	/// </summary>
	public int? SchemaId { get; }

	/// <summary>
	/// Thread ID where the log event was created.
	/// </summary>
	public int? ThreadId { get; }

	/// <summary>
	/// The call site (file path) where the log was invoked.
	/// </summary>
	public string? CallSite { get; }

	/// <summary>
	/// Optional job identifier for batch/background processing scenarios.
	/// </summary>
	public long? JobId { get; }

	/// <summary>
	/// The method name where the log was invoked.
	/// </summary>
	public string? Method { get; }

	/// <summary>
	/// Line number in source code where the log was invoked.
	/// </summary>
	public int? LineNumber { get; }

	/// <summary>
	/// The formatted log message.
	/// </summary>
	public string? Message { get; }

	/// <summary>
	/// Additional description or context for the log event.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Exception associated with this log event, if any.
	/// </summary>
	public Exception? Exception { get; }

	/// <summary>
	/// Creates a new log event with auto-generated ID and current timestamp.
	/// </summary>
	internal LogEvent(
		LogLevel level,
		string category,
		string? message,
		string? description = null,
		Exception? exception = null,
		int? schemaId = null,
		long? jobId = null,
		string? method = null,
		int? lineNumber = null,
		string? callSite = null)
	{
		Id = Interlocked.Increment(ref _idCounter);
		Timestamp = DateTime.UtcNow;
		Level = level;
		Category = category;
		Message = message;
		Description = description;
		Exception = exception;
		SchemaId = schemaId;
		ThreadId = Environment.CurrentManagedThreadId;
		JobId = jobId;
		Method = method;
		LineNumber = lineNumber;
		CallSite = callSite;
	}

	/// <summary>
	/// Full constructor for creating log events with all properties specified.
	/// </summary>
	internal LogEvent(
		int id,
		DateTime timestamp,
		LogLevel level,
		int? schemaId,
		int? threadId,
		string? callSite,
		long? jobId,
		string? method,
		int? lineNumber,
		string? message,
		string? description,
		Exception? exception,
		string category)
	{
		Id = id;
		Timestamp = timestamp;
		Level = level;
		Category = category;
		SchemaId = schemaId;
		ThreadId = threadId;
		CallSite = callSite;
		JobId = jobId;
		Method = method;
		LineNumber = lineNumber;
		Message = message;
		Description = description;
		Exception = exception;
	}

	/// <summary>
	/// Creates a copy of this log event with a different category.
	/// </summary>
	internal LogEvent WithCategory(string category)
	{
		return new LogEvent(
			Id,
			Timestamp,
			Level,
			SchemaId,
			ThreadId,
			CallSite,
			JobId,
			Method,
			LineNumber,
			Message,
			Description,
			Exception,
			category);
	}

	/// <summary>
	/// Creates a formatted string representation of this log event.
	/// </summary>
	public override string ToString()
	{
		var exceptionInfo = Exception is not null ? $" | Exception: {Exception.GetType().Name}: {Exception.Message}" : string.Empty;
		return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] [{Category}] {Message}{exceptionInfo}";
	}

	/// <summary>
	/// Factory method to create a trace log event.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static LogEvent Trace(string category, string message, string? description = null)
		=> new(LogLevel.Trace, category, message, description);

	/// <summary>
	/// Factory method to create a debug log event.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static LogEvent Debug(string category, string message, string? description = null)
		=> new(LogLevel.Debug, category, message, description);

	/// <summary>
	/// Factory method to create an information log event.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static LogEvent Information(string category, string message, string? description = null)
		=> new(LogLevel.Information, category, message, description);

	/// <summary>
	/// Factory method to create a warning log event.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static LogEvent Warning(string category, string message, string? description = null)
		=> new(LogLevel.Warning, category, message, description);

	/// <summary>
	/// Factory method to create an error log event.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static LogEvent Error(string category, string message, Exception? exception = null, string? description = null)
		=> new(LogLevel.Error, category, message, description, exception);

	/// <summary>
	/// Factory method to create a critical log event.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static LogEvent Critical(string category, string message, Exception? exception = null, string? description = null)
		=> new(LogLevel.Critical, category, message, description, exception);
}
