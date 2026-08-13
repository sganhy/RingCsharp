namespace Ring.Data.Enums;

// using enum value to order ddl execution
internal enum AlterQueryType : byte
{
    CreateSchema = 1,
    CreateTable = 10,
	CreateTableComment = 11,
	CreateColumnComment = 12,
	AlterTableAddColumn = 13,
	CreateNotNull = 14,
	CreateCheckConstraint = 15,
	CreatePrimaryKey = 16,
	CreateIndex = 103,
    Undefined = byte.MaxValue
}
