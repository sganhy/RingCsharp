namespace Ring.Schema.Models;

internal sealed class TableSpace : BaseEntity
{
	internal readonly string FileName;
	internal readonly string[] TableName;
	internal readonly bool Index;
	internal readonly bool Table;
	internal readonly bool Constraint;
    internal readonly string PhysicalName;

    internal TableSpace(int id, string name, string physicalName, string? description, bool isIndex, bool isTable, bool isConstraint, string[] tableName,
		string fileName, bool active, bool baseline)
		: base(id, name, description, baseline, active)
	{
		Index = isIndex;
		Table = isTable;
		Constraint = isConstraint;
		TableName = tableName;
		FileName = fileName;
		PhysicalName = physicalName;
    }
}