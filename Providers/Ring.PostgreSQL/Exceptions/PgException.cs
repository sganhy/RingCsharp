namespace Ring.PostgreSQL.Exceptions;

/// <summary>
///     Base class for errors reported by, or encountered while communicating
///     with, a PostgreSQL server. The fields mirror those of an ErrorResponse /
///     NoticeResponse message - see the Postgres protocol docs ("ErrorResponse (B)").
/// </summary>
public abstract class PgException : Exception
{
	/// <summary>The five-character SQLSTATE error code, e.g. "08001", "28P01", "23505".</summary>
	public string? SqlState { get; }

	/// <summary>The error severity: ERROR, FATAL, or PANIC (NOTICE/WARNING/etc. for notices).</summary>
	public string? Severity { get; }

	/// <summary>An optional secondary message carrying more detail about the error.</summary>
	public string? Detail { get; init; }

	/// <summary>An optional suggestion on how to resolve the problem.</summary>
	public string? Hint { get; init; }

	/// <summary>The context in which the error occurred (e.g. a PL/pgSQL call stack), if provided.</summary>
	public string? Where { get; init; }

	/// <summary>The name of the schema associated with the error, if any.</summary>
	public string? SchemaName { get; init; }

	/// <summary>The name of the table associated with the error, if any.</summary>
	public string? TableName { get; init; }

	/// <summary>The name of the column associated with the error, if any.</summary>
	public string? ColumnName { get; init; }

	/// <summary>The name of the constraint associated with the error, if any (e.g. a unique violation).</summary>
	public string? ConstraintName { get; init; }

	protected PgException(string message, string sqlState, string severity, string? detail = null, string? hint = null, Exception? innerException = null)
		: base(message, innerException)
	{
		SqlState = sqlState;
		Severity = severity;
		Detail = NullIfEmpty(detail);
		Hint = NullIfEmpty(hint);
	}

	private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

	public override string ToString()
	{
		var result = $"{Severity} [{SqlState}]: {Message}";
		if (Detail is not null) result += $"\n  Detail: {Detail}";
		if (Hint is not null) result += $"\n  Hint: {Hint}";
		if (Where is not null) result += $"\n  Where: {Where}";
		return result;
	}

	protected PgException()
	{
	}

	protected PgException(string message) : base(message)
	{
	}

	protected PgException(string message, Exception innerException) : base(message, innerException)
	{
	}
}