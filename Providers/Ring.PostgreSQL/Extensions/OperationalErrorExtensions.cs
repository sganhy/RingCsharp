using Ring.Data;
using Ring.PostgreSQL.Exceptions;

namespace Ring.PostgreSQL.Extensions;

internal static class OperationalErrorExtensions
{
	/// <summary>
	///     Converts a protocol-level <see cref="OperationalError"/> (parsed from a
	///     PostgreSQL ErrorResponse wire message) into a throwable
	///     <see cref="PgOperationalError"/> exception, preserving all fields.
	/// </summary>
	internal static PgOperationalError ToPgOperationalError(this OperationalError error) =>
		new(
			string.IsNullOrEmpty(error.Message) ? "An error was returned by the server." : error.Message,
			string.IsNullOrEmpty(error.SqlState) ? "58000" : error.SqlState,
			string.IsNullOrEmpty(error.Severity) ? "ERROR" : error.Severity,
			error.Detail,
			error.Hint
		);
}
