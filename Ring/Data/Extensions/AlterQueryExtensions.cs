using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Data.Extensions;

internal static class AlterQueryExtensions
{
	private const char HashCodeSeparator = (char)5555;

	internal static string? ToSql(this AlterQuery query)
	{
		var builder = query.Builder;
#pragma warning disable IDE0066 // Convert switch statement to expression
		switch (query.Type)
		{
			case AlterQueryType.CreateTable: return builder.Create(query.Table, query.TableSpace);
			case AlterQueryType.CreatePrimaryKey: return builder.Create(query.Constraint!, query.TableSpace);
			case AlterQueryType.CreateIndex: return builder.Create(query.Index!, query.Table, query.TableSpace);
		}
#pragma warning restore IDE0066
		return null;
	}

	internal static int GetHashCode(this AlterQuery alterQuery)
	{
		HashHelper.Djb2X(alterQuery.GetStringCode(), out int hash);
		return hash;
	}

	internal static string GetStringCode(this AlterQuery alterQuery)
	{
		/*
		*  readonly int Id
		*  readonly Table Table
		*  readonly AlterQueryType Type
		*  readonly IDdlBuilder Builder
		*  readonly IColumn? Column
		*  readonly Constraint? Constraint
		*  readonly Index? Index
		*  readonly TableSpace? TableSpace
		*/
		var result = new StringBuilder();
		result.Append(alterQuery.Id);
		result.Append(HashCodeSeparator);
		result.Append(alterQuery.Table.PhysicalName);
		result.Append(HashCodeSeparator);
		result.Append(alterQuery.Type.ToString());
		result.Append(HashCodeSeparator);
		// ignore Builder
		result.Append(alterQuery.Column?.Name);
		result.Append(HashCodeSeparator);
		return result.ToString();
	}

}
