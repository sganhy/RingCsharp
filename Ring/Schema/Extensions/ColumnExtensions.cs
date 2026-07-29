using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;

namespace Ring.Schema.Extensions;

internal static class ColumnExtensions
{
	internal static Column SetFieldType(this in Column column, FieldType fieldType) // Code size: 37 (0x25)
		=> new(column.Type, fieldType, column.PhysicalName, column.SearchableType, column.Id, column.RecordIndex);

	internal static int Hash(this in Column column)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddColumn(column);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Column instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this in Column column, in Column other) => // Code size: 93 (0x5d)
		column.Id == other.Id && column.Type == other.Type && column.FieldType == other.FieldType && column.SearchableType == other.SearchableType &&
			string.Equals(column.PhysicalName, other.PhysicalName, StringComparison.Ordinal) && column.RecordIndex == other.RecordIndex;
	
}
