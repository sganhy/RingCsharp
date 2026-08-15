using Ring.Data;
using Ring.Schema.Builders;
using Ring.Util.Extensions;

namespace Ring.Schema;

public sealed class SchemaManager
{
	private readonly IConnection _connection;

	public SchemaManager(IConnection connection)
	{
		_connection = connection;
	}

	public void CreateInitialSchema(string physicalSchema)
	{
		// create initial schema
		var schemaBuilder = new SchemaBuilder();
		var dbProvider = _connection.ProviderId().ToDatabaseProvider();
		var initialSchema = schemaBuilder.GetMeta(dbProvider, GetInitSchemaConfiguration(physicalSchema, "meta_table", "meta_index"), true);
		var bulkAlter =  new BulkAlter(initialSchema);
		foreach (var table in initialSchema.TablesById)
		{
			if (table.PhysicalType == Enums.PhysicalType.Table) bulkAlter.CreateTable(table);
		}
		bulkAlter.Apply(_connection);
	}

	public List<Record> SelecMeta(string physicalSchema)
	{
		var query = new BulkRetrieve();
		return new List<Record>();
	}

	// create initial schema
	private static IConfiguration GetInitSchemaConfiguration(string physicalSchema,string? defaultTableStorage, string? defaultIndexStorage) => new Configuration
	{
		DefaultSchema = physicalSchema,
		ConnectionString = string.Empty,
		DefaultTableStorage = defaultTableStorage,
		DefaultIndexStorage = defaultIndexStorage,
		MinConnectionPoolSize = 1,
		MaxConnectionPoolSize = 1
	};
}
