namespace Ring.Data.Enums;

// using enum value to order ddl execution
internal enum AlterQueryType: int
{
    CreateSchema = 1,
    CreateTable = 10,
	AlterTableAddColumn = 12,
	CreatePrimaryKey = 13,
    CreateIndex = 103,
    Undefined = int.MaxValue
}
