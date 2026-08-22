using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Constraint : BaseEntity, IEquatable<Constraint>
{
	internal readonly ConstraintType Type;
	internal readonly Table ToTable;
	internal readonly Column[] Columns;
	internal readonly string PhysicalName;
	internal readonly int? MinValue;
	internal readonly int? MaxValue;

	internal Constraint(int id, string name,string? description, bool baseline, bool enabled, ConstraintType type, Table table, 
		string physicalName, Column[] columns, int? minValue=null, int? maxValue=null) : base(id, name, description, baseline, enabled)
	{
		Type = type;
		ToTable = table;
		Columns = columns;
		PhysicalName = physicalName;
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public static bool operator ==(Constraint left, Constraint right) => left.Equals(right);
	public static bool operator !=(Constraint left, Constraint right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Constraint constraint && Equals(constraint);
	public bool Equals(Constraint? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Type} - {ToTable.Name}";
#endif

}
