using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Builders;
using Ring.Util.Extensions;
using Ring.Util.Models;

namespace Ring.Data.Extensions;

internal static class SaveQueryExtensions
{
	internal static SqlEntry? ToSql(this in SaveQuery query, IDmlBuilder builder)
	{
		// Code size: 31 (0x1f)
		switch (query.Type)
		{
			case SaveQueryType.InsertRecord:
			case SaveQueryType.PreparedInsertRecord:
				return builder.Insert(query.Table);
		}
		return null;
	}

	internal static int Hash(this in SaveQuery saveQuery)
	{
		// Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddSaveQuery(saveQuery);
		return hash.ToHashCode();
	}

	internal static bool IsEquivalentTo(in this SaveQuery query, in SaveQuery other)
	{
		// Code size: 129 (0x81) - no virtual calls
		if (query.Type != other.Type || query.Offset != other.Offset) return false;

		var table = query.Table;
		var otherTable = other.Table;
		if (table is null || otherTable is null) return ReferenceEquals(table, otherTable);
		if (table.Id != otherTable.Id || table.SchemaId != otherTable.SchemaId) return false;

		if (ReferenceEquals(query.Data, other.Data)) return true; // same buffer: identical, no loop
		return query.Data.AsSpan().SequenceEqual(other.Data); // lengths differ => false, no exception
	}
}
