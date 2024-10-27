using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Constraint
{
	internal readonly ConstraintType Type;
	internal readonly Table ToTable;
	internal readonly List<IColumn> columns;

	internal Constraint(ConstraintType type, Table table)
	{
		Type = type;
		ToTable = table;
		columns = new List<IColumn>();
    }
}
