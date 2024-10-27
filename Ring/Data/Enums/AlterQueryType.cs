namespace Ring.Data.Enums;

// using enum value to order ddl execution
internal enum AlterQueryType: int
{
	CreateTable = 1,
	AlterTableAddColumn = 2,
	CreatePrimaryKey = 3,
	Undefined = int.MaxValue
}
