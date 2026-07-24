namespace Ring.Data;

public sealed class ConnectionOperationalError
{
	internal readonly string Message;
	internal readonly string SqlState;
	internal readonly string Severity;
	internal readonly string? Detail;
	internal readonly string? TableName;

	internal ConnectionOperationalError(string message, string state, string severity, string? detail, string? tableName)
	{
		Message = message;
		SqlState = state;
		Severity = severity;
		Detail = detail;
		TableName = tableName;
	}
}
