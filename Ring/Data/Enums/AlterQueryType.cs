namespace Ring.Data.Enums;

// using enum value to order ddl execution
internal enum AlterQueryType : short
{
    CreateSchema = 1,
    CreateTable = 10,
	AlterTableAddColumn = 12,
	CreateNotNull = 13,
	CreateCheckConstraint = 14,
	CreatePrimaryKey = 15,
	CreateIndex = 103,
    Undefined = short.MaxValue
}
