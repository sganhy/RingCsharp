using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders;

internal sealed class TableBuilder
{
    internal static readonly string SystemTablePrefix = "@";
    internal static readonly string TableMetaIdName = SystemTablePrefix + "meta_id";
    internal static readonly string TableMetaName = SystemTablePrefix + "meta";
    internal static readonly string TableLogName = SystemTablePrefix + "log";
    internal static readonly string FieldId = "id";
    internal static readonly string FieldSchemaId = "schema_id";
    internal static readonly string FieldObjectType = "object_type";
    internal static readonly string FieldValue = "value";
    internal static readonly string FieldReferenceId = "reference_id";
    internal static readonly string FieldDataType = "data_type";
    internal static readonly string FieldFlags = "flags";
    internal static readonly string FieldName = "name";
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

        var metaList = new List<Meta> {
            GetField(FieldId, FieldType.Int),
            GetField(FieldSchemaId, FieldType.Int),
            GetField(FieldObjectType, FieldType.Byte),
            GetField(FieldReferenceId, FieldType.Int),
            GetField(FieldDataType, FieldType.Int),
            GetField(FieldFlags, FieldType.Long),
            GetField(FieldName, FieldType.String,30),
            GetField(FieldDescription, FieldType.String,false),
            GetField(FieldValue, FieldType.String,false),
            GetField(FieldActive, FieldType.Boolean)
        };
        metaList.Add(GetUniqueIndex(4, metaList));
        var metaTable = GetTable((int)TableType.Meta, TableMetaName);
        var result = GetTable(schemaName, provider, metaList.ToArray(), metaTable, TableType.Meta);
        result.LoadColumnInformation();
        result.LoadRelationRecordIndex();
        return result;
    }

#pragma warning disable CA1822, S2325 // Mark members as static
    internal Table GetMetaId(string schemaName, DatabaseProvider provider) {
#pragma warning restore CA1822, S2325
        var metaList = new List<Meta> {
            GetField(FieldId, FieldType.Int),
            GetField(FieldSchemaId, FieldType.Int),
            GetField(FieldObjectType, FieldType.Byte),
            GetField(FieldValue, FieldType.Long),
        };
        var metaTable = GetTable((int)TableType.MetaId, TableMetaIdName);
        metaList.Add(GetUniqueIndex(3, metaList));
        var result= GetTable(schemaName, provider, metaList.ToArray(), metaTable, TableType.MetaId);
        result.LoadColumnInformation();
        result.LoadRelationRecordIndex();
        return result;
    }

#pragma warning disable CA1822, S2325 // Mark members as static
    internal Table GetLog(string schemaName, DatabaseProvider provider) {
#pragma warning restore CA1822, S2325
        var metaList = new List<Meta> {
            GetField(FieldId, FieldType.Long),
            GetField(FieldEntryTime, FieldType.DateTime),
            GetField(FieldLevelId, FieldType.Short),
            GetField(FieldSchemaId, FieldType.Int),
            GetField(FieldThreadId, FieldType.Int,false),
            GetField(FieldCallSite, FieldType.String,255, false),
            GetField(FieldJobId, FieldType.Long, false),
            GetField(FieldMethod, FieldType.String, 80, false),
            GetField(FieldLineNumber, FieldType.Int, 80, false),
            GetField(FieldMessage, FieldType.String, 255, false),
            GetField(FieldDescription, FieldType.String, 0, false),
        };
        var metaTable = GetTable((int)TableType.Log, TableLogName);
        var result = GetTable(schemaName, provider, metaList.ToArray(), metaTable, TableType.Log);
        result.LoadColumnInformation();
        result.LoadRelationRecordIndex();
        return result;
    }

#pragma warning disable CA1822, S2325 // Mark members as static
    internal Table GetCatalog(EntityType entityType, DatabaseProvider provider) {
#pragma warning restore CA1822, S2325
        var tableType = GetTablType(entityType);
        var metaList = new List<Meta>(){ GetField(provider.GetSchemaFieldName(entityType), FieldType.String) };
        if (entityType != EntityType.Schema)
            metaList.Add(GetField(provider.GetEntityFieldName(entityType), FieldType.String));
        var catalog = GetTable((int)tableType, provider.GetCatalogViewName(entityType));
        var result = GetTable(provider.GetCatalogSchema(), provider, metaList.ToArray(), catalog, 
            tableType,PhysicalType.View);
        result.LoadColumnInformation();
        result.LoadRelationRecordIndex();
        return result;
    }

#pragma warning disable CA1822, S2325 // Mark members as static
    internal Table GetMtm(Table partialTable, string physicalName) { 
#pragma warning restore CA1822, S2325

        // add @ prefix to logical name
        var metaTable = new Meta(0, SystemTablePrefix + partialTable.Name, EntityType.Table);
        var metaRelation = new Meta(0, partialTable.Name, EntityType.Relation);
        // add index 
        var flags = 0L;
        var value = Meta.SetIndexedColumns(new string[] { partialTable.Name, partialTable.Name });
        flags = Meta.SetIndexUnique(flags, true);
        var metaIndex = new Meta(0,partialTable.Name,EntityType.Index, flags, value);
        var metaArr = new Meta[] { metaRelation, metaRelation, metaIndex };
        var segMent = new ArraySegment<Meta>(metaArr, 0, 3);
        var result = metaTable.ToTable(segMent, TableType.Mtm, PhysicalType.Table, physicalName) ?? partialTable;
        result.RecordIndexes[0]=0; // columnMapper 4 Mtm table is always {0,1}
        result.RecordIndexes[1]=1; // columnMapper 4 Mtm table is always {0,1}
        return result;
    }


    #region private methods 

    private static Table GetTable(string schemaName, DatabaseProvider provider, Meta[] metaArray, Meta metaTable, TableType tableType, 
        PhysicalType? physicalType=null)
    {
        var ddlBuilder = provider.GetDdlBuilder();
        var emptyTable = Meta.GetEmptyTable(metaTable, tableType);
        var emptySchema = Meta.GetEmptySchema(GetSchema(0, schemaName), provider);
        var spanMeta = metaArray.AsSpan();
        for (var i=0; i< spanMeta.Length; ++i) spanMeta[i] = new Meta(i,spanMeta[i]);
        return metaTable.ToTable(new ArraySegment<Meta>(metaArray, 0, metaArray.Length),
                tableType, physicalType ?? PhysicalType.Table, ddlBuilder.GetPhysicalName(emptyTable, emptySchema)) ?? emptyTable;
    }

    private static Meta GetTable(int id, string name) {
        var flags = 0L;
        flags = Meta.SetEntityBaseline(flags, true);
        return new(id, (byte)EntityType.Table, 0, 0, flags, name, null, null, true);
    }
    private static Meta GetSchema(int id, string name) => new(id, name, EntityType.Schema);
    private static Meta GetField(string name, FieldType fieldType, bool notNull)
        => GetField(name, fieldType, 0, notNull);
    private static Meta GetField(string name, FieldType fieldType, int fieldSize)
        => GetField(name, fieldType, fieldSize, true);
    private static Meta GetField(string name, FieldType fieldType)
        => GetField(name, fieldType, 0, true);
    private static Meta GetField(string name, FieldType fieldType, int fieldSize, bool notNull)
    {
        var flags = 0L;
        var dataType = 0;
        flags = Meta.SetFieldCaseSensitive(flags, true);
        flags = Meta.SetFieldNotNull(flags, notNull);
        flags = Meta.SetFieldSize(flags, fieldSize);
        flags = Meta.SetEntityBaseline(flags, true);
        dataType = Meta.SetFieldType(dataType, fieldType);
        return new (0, (byte)EntityType.Field, 0, dataType, flags, name, null, null, true);
    }
    private static Meta GetUniqueIndex(int firstField, List<Meta> lstMeta)
    {
        
        var fields = new List<string>();
        for (var i = 0; i < firstField; ++i) fields.Add(lstMeta[i].Name);
        var flags = 0L;
        flags = Meta.SetIndexUnique(flags, true);
        var meta = new Meta(0, string.Empty, EntityType.Index, flags, Meta.SetIndexedColumns(fields.ToArray()));
        return meta;
    }
    private static TableType GetTablType(EntityType entityType)
    {
        TableType result;
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (entityType)
        {
            case EntityType.Table:
                result = TableType.TableCatalog;
                break;
            case EntityType.Schema:
                result = TableType.SchemaCatalog;
                break;
            case EntityType.Tablespace:
                result = TableType.TableCatalog;
                break;
            default:
                result = TableType.Logical;
                break;
        }
#pragma warning restore IDE0066
        return result;
    }

    #endregion
}
