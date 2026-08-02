using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class EntityTypeExtensions
{
	internal static TableType ToTableType(this EntityType entityType)
	{
		// Code size: 26 (0x1a)
		switch (entityType)
		{
			case EntityType.Table: return TableType.TableCatalog;
			case EntityType.Schema: return TableType.SchemaCatalog;
			case EntityType.Tablespace: return TableType.TablespaceCatalog;
			default: return TableType.Logical;
		}
	}
}