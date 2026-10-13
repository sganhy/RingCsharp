using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Extensions;
using Ring.Util.Builders;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class AlterQueryExtensions
{
	internal static string? ToSql(this in AlterQuery query, IDdlBuilder builder)
	{
		// Code size: 181 (0xb5)
		switch (query.Type)
		{
			case AlterQueryType.CreateTable: return builder.Create(query.Table, query.TableSpace);
			case AlterQueryType.CreateTableComment: return builder.Comment(query.Table);
			case AlterQueryType.CreateColumnComment: return query.ColumnIndex>=0 ? builder.Comment(query.Table, query.Table.Columns[query.ColumnIndex]) : null;
			case AlterQueryType.CreatePrimaryKey: 
			case AlterQueryType.CreateNotNull:
			case AlterQueryType.CreateDefaultConstraint:
			case AlterQueryType.CreateCheckConstraint: return builder.Create(query.Constraint!, query.Table, query.TableSpace);
			case AlterQueryType.CreateIndex: return builder.Create(query.Index!, query.Table, query.TableSpace);
		}
		return null;
	}

	internal static int Hash(this in AlterQuery alterQuery)
	{
		// Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddAlterQuery(alterQuery);
		return hash.ToHashCode();
	}


	/// <summary>
	/// Determines if two AlterQuery instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this in AlterQuery? alterQuery, in AlterQuery? other)
	{
		// Code size: 236 (0xec)
		if (!other.HasValue && !alterQuery.HasValue) return true;
		if (!other.HasValue || !alterQuery.HasValue) return false;
		if (!ConstraintExtensions.IsEquivalentTo(alterQuery.Value.Constraint, other.Value.Constraint)) return false;
		if (!IndexExtensions.IsEquivalentTo(alterQuery.Value.Index, other.Value.Index)) return false;
		if (!TableSpaceExtensions.IsEquivalentTo(alterQuery.Value.TableSpace, other.Value.TableSpace)) return false;

		var otherValue = other.Value;
		var alterQueryValue = alterQuery.Value;
		return alterQueryValue.Type == otherValue.Type && alterQueryValue.Table.Id == otherValue.Table.Id && alterQueryValue.Table.SchemaId == otherValue.Table.SchemaId &&
			 alterQueryValue.Id == otherValue.Id && alterQueryValue.ColumnIndex == otherValue.ColumnIndex;
    }
}
