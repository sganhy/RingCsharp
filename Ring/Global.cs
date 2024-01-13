using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Data;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring;

internal static class Global
{
    private readonly static SchemaBuilder _schemaBuilder = new SchemaBuilder();
    private static DbSchema? _metaSchema;
    private static DbSchema? _defaultSchema;

    internal static void Start(string schemaName, Type connectionType, string connectionString,DatabaseProvider provider, 
        int maxConnPoolSize=3)
    {
        _metaSchema = _schemaBuilder.GetMeta(schemaName, provider, maxConnPoolSize, connectionString, connectionType);
        _metaSchema.Connections.Init();
        var tblBuilder = new TableBuilder();
        _metaSchema.SelectQuery(tblBuilder.GetCatalog(EntityType.Table, provider),
            "select table_schema, table_name from information_schema.tables", Array.Empty<IDbDataParameter>());
        _defaultSchema = _metaSchema;
    }

}
