using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Globalization;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Builders;

internal sealed class SchemaBuilder
{
	private readonly TableBuilder _tableBuilder = new();
	private readonly ParameterBuilder _parameterBuilder = new();

	internal DbSchema GetMeta(DatabaseProvider provider, IConfiguration configuration, bool includeInactive=true)
	{
		// Code size: 484 (0x1e4)
		var metaList = new List<Meta>();
        const SchemaType type = SchemaType.Static;
        const SchemaLoadType loadType = SchemaLoadType.Full;
		var schemaInfo = GetMetaWSchemaInfo(configuration.DefaultSchema);
		var minConnectionPoolSize = int.Max(configuration.MinConnectionPoolSize,1);
		var maxConnectionPoolSize = int.Max(configuration.MaxConnectionPoolSize,1);

		if (minConnectionPoolSize > maxConnectionPoolSize) maxConnectionPoolSize = minConnectionPoolSize;

		metaList.Add(schemaInfo);
		metaList.Add(_parameterBuilder.GetParameter(ParameterType.MinPoolSize,
			minConnectionPoolSize.ToString(CultureInfo.InvariantCulture), 0).ToMeta());
		metaList.Add(_parameterBuilder.GetParameter(ParameterType.MaxPoolSize,
			maxConnectionPoolSize.ToString(CultureInfo.InvariantCulture),0).ToMeta());
		metaList.Add(_parameterBuilder.GetParameter(ParameterType.DbConnectionString,
			configuration.ConnectionString, 0).ToMeta());

		metaList.AddRange(_tableBuilder.GetMeta(configuration.DefaultSchema, provider).ToMeta(0));
		metaList.AddRange(_tableBuilder.GetMetaId(configuration.DefaultSchema, provider).ToMeta(0));
		metaList.AddRange(_tableBuilder.GetLog(configuration.DefaultSchema, provider).ToMeta(0));
		if (includeInactive) metaList.AddRange(_tableBuilder.GetTest(configuration.DefaultSchema, provider).ToMeta(0));
		metaList.AddRange(_tableBuilder.GetCatalog(EntityType.Table, provider).ToMeta(0));
		metaList.AddRange(_tableBuilder.GetCatalog(EntityType.Tablespace, provider).ToMeta(0));
		metaList.AddRange(_tableBuilder.GetCatalog(EntityType.Schema, provider).ToMeta(0));

		// load tablespace info
		if (!string.IsNullOrWhiteSpace(configuration.DefaultTableStorage))
			metaList.Add(GetStorage(configuration.DefaultTableStorage, false, true));
		if (!string.IsNullOrWhiteSpace(configuration.DefaultIndexStorage))
			metaList.Add(GetStorage(configuration.DefaultIndexStorage, true, false));

		var result = Meta.ToSchema(metaList.ToArray(),provider, type, loadType) ?? Meta.GetDefaultSchema(schemaInfo, provider);
		// initialise cache for : DmlBuilder & DqlBuilder
		result.DmlBuilder.Init(result);
		result.DqlBuilder.Init(result);
		return result;
	}

	private static Meta GetMetaWSchemaInfo(string schemaName)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, true);
		return new(0, (byte)EntityType.Schema, 0, 0, flags, schemaName, null, null, true);
	}

	private static Meta GetStorage(string name, bool index, bool table)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, true);
		flags = Meta.SetTablespaceIndex(flags, index);
		flags = Meta.SetTablespaceTable(flags, table);
		return new(table?1:2, (byte)EntityType.Tablespace, 0, 0, flags, name, null, null, true);
	}

}
