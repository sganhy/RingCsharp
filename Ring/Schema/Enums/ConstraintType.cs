namespace Ring.Schema.Enums;

internal enum ConstraintType : byte
{
	PrimaryKey = 1,
	UniqueKey = 2,
	Check = 3,
	NotNull = 8,
	ForeignKey = 9,
	Undefined = 127
}
