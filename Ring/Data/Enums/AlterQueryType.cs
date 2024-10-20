namespace Ring.Data.Enums;

internal enum AlterQueryType: int
{
    CreateTable = 1,
    AlterTableAddColumn = 2,
    Undefined = int.MaxValue
}
