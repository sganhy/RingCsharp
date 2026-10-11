using Ring.Data.Extensions;
using System.Runtime.InteropServices;

namespace Ring.Data;

[StructLayout(LayoutKind.Auto)]
public readonly struct OperationalError: IEquatable<OperationalError>
{
	// Total Size(Auto): 64 bytes
	public readonly OperationalErrorType Type;
	public readonly string Message;
	public readonly string SqlState;
	public readonly string Severity;
	public readonly string? Detail;
	public readonly string? Hint;
	public readonly string? TableName;
	public readonly string? SchemaName;

	public OperationalError(OperationalErrorType errorType,	string message,	string sqlState, string severity, string? detail, string? hint, string? tableName,	string? schemaName)
	{
		Type = errorType;
		Message = message;
		SqlState = sqlState;
		Severity = severity;
		Detail = detail;
		Hint = hint;
		TableName = tableName;
		SchemaName = schemaName;
	}

	public static bool operator ==(in OperationalError left, in OperationalError right) => left.Equals(right);
	public static bool operator !=(in OperationalError left, in OperationalError right) => !left.Equals(right);
	public override readonly bool Equals(object? obj) => obj is OperationalError operationalError && Equals(operationalError);
	public readonly bool Equals(OperationalError other) => this.IsEquivalentTo(other);
	public override readonly int GetHashCode() => this.Hash();
}
