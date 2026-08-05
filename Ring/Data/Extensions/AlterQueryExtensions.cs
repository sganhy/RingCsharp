using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Builders;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class AlterQueryExtensions
{
	internal static string? ToSql(this AlterQuery query, IDdlBuilder builder)
	{
		// Code size: 91 (0x5b)
		switch (query.Type)
		{
			case AlterQueryType.CreateTable: 
				return builder.Create(query.Table, query.TableSpace);
			case AlterQueryType.CreateNotNull:
			case AlterQueryType.CreatePrimaryKey:
			//case AlterQueryType.CreateCheckConstraint:
				return builder.Create(query.Constraint!, query.TableSpace);
			case AlterQueryType.CreateIndex: 
				return builder.Create(query.Index!, query.Table, query.TableSpace);
		}
		return null;
	}

	internal static int Hash(this AlterQuery alterQuery)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddAlterQuery(alterQuery);
		return hash.ToHashCode();
	}


	/// <summary>
	/// Determines if two AlterQuery instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this AlterQuery alterQuery, AlterQuery? other)
	{
		if (!other.HasValue) return false;
		var otherValue = other.Value;
        if (alterQuery.Type != otherValue.Type || alterQuery.Table.Id != otherValue.Table.Id || alterQuery.Table.SchemaId == otherValue.Table.SchemaId) return false;
		return true;
    }
}
