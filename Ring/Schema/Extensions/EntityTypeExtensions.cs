using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class EntityTypeExtensions
{
	internal static TableType ToTableType(this EntityType entityType)
	{
		// Code size: 34 (0x22)
		TableType result;
		switch (entityType)
		{
			case EntityType.Table:
				result = TableType.TableCatalog;
				break;
			case EntityType.Schema:
				result = TableType.SchemaCatalog;
				break;
			case EntityType.Tablespace:
				result = TableType.TableCatalog;
				break;
			default:
				result = TableType.Logical;
				break;
		}
		return result;
	}
}