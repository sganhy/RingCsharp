using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;

namespace Ring.Schema.Extensions;

internal static class TableSpaceExtensions
{
	/// <summary>
	/// 	Convert DbSchema model and its contained schema objects into a Meta array representation.
	/// </summary>
	internal static Meta ToMeta(this TableSpace tableSpace, int schemaId)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, tableSpace.Baseline);
		flags = Meta.SetTablespaceIndex(flags, tableSpace.Index);
		flags = Meta.SetTablespaceTable(flags, tableSpace.Table);
		return new Meta(
			tableSpace.Id,
			(byte)EntityType.Tablespace,
			schemaId, // ReferenceId
			0,
			flags,
			tableSpace.Name,
			tableSpace.Description,
			null, // Value
			tableSpace.Active
		);
	}


	internal static int Hash(this TableSpace tableSpace)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddTableSpace(tableSpace);
		return hash.ToHashCode();
	}

	internal static bool IsEquivalentTo(this TableSpace tableSpace, TableSpace? other)
	{
		if (other is null) return false;
		if (!tableSpace.BaseEntityEquals(other)) return false;
		if (tableSpace.Index != other.Index) return false;
		if (tableSpace.Table != other.Table) return false;
		if (tableSpace.Constraint != other.Constraint) return false;
		if (tableSpace.PhysicalName != other.PhysicalName) return false;
		// TODO review of tablespace structure
		return true;
	}

}