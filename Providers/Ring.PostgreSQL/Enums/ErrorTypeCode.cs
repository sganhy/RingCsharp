namespace Ring.PostgreSQL.Enums;

internal enum ErrorTypeCode : byte
{
	Done = 0,
	Severity = (byte)'S',
	InvariantSeverity = (byte)'V',
	Code = (byte)'C',
	Message = (byte)'M',
	Detail = (byte)'D',
	Hint = (byte)'H',
	Position = (byte)'P',
	InternalPosition = (byte)'p',
	InternalQuery = (byte)'q',
	Where = (byte)'W',
	SchemaName = (byte)'s',
	TableName = (byte)'t',
	ColumnName = (byte)'c',
	DataTypeName = (byte)'d',
	ConstraintName = (byte)'n',
	File = (byte)'F',
	Line = (byte)'L',
	Routine = (byte)'R'
}
