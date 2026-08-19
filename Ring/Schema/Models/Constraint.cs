using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Constraint
{
	internal readonly ConstraintType Type;
	internal readonly Table ToTable;
	internal readonly Column[] Columns;
	internal readonly string PhysicalName;
	internal readonly int? MinValue;
	internal readonly int? MaxValue;

	internal Constraint(ConstraintType type, Table table, string physicalName, int columnCount, int? minValue=null, int? maxValue=null)
	{
		Type = type;
		ToTable = table;
		Columns = columnCount <= 0 ? Array.Empty<Column>() : new Column[columnCount];
		PhysicalName = physicalName;
		MinValue = minValue;
		MaxValue = maxValue;
	}

#if DEBUG
	public override string ToString() => $"{Type} - {ToTable.Name}";
#endif

}
