using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Globalization;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Builders;

internal sealed class SchemaBuilder
{
    private readonly TableBuilder _tableBuilder = new();
    private readonly ParameterBuilder _parameterBuilder = new();

    internal DbSchema GetMeta(string schemaName, DatabaseProvider provider, int maxConnPoolSize, string connectionString)
    {
        var metaList = new List<Meta>();
        var type = SchemaType.Static;
        var loadType = SchemaLoadType.Full;
        var schemaInfo = GetMetaWSchemaInfo(schemaName);

        metaList.Add(schemaInfo);
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.MaxPoolSize, 
            maxConnPoolSize.ToString(CultureInfo.InvariantCulture),0).ToMeta());
        metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionString, connectionString, 0).ToMeta());
        //metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionType, typeof(SchemaBuilder), 0).ToMeta());

        metaList.AddRange(_tableBuilder.GetMeta(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetMetaId(schemaName, provider).ToMeta(0));
        metaList.AddRange(_tableBuilder.GetLog(schemaName, provider).ToMeta(0));

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
