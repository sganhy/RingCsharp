using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Text;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;

/// <summary>
/// 	Casting from Meta.DataType (Int32) to specific Enum
/// </summary>
internal static class IndexExtensions
{
	private const char HashCodeSeparator = (char)1111;

	internal static Meta ToMeta(this Index index, int tableId)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, index.Baseline);
		flags = Meta.SetIndexUnique(flags, index.Unique);
		flags = Meta.SetIndexBitmap(flags, index.Bitmap);
		// int id, byte objectType, int referenceId, int dataType, long flags, string name, string? description, string? value, bool active
		string? value = Meta.SetIndexedColumns(index.Columns);
		var meta = new Meta(index.Id, (byte)EntityType.Index, tableId, 0, flags, index.Name, index.Description, value, index.Active);
		return meta;
	}

	internal static bool IsPrimaryKey(this Index index, Table table)
	{
		// Code size: 130 (0x82)
		var result = false;
		if (index.Unique)
		{
			var pk = table.GetPrimaryKey();
			var key = new StringBuilder();
			foreach (var idx in pk)
			{
				key.Append(idx.PhysicalName);
				key.Append(',');
			}
			key.Length--;
			var indexKey = string.Join(',', index.Columns);
			result = indexKey == key.ToString();
		}
		return result;
	}

	internal static long GetHashCode(this Index index)
	{
		HashHelper.Djb2X(GetStringCode(index), out long hash);
		return hash;
	}

	internal static string GetStringCode(this Index index)
	{
		// Code size: 95 (0x5f)
		/*
		* readonly bool Bitmap
		* readonly string[] Columns
		* readonly bool Unique
		*/
		return new StringBuilder()
			.Append(index.Bitmap)
			.Append(HashCodeSeparator)
			.Append(string.Join(HashCodeSeparator, index.Columns))
			.Append(HashCodeSeparator)
			.Append(index.Unique)
			.Append(HashCodeSeparator)
		/* + BaseEntity string code */
			.Append(BaseEntityExtensions.GetStringCode(index))
			.ToString();
	}

}
