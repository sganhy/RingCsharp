using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Reflection;

namespace Ring.Schema.Builders;

internal sealed class TableBuilder
{
	// BUGS & IMPROVEMENTS:
	//     1) GetIndex() pass 1: Where memory actually gets duplicated; replaced Meta[] lstMeta parameter; Severity: High (Solved)
	private readonly string FieldName = "name";
	private readonly string FieldSchemaName = "schema_name";
	private int _currentFieldId;

	internal Table GetMeta(string schemaName, DatabaseProvider provider) // Code size: 49 (0x31)
		=> GetTable(schemaName, provider, ResourceHelper.GetMetaRows(TableType.Meta), ResourceHelper.GetMetaTable(TableType.Meta) ?? Meta.GetDefaultMeta(EntityType.Table), PhysicalType.Table);
	internal Table GetMetaId(string schemaName, DatabaseProvider provider) // Code size: 49 (0x31)
		=> GetTable(schemaName, provider, ResourceHelper.GetMetaRows(TableType.MetaId), ResourceHelper.GetMetaTable(TableType.MetaId) ?? Meta.GetDefaultMeta(EntityType.Table), PhysicalType.Table);
	internal Table GetLog(string schemaName, DatabaseProvider provider) // Code size: 49 (0x31)
		=> GetTable(schemaName, provider, ResourceHelper.GetMetaRows(TableType.Log), ResourceHelper.GetMetaTable(TableType.Log) ?? Meta.GetDefaultMeta(EntityType.Table), PhysicalType.Table);
	internal Table GetTest(string schemaName, DatabaseProvider provider, PhysicalType physicalType) // Code size: 49 (0x31)
		=> GetTable(schemaName, provider, ResourceHelper.GetMetaRows(TableType.Test), ResourceHelper.GetMetaTable(TableType.Test) ?? Meta.GetDefaultMeta(EntityType.Table), physicalType);

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
	private Table GetTable(string schemaName, DatabaseProvider provider, Meta[] metaArray, Meta metaTable, PhysicalType physicalType)
	{
#pragma warning restore CA1822
		// Code size: 96 (0x60)
		var ddlBuilder = provider.GetDdlBuilder();
		var emptyTable = Meta.GetDefaultTable(metaTable);
		var emptySchema = Meta.GetDefaultSchema(GetSchema(0, schemaName), provider);
		var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);
		var tableType = metaTable.DataType.ToTableType();
		var table = metaTable.ToTable(new ReadOnlySpan<Meta>(metaArray, 0, metaArray.Length), physicalType, ddlBuilder, physicalName, tableType.GetObjectIndex()) ?? emptyTable;
		return table;
	}

	private Meta GetTable(int id, string name, TableType tableType) 
	{
		// Code size: 49 (0x31)
		var flags = 0L;
		_currentFieldId = 0;
		flags = Meta.SetEntityBaseline(flags, true);
        flags = Meta.SetTableReadonly(flags, true);
        flags = Meta.SetTableAllowAttributeExtension(flags, false); // no flexible attributes!
        return new(id, (byte)EntityType.Table, 0, (int)tableType, flags, name, null, null, true);
	}
	private static Meta GetSchema(int id, string name) => new(id, (byte)EntityType.Schema, 0, 0, 0L, name, null, null, true);
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

	#endregion
}
