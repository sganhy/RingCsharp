using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;

/// <summary>
/// 	Casting from Meta.DataType (Int32) to specific Enum
/// </summary>
internal static class IndexExtensions
{

	internal static Meta ToMeta(this Index index, int tableId)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, index.Baseline);
		flags = Meta.SetIndexUnique(flags, index.Unique);
		flags = Meta.SetIndexBitmap(flags, index.Bitmap);
		// int id, byte objectType, int referenceId, int dataType, long flags, string name, string? description, string? value, bool active
		string? value = index.ColumnList;
		var meta = new Meta(index.Id, (byte)EntityType.Index, tableId, 0, flags, index.Name, index.Description, value, index.Active);
		return meta;
	}

	internal static bool IsPrimaryKey(this Index index, Table table)
	{
		// Code size: 51 (0x33)
		var result = false;
		if (index.Unique)
		{
			var pk = table.GetPrimaryKey().ToArray();
			result = index.Columns.Join(',') == pk.Join(',');
		}
		return result;
	}

	internal static int Hash(this Index index)
	{
		// Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddIndex(index);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Field instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this Index index, Index? other)
	{
		// Code size: 105 (0x69)
		if (!index.BaseEntityEquals(other)) return false;
		// other cannot be null here 
		return index.Unique == other!.Unique && index.Bitmap == other.Bitmap && string.Equals(index.ColumnList, other.ColumnList, StringComparison.Ordinal);
	}

}
