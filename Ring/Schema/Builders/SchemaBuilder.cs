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
		// Code size: 491 (0x1eb)
		const SchemaType type = SchemaType.Static;
        const SchemaLoadType loadType = SchemaLoadType.Full;
		var schemaInfo = GetMetaWSchemaInfo(configuration.DefaultSchema);
		var minConnectionPoolSize = int.Max(configuration.MinConnectionPoolSize,1);
		var maxConnectionPoolSize = int.Max(configuration.MaxConnectionPoolSize,1);

		if (minConnectionPoolSize > maxConnectionPoolSize) maxConnectionPoolSize = minConnectionPoolSize;

		// get parameters - metaArray
		var metaArray = new Meta[] {
			schemaInfo,
			_parameterBuilder.GetParameter(ParameterType.MinPoolSize, minConnectionPoolSize.ToString(CultureInfo.InvariantCulture), 0).ToMeta(),
			_parameterBuilder.GetParameter(ParameterType.MaxPoolSize, maxConnectionPoolSize.ToString(CultureInfo.InvariantCulture),0).ToMeta(),
			_parameterBuilder.GetParameter(ParameterType.DbConnectionString, configuration.ConnectionString, 0).ToMeta(),
			!string.IsNullOrWhiteSpace(configuration.DefaultTableStorage)? GetStorage(configuration.DefaultTableStorage, false, true) : Meta.GetDefaultMeta(EntityType.Undefined),
			!string.IsNullOrWhiteSpace(configuration.DefaultIndexStorage)? GetStorage(configuration.DefaultIndexStorage, true, false) : Meta.GetDefaultMeta(EntityType.Undefined)
		};
		

		// get tables - prebuiltTables
		var prebuiltTables = new Table[] {
			_tableBuilder.GetMeta(configuration.DefaultSchema, provider),
			_tableBuilder.GetMetaId(configuration.DefaultSchema, provider),
			_tableBuilder.GetLog(configuration.DefaultSchema, provider),
			_tableBuilder.GetTest(configuration.DefaultSchema, provider),
			_tableBuilder.GetCatalog(EntityType.Table, provider),
			_tableBuilder.GetCatalog(EntityType.Tablespace, provider),
			_tableBuilder.GetCatalog(EntityType.Schema, provider)
		};

		// sort prebuiltTables by Name
		prebuiltTables.AsSpan().Sort(static (x, y) => string.CompareOrdinal(x.Name, y.Name));

		var result = Meta.ToSchema(metaArray, provider, type, loadType, prebuiltTables) ?? Meta.GetDefaultSchema(schemaInfo, provider);
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
