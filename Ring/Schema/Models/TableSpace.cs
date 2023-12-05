namespace Ring.Schema.Models;

internal sealed class TableSpace : BaseEntity
{
	internal readonly string FileName;
	internal readonly string TableName;
	internal readonly bool Index;
	internal readonly bool Table;
	internal readonly bool Constraint;

	internal TableSpace(int id, string name, string? description, bool isIndex, bool isTable, bool isConstraint, string tableName,
        string fileName, bool active, bool baseline)
        : base(id, name, description, active, baseline)
    {
		Index = isIndex;
		Table = isTable;
		Constraint = isConstraint;
		TableName = tableName;
		FileName = fileName;
    }
}