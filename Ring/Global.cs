using Ring.Schema.Builders;
using Ring.Schema.Enums;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring;

internal static class Global
{
    private readonly static SchemaBuilder _schemaBuilder = new SchemaBuilder();
    private static DbSchema? _metaSchema;
    private static DbSchema? _defaultSchema;

    internal static void Start(string schemaName, DatabaseProvider provider, int maxConnPoolSize=10)
    {
        _metaSchema = _schemaBuilder.GetMeta(schemaName, provider, maxConnPoolSize);
        _defaultSchema = _metaSchema;
    }

}
