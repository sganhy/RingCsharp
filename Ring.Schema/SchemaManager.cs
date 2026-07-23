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
		var initialSchema = schemaBuilder.GetMeta(dbProvider, GetInitSchemaConfiguration(physicalSchema), true);
		var bulkAlter =  new BulkAlter(initialSchema);
		foreach (var table in initialSchema.TablesById)
		{
			bulkAlter.CreateTable(table);
		}
		bulkAlter.Apply(_connection);
	}

	private static IConfiguration GetInitSchemaConfiguration(string physicalSchema) => new Configuration
	{
		DefaultSchema = physicalSchema,
		ConnectionString = string.Empty,
		MinConnectionPoolSize = 1,
		MaxConnectionPoolSize = 1
	};
}
