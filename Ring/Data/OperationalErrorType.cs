namespace Ring.Data;

public enum OperationalErrorType
{
	None = 0,
	PgError = 1,
	ConnectionClosed = 2,
	InvalidProtocol = 3
}
