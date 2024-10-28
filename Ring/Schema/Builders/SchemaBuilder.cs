using Ring.Data;
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

    internal DbSchema GetMeta(DatabaseProvider provider, IConfiguration configuration)
    {
        var metaList = new List<Meta>();
        var type = SchemaType.Static;
        var loadType = SchemaLoadType.Full;
        var schemaInfo = GetMetaWSchemaInfo(configuration.MetaSchemaName);

        metaList.Add(schemaInfo);
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.MinPoolSize,
            configuration.MinConnectionPoolSize.ToString(CultureInfo.InvariantCulture), 0).ToMeta());
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.MaxPoolSize,
            configuration.MaxConnectionPoolSize.ToString(CultureInfo.InvariantCulture),0).ToMeta());
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionString, 
            configuration.ConnectionString, 0).ToMeta());
        //metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionType, typeof(SchemaBuilder), 0).ToMeta());

        metaList.AddRange(_tableBuilder.GetMeta(configuration.MetaSchemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetMetaId(configuration.MetaSchemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetLog(configuration.MetaSchemaName, provider).ToMeta(0));

        var result = Meta.ToSchema(metaList.ToArray(),provider, type, loadType) ?? Meta.GetEmptySchema(schemaInfo, provider);
        // initialise cache for : DmlBuiler & DqlBuiler
        var tableIndex = result.GetTableIndex();
        result.DmlBuiler.Init(result, tableIndex);
        result.DqlBuiler.Init(result, tableIndex);
        return result;
    }

    private static Meta GetMetaWSchemaInfo(string schemaName)
    {
        var flags = 0L;
        flags = Meta.SetEntityBaseline(flags, true);
        return new(0, (byte)EntityType.Schema, 0, 0, flags, schemaName, null, null, true);
    }

}
