namespace Ring.Data;

public enum OperationalErrorType
{
	None = 0,
	PgError = 1,
	ConnectionClosed = 2,
	InvalidProtocol = 3,
	InvalidQuery = 4 // Added for client-side validation limits (e.g., MaxSqlLength)
}
