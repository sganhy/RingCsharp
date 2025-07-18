using Ring.Schema.Enums;
using Ring.Schema.Models;
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

}
