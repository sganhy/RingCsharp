using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Builders;

internal sealed class SchemaBuilder
{
    private readonly TableBuilder _tableBuilder = new();
    private readonly ParameterBuilder _parameterBuilder = new();

    internal DbSchema GetMeta(string schemaName, DatabaseProvider provider)
    {
        var metaList = new List<Meta>();
        var source = SchemaSourceType.NativeDataBase;
        var loadType = SchemaLoadType.Full;
        metaList.Add(new Meta() { ObjectType = (byte)EntityType.Schema, Name=schemaName });
        metaList.AddRange(_tableBuilder.GetMeta(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetMetaId(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetLog(schemaName, provider).ToMeta(0));
        return metaList.ToArray().ToSchema(provider, source, loadType);
    }

}
