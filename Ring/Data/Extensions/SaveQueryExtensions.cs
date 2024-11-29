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
		HashHelper.Djb2X(saveQuery.GetStringCode(), out int hash);
		return hash;
	}

	internal static string GetStringCode(this SaveQuery saveQuery)
	{
		/*
		*	readonly Table Table
		*	readonly SaveQueryType Type
		*	readonly IDmlBuilder Builder
		*	readonly string?[] Data
		*	readonly int Offset
		*/
		var result = new StringBuilder();
		result.Append(saveQuery.Table.PhysicalName);
		result.Append(HashCodeSeparator);
		result.Append(saveQuery.Type.ToString());
		// ignore Builder
		result.Append(HashCodeSeparator);
		result.Append(saveQuery.Offset);
		return result.ToString();
	}

	internal static string? ToSql(this SaveQuery query)
	{
		var builder = query.Builder;
#pragma warning disable IDE0066 // Convert switch statement to expression
		switch (query.Type)
		{
			case SaveQueryType.InsertRecord: return builder.Insert(query.Table);
		}
#pragma warning restore IDE0066
		return null;
	}

}
