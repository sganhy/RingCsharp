using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders;

internal sealed class TableBuilder
{
	private readonly string FieldId = "id";
	private readonly string FieldSchemaId = "schema_id";
	private readonly string FieldObjectType = "object_type";
	private readonly string FieldValue = "value";
	private readonly string FieldReferenceId = "reference_id";
	private readonly string FieldDataType = "data_type";
	private readonly string FieldFlags = "flags";
	private readonly string FieldName = "name";
	private readonly string FieldSchemaName = "schema_name";
	private readonly string FieldTestPrefix = "test_";
	private readonly string FieldActive = "active";
	private readonly string FieldDescription = "description";
	private readonly string FieldLevelId = "level_id";
	private readonly string FieldEntryTime = "entry_time";
	private readonly string FieldThreadId = "thread_id";
	private readonly string FieldCallSite = "call_site";
	private readonly string FieldJobId = "job_id";
	private readonly string FieldMethod = "method";
	private readonly string FieldLineNumber = "line_number";
	private readonly string FieldMessage = "message";

	internal Table GetMeta(string schemaName, DatabaseProvider provider) {
		// Code size: 310 (0x136)
		var id = GetField(FieldId, FieldType.Int);
		var schemaId = GetField(FieldSchemaId, FieldType.Int);
		var objectType = GetField(FieldObjectType, FieldType.Byte);
		var referenceId = GetField(FieldReferenceId, FieldType.Int);
		// increase IL code to 374 bytes - better to copy Meta struct twice than to use ref readonly
		// ref readonly var id = ref fieldId;
		// ref readonly var schemaId = ref fieldSchemaId;
		// ref readonly var objectType = ref fieldObjectType;
		// ref readonly var referenceId = ref fieldReferenceId;
		var metaArr = new[] {
			id, schemaId, objectType, referenceId,
			GetField(FieldDataType, FieldType.Int),
			GetField(FieldFlags, FieldType.Long),
			GetField(FieldName, FieldType.String,60),
			GetField(FieldDescription, FieldType.LongString,false),
			GetField(FieldValue, FieldType.LongString,false),
			GetField(FieldActive, FieldType.Boolean),
			GetIndex(true, new [] { id, schemaId, objectType, referenceId })
		};
		var metaTable = GetTable((int)TableType.Meta, TableType.Meta.GetLogicalName(), TableType.Meta);
		return GetTable(schemaName, provider, metaArr, metaTable);
	}

	internal Table GetMetaId(string schemaName, DatabaseProvider provider) 
	{
		// Code size: 173 (0xad)
		var id = GetField(FieldId, FieldType.Int);
		var schemaId = GetField(FieldSchemaId, FieldType.Int);
		var objectType = GetField(FieldObjectType, FieldType.Byte);
		var metaArr = new[] {
			id, schemaId, objectType,
			GetField(FieldValue, FieldType.Long),
			GetIndex(true, new[] { id, schemaId, objectType })
		};
		var metaTable = GetTable((int)TableType.MetaId, TableType.MetaId.GetLogicalName(), TableType.MetaId);
		return GetTable(schemaName, provider, metaArr, metaTable);
	}

	internal Table GetLog(string schemaName, DatabaseProvider provider) 
	{
		// Code size: 322 (0x142)
		var entryTime = GetField(FieldEntryTime, FieldType.DateTime);
		var metaList = new[] {
			GetField(FieldId, FieldType.Long),
			entryTime,
			GetField(FieldLevelId, FieldType.Short),
			GetField(FieldSchemaId, FieldType.Int),
			GetField(FieldThreadId, FieldType.Int,false),
			GetField(FieldCallSite, FieldType.String,255, false, SearchableType.None),
			GetField(FieldJobId, FieldType.Long, false),
			GetField(FieldMethod, FieldType.String, 80, false, SearchableType.None),
			GetField(FieldLineNumber, FieldType.Int, 80, false, SearchableType.None),
			GetField(FieldMessage, FieldType.String, 255, false, SearchableType.None),
			GetField(FieldDescription, FieldType.LongString,false),
			GetIndex(false, new[] { entryTime })
		};
		var metaTable = GetTable((int)TableType.Log, TableType.Log.GetLogicalName(), TableType.Log);
		return GetTable(schemaName, provider, metaList.ToArray(), metaTable);
	}

	internal Table GetTest(string schemaName, DatabaseProvider provider)
	{
		// Code size: 272 (0x110)
		var metaList = new List<Meta>();
		var values = Enum.GetValues<FieldType>();
		var i = 0;
		foreach (var fieldType in values)
		{
			if (fieldType == FieldType.Undefined) continue;
			if (fieldType == FieldType.String)
			{
				// add 1 non searchable field 
				metaList.Add(GetField(FieldTestPrefix + i++, fieldType, 16, false, SearchableType.None));
				// add 2 searchable field 
				metaList.Add(GetField(FieldTestPrefix + i++, fieldType, 512, false, SearchableType.IgnoreCase));
				metaList.Add(GetField(FieldTestPrefix + i++, fieldType, 64, false, SearchableType.IgnoreDiacritic));
			}
			else metaList.Add(GetField(FieldTestPrefix + i++, fieldType, false));
		}
		var metaTest = GetTable((int)TableType.Test, TableType.Test.GetLogicalName(), TableType.Logical, false);
		return GetTable(schemaName, provider, metaList.ToArray(), metaTest);
	}

	internal Table GetCatalog(EntityType entityType, DatabaseProvider provider) 
	{
		// Code size: 236 (0xec)
		var ddlBuilder = provider.GetDdlBuilder();
		var fieldName = GetField(FieldSchemaName, FieldType.String);
		var defaultField = Meta.GetDefaultField(fieldName,FieldType.String);
		var tableType = entityType.ToTableType();
		var metaList = new[] { fieldName, GetField(FieldName, FieldType.String)	};
		var catalog = GetTable((int)tableType, tableType.GetLogicalName(), tableType);
		var tableObject = GetTable(string.Empty, provider, metaList.ToArray(), catalog, PhysicalType.View);
		tableObject.Columns[0] = tableObject.Columns[0].SetPhysicalName(
			ddlBuilder.GetPhysicalName(tableObject.GetField(FieldSchemaName) ?? defaultField, tableObject));
		tableObject.Columns[1] = tableObject.Columns[1].SetPhysicalName(
			ddlBuilder.GetPhysicalName(tableObject.GetField(FieldName) ?? defaultField, tableObject));
		return tableObject;
	}

	#region private methods 

	private static Table GetTable(string schemaName, DatabaseProvider provider, Meta[] metaArray, Meta metaTable, PhysicalType? physicalType=null)
	{
		// Code size: 127 (0x7f)
		var ddlBuilder = provider.GetDdlBuilder();
		var emptyTable = Meta.GetDefaultTable(metaTable);
		var emptySchema = Meta.GetDefaultSchema(GetSchema(0, schemaName), provider);
		var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);

		var spanMeta = metaArray.AsSpan();
		for (var i=0; i< spanMeta.Length; ++i) spanMeta[i] = Meta.Create(i,spanMeta[i]);
		return metaTable.ToTable(new ReadOnlySpan<Meta>(metaArray, 0, metaArray.Length),
				physicalType ?? PhysicalType.Table, ddlBuilder, physicalName, 0) ?? emptyTable;
	}

	private static Meta GetTable(int id, string name, TableType tableType, bool active=true) 
	{
		// Code size: 96 (0x60)
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, true);
        flags = Meta.SetTableReadonly(flags, true);
        flags = Meta.SetTableAllowAttributeExtension(flags, false); // no flexible attributes!
        switch (tableType)
		{ 
			case TableType.Meta:
                flags = Meta.SetPhysicalDeletion(flags, false);
                flags = Meta.SetTableCached(flags, true);
                flags = Meta.SetPreparedStatement(flags, false);
                break;
            case TableType.MetaId:
                flags = Meta.SetTableCached(flags, true);
                flags = Meta.SetPreparedStatement(flags, true);
                break;
        }
        return new(id, (byte)EntityType.Table, 0, (int)tableType, flags, name, null, null, active);
	}
	private static Meta GetSchema(int id, string name) => new(id, (byte)EntityType.Schema, 0, 0, 0L, name, null, null, true);
	private static Meta GetField(string name, FieldType fieldType, bool notNull)
		=> GetField(name, fieldType, 0, notNull, SearchableType.None);
	private static Meta GetField(string name, FieldType fieldType, int fieldSize)
		=> GetField(name, fieldType, fieldSize, true, SearchableType.None);
	private static Meta GetField(string name, FieldType fieldType)
		=> GetField(name, fieldType, 0, true,SearchableType.None);
	private static Meta GetField(string name, FieldType fieldType, int fieldSize, bool notNull, SearchableType searchableType)
	{
		var flags = 0L;
		var dataType = 0;
		flags = Meta.SetFieldNotNull(flags, notNull);
		flags = Meta.SetFieldSize(flags, fieldSize);
		if (fieldType == FieldType.String) flags = Meta.SetSearchableType(flags, searchableType);
		flags = Meta.SetEntityBaseline(flags, true);
		dataType = Meta.SetFieldType(dataType, fieldType);
		return new (0, (byte)EntityType.Field, 0, dataType, flags, name, null, null, true);
	}
	private static Meta GetIndex(bool unique, Meta[] lstMeta)
	{
		var fields = new List<string>();
		for (var i = 0; i < lstMeta.Length; ++i) fields.Add(lstMeta[i].Name);
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, true);
		flags = Meta.SetIndexUnique(flags, unique);
		return new (0, (byte)EntityType.Index, 0, 0, flags, string.Empty, null, Meta.GetColumnList(fields.ToArray()), true);
	}

	#endregion
}
