using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;

namespace Ring.Schema.Builders;

internal sealed class TableBuilder
{
	internal static readonly string FieldId = "id";
	internal static readonly string FieldSchemaId = "schema_id";
	internal static readonly string FieldObjectType = "object_type";
	internal static readonly string FieldValue = "value";
	internal static readonly string FieldReferenceId = "reference_id";
	internal static readonly string FieldDataType = "data_type";
	internal static readonly string FieldFlags = "flags";
	internal static readonly string FieldName = "name";
	internal static readonly string FieldTestPrefix = "test_";
	internal static readonly string FieldActive = "active";
	internal static readonly string FieldDescription = "description";
	internal static readonly string FieldLevelId = "level_id";
	internal static readonly string FieldEntryTime = "entry_time";
	internal static readonly string FieldThreadId = "thread_id";
	internal static readonly string FieldCallSite = "call_site";
	internal static readonly string FieldJobId = "job_id";
	internal static readonly string FieldMethod = "method";
	internal static readonly string FieldLineNumber = "line_number";
	internal static readonly string FieldMessage = "message";

#pragma warning disable CA1822, S2325
	internal Table GetMeta(string schemaName, DatabaseProvider provider) {
#pragma warning restore CA1822, S2325
		// Code size: 301 (0x12d)
		var metaList = new List<Meta> {
			GetField(FieldId, FieldType.Int),
			GetField(FieldSchemaId, FieldType.Int),
			GetField(FieldObjectType, FieldType.Byte),
			GetField(FieldReferenceId, FieldType.Int),
			GetField(FieldDataType, FieldType.Int),
			GetField(FieldFlags, FieldType.Long),
			GetField(FieldName, FieldType.String,30),
			GetField(FieldDescription, FieldType.LongString,false),
			GetField(FieldValue, FieldType.LongString,false),
			GetField(FieldActive, FieldType.Boolean)
		};
		metaList.Add(GetIndex(true, new [] { metaList[0], metaList[1], metaList[2], metaList[3] }));
		var metaTable = GetTable((int)TableType.Meta, TableType.Meta.GetLogicalName(), TableType.Meta);
		return GetTable(schemaName, provider, metaList.ToArray(), metaTable);
	}

#pragma warning disable CA1822, S2325 // Mark members as static
	internal Table GetMetaId(string schemaName, DatabaseProvider provider) 
	{
		// Code size: 177 (0xb1)
#pragma warning restore CA1822, S2325
		var metaList = new List<Meta> {
			GetField(FieldId, FieldType.Int),
			GetField(FieldSchemaId, FieldType.Int),
			GetField(FieldObjectType, FieldType.Byte),
			GetField(FieldValue, FieldType.Long),
		};
		var metaTable = GetTable((int)TableType.MetaId, TableType.MetaId.GetLogicalName(), TableType.MetaId);
		metaList.Add(GetIndex(true, new[] { metaList[0], metaList[1], metaList[2] }));
		return GetTable(schemaName, provider, metaList.ToArray(), metaTable);
	}

#pragma warning disable CA1822, S2325 // Mark members as static
	internal Table GetLog(string schemaName, DatabaseProvider provider) 
	{
		// Code size: 300 (0x12c)
#pragma warning restore CA1822, S2325
		var metaList = new List<Meta> {
			GetField(FieldId, FieldType.Long),
			GetField(FieldEntryTime, FieldType.DateTime),
			GetField(FieldLevelId, FieldType.Short),
			GetField(FieldSchemaId, FieldType.Int),
			GetField(FieldThreadId, FieldType.Int,false),
			GetField(FieldCallSite, FieldType.String,255, false, SearchableType.None),
			GetField(FieldJobId, FieldType.Long, false),
			GetField(FieldMethod, FieldType.String, 80, false, SearchableType.None),
			GetField(FieldLineNumber, FieldType.Int, 80, false, SearchableType.None),
			GetField(FieldMessage, FieldType.String, 255, false, SearchableType.None),
			GetField(FieldDescription, FieldType.String, 0, false, SearchableType.None)
		};
		var metaTable = GetTable((int)TableType.Log, TableType.Log.GetLogicalName(), TableType.Log);
		metaList.Add(GetIndex(false, new[] { metaList[1] }));
		return GetTable(schemaName, provider, metaList.ToArray(), metaTable);
	}

#pragma warning disable CA1822, S2325 // Mark members as static
	internal Table GetTest(string schemaName, DatabaseProvider provider)
	{
		// Code size: 272 (0x110)
#pragma warning restore CA1822, S2325
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
				metaList.Add(GetField(FieldTestPrefix + i++, fieldType, 64, false, SearchableType.IgnoreCaseAndDiacritics));
			}
			else metaList.Add(GetField(FieldTestPrefix + i++, fieldType, false));
		}
		var metaTest = GetTable((int)TableType.Test, TableType.Test.GetLogicalName(), TableType.Logical, false);
		return GetTable(schemaName, provider, metaList.ToArray(), metaTest);
	}

#pragma warning disable CA1822, S2325 // Mark members as static
	internal Table GetCatalog(EntityType entityType, DatabaseProvider provider) 
	{
		// Code size: 99 (0x63)
		var tableType = entityType.ToTableType();
		var metaList = new List<Meta>(){ GetField(provider.GetSchemaFieldName(entityType), FieldType.String) };
		if (entityType != EntityType.Schema)
			metaList.Add(GetField(provider.GetEntityFieldName(entityType), FieldType.String));
		var catalog = GetTable((int)tableType, provider.GetCatalogViewName(entityType), tableType);
		return GetTable(provider.GetCatalogSchema(), provider, metaList.ToArray(), catalog, PhysicalType.View);
	}
#pragma warning restore CA1822, S2325

#pragma warning disable CA1822, S2325 // Mark members as static
	internal Table GetMtm(Table partialTable, IDdlBuilder ddlBuilder, string physicalName, int objectIndex, Relation relation1, Relation relation2) 
	{
        // Code size: 226 (0xe2)
        // add @ prefix to logical name
        var metaTable = new Meta(0, (byte)EntityType.Table, 0, (int)TableType.Mtm, 0L, TableType.Mtm.GetLogicalName(partialTable.Name), null,null,true);
		var metaRelation1 = relation1.ToMeta(partialTable.Id);
		var metaRelation2 = relation2.ToMeta(partialTable.Id);
		// add index 
		var flags = 0L;
		var reltArr = new [] { metaRelation1.Name, metaRelation2.Name };
        var value = Meta.GetColumnList(reltArr);
		flags = Meta.SetIndexUnique(flags, true);
		var metaIndex = new Meta(0, (byte)EntityType.Index, 0, 0, flags, partialTable.Name, null, value, true);
		var metaArr = new [] { metaRelation1, metaRelation2, metaIndex };
		var segMent = new ReadOnlySpan<Meta>(metaArr, 0, 3);
		var result = metaTable.ToTable(segMent, PhysicalType.Table, ddlBuilder, physicalName, objectIndex) ?? partialTable;
		result.Relations[0] = relation1;
		result.Relations[1] = relation2;
		return result;
	}
#pragma warning restore CA1822, S2325

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

	private static Meta GetTable(int id, string name, TableType tableType, bool active=true) {
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, true);
        flags = Meta.SetTableReadonly(flags, true);
        flags = Meta.SetTableAllowAttributeExtension(flags, false); // no flexible attributes!
        flags = Meta.SetTableReadonly(flags, true);
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
