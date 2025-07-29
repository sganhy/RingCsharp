using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring;

internal static class Global
{
    private readonly static SchemaBuilder _schemaBuilder = new SchemaBuilder();
    private static DbSchema? _metaSchema;
    private static DbSchema? _defaultSchema;
    private static int _maxSchemaId = 0;
    private static bool _initialized = false;
    private static DbSchema[] _schemas = new DbSchema[2048];

    internal static void Start(string schemaName, Type connectionType, string connectionString,DatabaseProvider provider, 
        int maxConnPoolSize=3)
    {
        //_metaSchema = _schemaBuilder.GetMeta(schemaName, provider, maxConnPoolSize, connectionString);
        _metaSchema.Connections.Init();
        var tblBuilder = new TableBuilder();
        /*
        _metaSchema.SelectQuery(tblBuilder.GetCatalog(EntityType.Table, provider),
            "select table_schema, table_name from information_schema.tables", Array.Empty<IDbDataParameter>());
        */
        _defaultSchema = _metaSchema;
    }

    internal static void SetDefaultSchema(DbSchema schema) => _defaultSchema = schema; // Code size: 7 (0x7)
    internal static void AddSchema(DbSchema schema) => _schemas[schema.Id] = schema;
    internal static DbSchema DefaultSchema => _defaultSchema ?? Meta.GetEmptySchema(new Meta(string.Empty),DatabaseProvider.Undefined); // Code size: 27 (0x1b)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static DbSchema? GetSchema(int id) => _schemas[id];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Table? GetTable(string? schemaName, string tableName) => schemaName == null ? _defaultSchema?.GetTable(tableName) : GetSchema(schemaName)?.GetTable(schemaName); // Code size: 40 (0x28)

    internal static DbSchema? GetSchema(string name)
    {
        return null;
    }

}
