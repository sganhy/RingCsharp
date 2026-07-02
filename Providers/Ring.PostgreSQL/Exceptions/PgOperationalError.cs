namespace Ring.PostgreSQL.Exceptions;


/// <summary>
///     An error related to the database's operation rather than to the SQL the
///     application sent - e.g. a failed connection attempt, an authentication
///     failure, or the server shutting down or running out of resources.
///
///     Roughly corresponds to SQLSTATE classes 08 (Connection Exception),
///     28 (Invalid Authorization Specification), 53 (Insufficient Resources),
///     57 (Operator Intervention), and 58 (System Error).
/// </summary>
public sealed class PgOperationalError : PgException
{
	public PgOperationalError(string message, string sqlState, string severity, string? detail = null, string? hint = null, Exception? innerException = null)
		: base(message, sqlState, severity, detail, hint, innerException)
	{
	}

	public PgOperationalError()
	{
	}

	public PgOperationalError(string message) : base(message)
	{
	}

	public PgOperationalError(string message, Exception innerException) : base(message, innerException)
	{
	}
}
