namespace Ring.Logging;

public sealed class LogEvent
{
	public int Id { get; }
	public LogLevel Level { get; }
	public int? SchemaId { get; }
	public int? ThreadId { get; }
	public string? CallSite { get; }
	public long? JobId { get; }
	public string? Method { get; }
	public int? LineNumber { get; }
	public string? Message { get; }
	public string? Description { get; }
	public Exception? Exception { get; }

	internal LogEvent()
	{
	}

	internal LogEvent(int id, DateTime entryTime, LogLevel level, int? schemaId, int? threadId, string? callSite, long? jobId, string? method, int? lineNumber, string? message, string? description, Exception? exception)
	{
		Id = id;
		Level = level;
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
}
