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
		switch (query.Type)
		{
			case AlterQueryType.CreateTable: return builder.Create(query.Table, query.TableSpace);
			case AlterQueryType.CreatePrimaryKey: return builder.Create(query.Constraint!, query.TableSpace);
			case AlterQueryType.CreateIndex: return builder.Create(query.Index!, query.Table, query.TableSpace);
		}
		return null;
	}

	internal static int GetHashCode(this AlterQuery alterQuery)
	{
		HashHelper.Djb2X(alterQuery.GetStringCode(), out int hash);
		return hash;
	}

	internal static string GetStringCode(this AlterQuery alterQuery)
	{
        // Code size: 126 (0x7e)
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
        return new StringBuilder()
			.Append(alterQuery.Id)
			.Append(HashCodeSeparator)
			.Append(alterQuery.Table.PhysicalName)
			.Append(HashCodeSeparator)
			.Append(alterQuery.Type.ToString())
			.Append(HashCodeSeparator)
			// ignore Builder
			.Append(alterQuery.Column?.PhysicalName)
			.Append(HashCodeSeparator).ToString();
	}

}
