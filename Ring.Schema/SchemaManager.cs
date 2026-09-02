using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Extensions;
using Ring.Schema.Helpers;
using Ring.Schema.Models;
using Ring.Util.Extensions;

namespace Ring.Schema;

public sealed class SchemaManager
{
	private const string FieldId = "id";
	private const string FieldSchemaId = "schema_id";
	private const string FieldObjectType = "object_type";
	private const string FieldReferenceId = "reference_id";
	private const string FieldDataType = "data_type";
	private const string FieldFlags = "flags";
	private const string FieldName = "name";
	private const string FieldDescription = "description";
	private const string FieldValue = "value";
	private const string FieldActive = "active";

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
		var initialSchema = schemaBuilder.GetMeta(dbProvider, GetInitSchemaConfiguration(physicalSchema, "meta_table", "meta_index"));
		var bulkAlter =  new BulkAlter(initialSchema);
		foreach (var table in initialSchema.TablesById)
		{
			if (table.PhysicalType == Enums.PhysicalType.Table) bulkAlter.CreateTable(table);
		}
		bulkAlter.Apply(_connection);


		var bulkSave = new BulkSave(initialSchema);
		foreach (var param in initialSchema.Parameters)
		{
			var meta= param.ToMeta();
			var record = ToRecord(meta, initialSchema.GetTable("@meta"), initialSchema.Id);
			bulkSave.ForceInsert(record);
		}
		bulkSave.Save(_connection,true);



	}

	public List<Record> SelecMeta(string physicalSchema)
	{
		var query = new BulkRetrieve();
		var schemaBuilder = new SchemaBuilder();
		var dbProvider = _connection.ProviderId().ToDatabaseProvider();
		query.Schema = schemaBuilder.GetMeta(dbProvider, GetInitSchemaConfiguration(physicalSchema, "meta_table", "meta_index"));
		//query.SimpleQuery(0, "@test");
		query.SimpleQuery(0, "@test");
		query.RetrieveRecords(_connection);
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

	internal static Record ToRecord(Meta meta, Table metaTable, int schemaId)
	{
		var record = new Record(metaTable);

		// Id/SchemaId/ObjectType/ReferenceId/DataType all go through
		// SetField(string, long): its FieldType switch already accepts Long,
		// Int, or Short columns (int4/int2 here) with range-checking, so this
		// works without needing a separate per-column FieldType map.
		record.SetField(FieldId, meta.Id);
		record.SetField(FieldSchemaId, schemaId);
		record.SetField(FieldObjectType, meta.ObjectType);
		record.SetField(FieldReferenceId, meta.ReferenceId);
		record.SetField(FieldDataType, meta.DataType);
		record.SetField(FieldFlags, meta.Flags);
		record.SetField(FieldName, meta.Name);
		record.SetField(FieldDescription, meta.Description); // null-safe: SetField(string, string?) handles null
		record.SetField(FieldValue, meta.Value);              // null-safe, same as above
		record.SetField(FieldActive, meta.Active);            // active is bool NOT NULL - matches SetField(string, bool)

		return record;
	}

}
