using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Globalization;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Builders;

internal sealed class SchemaBuilder
{
    private readonly TableBuilder _tableBuilder = new();
    private readonly ParameterBuilder _parameterBuilder = new();

    internal DbSchema GetMeta(string schemaName, DatabaseProvider provider, int maxConnPoolSize)
    {
        var metaList = new List<Meta>();
        var source = SchemaSourceType.NativeDataBase;
        var loadType = SchemaLoadType.Full;
        metaList.Add(GetMetaWSchemaInfo(schemaName));
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.MaxPoolSize, 
            maxConnPoolSize.ToString(CultureInfo.InvariantCulture),0).ToMeta()) ;
        metaList.AddRange(_tableBuilder.GetMeta(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetMetaId(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetLog(schemaName, provider).ToMeta(0));
        return metaList.ToArray().ToSchema(provider, source, loadType);
    }

    private static Meta GetMetaWSchemaInfo(string schemaName)
    {
        var meta = new Meta();
        meta.SetEntityId(0);
        meta.SetEntityName(schemaName);
        meta.SetEntityType(EntityType.Schema);
        meta.SetEntityBaseline(true);
        meta.SetEntityActive(true);
        return meta;
    }

}
