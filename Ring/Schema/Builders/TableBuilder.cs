using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;
using System.Reflection;

namespace Ring.Schema.Builders;

internal sealed class TableBuilder
{
	// BUGS & IMPROVEMENTS:
	//     1) GetIndex() pass 1: Where memory actually gets duplicated; replaced Meta[] lstMeta parameter; Severity: High (Solved)
	private readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private readonly string FieldId = "id";
	private readonly string FieldSchemaId = "schema_id";
	private readonly string FieldName = "name";
	private readonly string FieldSchemaName = "schema_name";
	private readonly string FieldTestPrefix = "test_";
	private readonly string FieldDescription = "description";
	private readonly string FieldLevelId = "level_id";
	private readonly string FieldEntryTime = "entry_time";
	private readonly string FieldThreadId = "thread_id";
	private readonly string FieldCallSite = "call_site";
	private readonly string FieldJobId = "job_id";
	private readonly string FieldMethod = "method";
	private readonly string FieldLineNumber = "line_number";
	private readonly string FieldMessage = "message";

	private int _currentFieldId;
	private int _currentIndexId;
	private string _currentTableName= string.Empty;

	internal Table GetMeta(string schemaName, DatabaseProvider provider) // Code size: 57 (0x39)
		=> GetTable(schemaName, provider, ResourceHelper.GetMetaRows(TableType.Meta), ResourceHelper.GetMetaTable(TableType.Meta) ?? Meta.GetDefaultMeta(EntityType.Table));

	internal Table GetMetaId(string schemaName, DatabaseProvider provider) // Code size: 57 (0x39)
		=> GetTable(schemaName, provider, ResourceHelper.GetMetaRows(TableType.MetaId), ResourceHelper.GetMetaTable(TableType.MetaId) ?? Meta.GetDefaultMeta(EntityType.Table));


	internal Table GetLog(string schemaName, DatabaseProvider provider) 
	{
		// Code size: 316 (0x13c)
		var metaTable = GetTable((int)TableType.Log, TableType.Log.GetLogicalName(), TableType.Log);
		var metaList = new[] {
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
			GetField(FieldDescription, FieldType.LongString,false),
			GetIndex(false, new[] { FieldEntryTime })
		};
		return GetTable(schemaName, provider, metaList, metaTable);
	}

	internal Table GetTest(string schemaName, DatabaseProvider provider, PhysicalType physicalType)
	{
		// Code size: 279 (0x117)
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
		var metaTest = GetTable((int)TableType.Test, TableType.Test.GetLogicalName(), TableType.Test);
		return GetTable(schemaName, provider, metaList.ToArray(), metaTest, physicalType);
	}

	internal Table GetCatalog(EntityType entityType, DatabaseProvider provider) 
	{
		// Code size: 231 (0xe7)
		var ddlBuilder = provider.GetDdlBuilder();
		var fieldName = GetField(FieldSchemaName, FieldType.String);
		var defaultField = Meta.GetDefaultField(fieldName,FieldType.String);
		var tableType = entityType.ToTableType();
		var metaArr = new[] { fieldName, GetField(FieldName, FieldType.String)	};
		var catalog = GetTable((int)tableType, tableType.GetLogicalName(), tableType);
		var tableObject = GetTable(string.Empty, provider, metaArr, catalog, PhysicalType.View);
		tableObject.Columns[0] = tableObject.Columns[0].SetPhysicalName(
			ddlBuilder.GetPhysicalName(tableObject.GetField(FieldSchemaName) ?? defaultField, tableObject));
		tableObject.Columns[1] = tableObject.Columns[1].SetPhysicalName(
			ddlBuilder.GetPhysicalName(tableObject.GetField(FieldName) ?? defaultField, tableObject));
		return tableObject;
	}

	#region private methods 

#pragma warning disable CA1822 // Mark members as static
	private Table GetTable(string schemaName, DatabaseProvider provider, Meta[] metaArray, Meta metaTable, PhysicalType? physicalType=null)
	{
#pragma warning restore CA1822
		// Code size: 96 (0x60)
		var ddlBuilder = provider.GetDdlBuilder();
		var emptyTable = Meta.GetDefaultTable(metaTable);
		var emptySchema = Meta.GetDefaultSchema(GetSchema(0, schemaName), provider);
		var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);
		var tableType = metaTable.DataType.ToTableType();
		var table = metaTable.ToTable(new ReadOnlySpan<Meta>(metaArray, 0, metaArray.Length), physicalType ?? PhysicalType.Table, ddlBuilder, physicalName, tableType.GetObjectIndex()) ?? emptyTable;
		return table;
	}

	private Meta GetTable(int id, string name, TableType tableType) 
	{
		// Code size: 115 (0x73)
		var flags = 0L;
		_currentFieldId = 0;
		_currentIndexId = 0;
		_currentTableName = name;
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
        return new(id, (byte)EntityType.Table, 0, (int)tableType, flags, name, null, null, true);
	}
	private static Meta GetSchema(int id, string name) => new(id, (byte)EntityType.Schema, 0, 0, 0L, name, null, null, true);
	private Meta GetField(string name, FieldType fieldType, bool notNull) => GetField(name, fieldType, 0, notNull, SearchableType.None); // Code size: 11 (0xb)
	private Meta GetField(string name, FieldType fieldType) => GetField(name, fieldType, 0, true,SearchableType.None); // Code size: 11 (0xb)
	private Meta GetField(string name, FieldType fieldType, int fieldSize, bool notNull, SearchableType searchableType, string? defaultValue = null)
	{
		// Code size: 100 (0x64) - no virtual calls
		var flags = 0L;
		var dataType = 0;
		flags = Meta.SetFieldNotNull(flags, notNull);
		flags = Meta.SetFieldSize(flags, fieldSize);
		if (fieldType == FieldType.String) flags = Meta.SetSearchableType(flags, searchableType);
		flags = Meta.SetEntityBaseline(flags, true);
		dataType = Meta.SetFieldType(dataType, fieldType);
		return new (++_currentFieldId, (byte)EntityType.Field, 0, dataType, flags, name, null, defaultValue, true);
	}
	private Meta GetIndex(bool unique, params string[] fieldNames)
	{
		// Code size: 100 (0x64)
		var flags = 0L;
		++_currentIndexId;
		flags = Meta.SetEntityBaseline(flags, true);
		flags = Meta.SetIndexUnique(flags, unique);
		var name = _currentTableName + '_' + _currentIndexId.ToString(DefaultCulture).PadLeft(3, '0');
		return new(_currentIndexId, (byte)EntityType.Index, 0, 0, flags, name, null, Meta.GetColumnList(fieldNames), true);
	}

	#endregion
}
