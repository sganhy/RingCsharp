using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Constraint
{
	internal readonly ConstraintType Type;
	internal readonly Table ToTable;
	internal readonly List<Column> columns;
	internal readonly string PhysicalName;

	internal Constraint(ConstraintType type, Table table, string physicalName)
	{
		Type = type;
		ToTable = table;
		columns = new List<Column>();
		PhysicalName = physicalName;
	}
}
