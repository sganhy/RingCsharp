using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class TableSpace : BaseEntity, IEquatable<TableSpace>
{
	internal readonly string FileName;
	internal readonly string[] TableName; // sorted logical table names, if empty then default tablespace
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

	public static bool operator ==(TableSpace left, TableSpace right) => left.Equals(right);
	public static bool operator !=(TableSpace left, TableSpace right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is TableSpace tableSpace && Equals(tableSpace);
	public bool Equals(TableSpace? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {Name}";
#endif

}