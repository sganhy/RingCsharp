using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Builders;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class SaveQueryExtensions
{
	internal static string? ToSql(this in SaveQuery query, IDmlBuilder builder)
	{
		// Code size: 181 (0xb5)
		switch (query.Type)
		{
			case SaveQueryType.InsertRecord: return builder.Insert(query.Table);
		}
		return null;
	}

	internal static int Hash(this in SaveQuery saveQuery)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddSaveQuery(saveQuery);
		return hash.ToHashCode();
	}

	internal static bool IsEquivalentTo(this in SaveQuery alterQuery, SaveQuery? other)	=> 
		alterQuery.Table.Id == other?.Table.Id && alterQuery.Table.SchemaId == other?.Table.SchemaId && alterQuery.Type == other?.Type && alterQuery.Offset == other?.Offset;
	

}
