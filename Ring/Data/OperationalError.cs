namespace Ring.Data;

public sealed class OperationalError
{
	public string Message { get; internal set; } = string.Empty;
	public string SqlState { get; internal set; } = string.Empty;
	public string Severity { get; internal set; } = string.Empty;
	public string? Detail { get; internal set; }
	public string? Hint { get; internal set; }
	public string? TableName { get; internal set; }
	public string? SchemaName { get; internal set; }
}
