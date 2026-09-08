using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Extensions;

namespace Ring.Schema.Extensions;

internal static class ColumnExtensions
{
	internal static Column SetFieldType(this in Column column, FieldType fieldType, IDdlBuilder builder)
	{
		// Code size: 61 (0x3d)
		var binaryLength = -1;
		var binaryType = false;
		if (fieldType!=FieldType.String) 
		{
			binaryLength = builder.GetBinaryParamLength(fieldType);
			binaryType = binaryLength>0;
		}
		return new(column.Type, fieldType, column.PhysicalName, column.SearchableType, column.Id, column.RecordIndex, binaryLength, binaryType);
	}

	internal static Column SetPhysicalName(this in Column column, string physicalName) // Code size: 49 (0x31)
		=> new(column.Type, column.FieldType, physicalName, column.SearchableType, column.Id, column.RecordIndex, column.BinaryLength, column.BinaryType);

	internal static int Hash(this in Column column)
	{
		// Code size: 29 (0x1d)
		var hash = new HashCode();
		hash.AddColumn(column);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Column instances have equivalent definitions, regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this in Column column, in Column other) => // Code size: 121 (0x79)
		column.Id == other.Id && column.Type == other.Type && column.FieldType == other.FieldType && column.SearchableType == other.SearchableType &&
			string.Equals(column.PhysicalName, other.PhysicalName, StringComparison.Ordinal) && column.RecordIndex == other.RecordIndex && column.BinaryLength == other.BinaryLength && 
			column.BinaryType == other.BinaryType;
	
}
