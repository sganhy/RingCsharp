using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class ColumnExtensions
{
	internal static string Join(this Column[] columns, char separator)
	{
		// Code size: 72 (0x48)
		var result = new StringBuilder();
		if (columns.Length > 0)
		{
			foreach (var column in columns) result.Append(column.PhysicalName).Append(separator);
			result.Length--;
		}
		return result.ToString();
	}

	internal static Column SetFieldType(this Column column, FieldType fieldType) // Code size: 37 (0x25)
		=> new(column.Type, fieldType, column.PhysicalName, column.SearchableType, column.Id, column.RecordIndex);

	internal static int Hash(this Column column)
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
	internal static bool IsEquivalentTo(this Column column, Column? other)
	{
		// Code size: 98 (0x62)
		if (other is null) return false;
		return column.Id == other!.Id && column.Type == other.Type && column.FieldType == other.FieldType && column.SearchableType == other.SearchableType &&
			string.Equals(column.PhysicalName, other.PhysicalName, StringComparison.Ordinal) && column.RecordIndex == other.RecordIndex;
	}


}
