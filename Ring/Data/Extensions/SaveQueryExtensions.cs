using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Data.Extensions;

internal static class SaveQueryExtensions
{
	private const char HashCodeSeparator = (char)4444;

	internal static int GetHashCode(this SaveQuery saveQuery)
	{
        // Code size: 15 (0xf)
        HashHelper.Djb2X(saveQuery.GetStringCode(), out int hash);
		return hash;
	}

	internal static string GetStringCode(this SaveQuery saveQuery)
	{
        // Code size: 85 (0x55)
        /*
		*	readonly Table Table
		*	readonly SaveQueryType Type
		*	readonly IDmlBuilder Builder
		*	readonly string?[] Data
		*	readonly int Offset
		*/
        var result = new StringBuilder();
		result.Append(saveQuery.Table.PhysicalName)
			.Append(HashCodeSeparator)
			.Append(saveQuery.Type.ToString())
			// ignore Builder
			.Append(HashCodeSeparator)
			.Append(saveQuery.Offset);
		return result.ToString();
	}

	internal static string? ToSql(this SaveQuery query)
	{
		var builder = query.Builder;
		switch (query.Type)
		{
			case SaveQueryType.InsertRecord: return builder.Insert(query.Table);
		}
		return null;
	}

}
