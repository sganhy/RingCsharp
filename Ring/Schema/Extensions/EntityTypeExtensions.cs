using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class EntityTypeExtensions
{
	internal static (int min,int max) GetRange(this EntityType _) => (0, 124); // Code size: 9 (0x9)

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