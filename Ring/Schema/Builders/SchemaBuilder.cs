using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Globalization;
using System.Runtime.CompilerServices;
using DbSchema = Ring.Schema.Models.Schema;

[assembly: InternalsVisibleTo("ProjectTest, PublicKey=00240000048000009400000006020000002400005253413100040000010001004946cfade36fd2a018cca52889dbdc66d71441210e6d14d113238681aa63a6ca75cea31cd9a2108961d8917afaf8a672b6d94ec92dd460decdb657e126ba2ee426ef48f42bc1587e505541f0ce4dd11e97abe55dee2a251a4315a1c8412cda7ffceb18ff0c04a2e3e0ae6e6d79532265224c8b29ba45981cbe65ec0d22a8f1d1")]

namespace Ring.Schema.Builders;

internal sealed class SchemaBuilder
{
    private readonly TableBuilder _tableBuilder = new();
    private readonly ParameterBuilder _parameterBuilder = new();

    internal DbSchema GetMeta(string schemaName, DatabaseProvider provider, int maxConnPoolSize, string connectionString, Type connectionType)
    {
        var metaList = new List<Meta>();
        var type = SchemaType.Static;
        var loadType = SchemaLoadType.Full;
        var schemaInfo = GetMetaWSchemaInfo(schemaName);
        metaList.Add(schemaInfo);
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.MaxPoolSize, 
            maxConnPoolSize.ToString(CultureInfo.InvariantCulture),0).ToMeta());
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionString, connectionString, 0).ToMeta());
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionType, connectionType.AssemblyQualifiedName, 0).ToMeta());
        metaList.AddRange(_tableBuilder.GetMeta(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetMetaId(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetLog(schemaName, provider).ToMeta(0));
        var result = metaList.ToArray().ToSchema(provider, type, loadType) ?? MetaExtensions.GetEmptySchema(schemaInfo, provider);
        // initialise cache for : DmlBuiler & DqlBuiler
        result.DmlBuiler.Init(result);
        result.DqlBuiler.Init(result);
        return result;
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
