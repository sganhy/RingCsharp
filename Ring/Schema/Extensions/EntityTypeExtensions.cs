using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class EntityTypeExtensions
{
    internal static TableType ToTableType(this EntityType entityType)
    {
        TableType result;
#pragma warning disable IDE0066 // Convert switch statement to expression
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
#pragma warning restore IDE0066
        return result;
    }
}