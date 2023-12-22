using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders;

internal sealed class TableBuilder
{
    private int _ItemId = 1;
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

    internal Table GetMeta(string schemaName, DatabaseProvider provider)
    {
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
        result.LoadMapper();
        return result;
    }
    internal Table GetMetaId(string schemaName, DatabaseProvider provider)
    {
        var metaList = new List<Meta> {
            GetField(FieldId, FieldType.Int),
            GetField(FieldSchemaId, FieldType.Int),
            GetField(FieldObjectType, FieldType.Byte),
            GetField(FieldValue, FieldType.Long),
        };
        var metaTable = GetTable((int)TableType.MetaId, TableMetaIdName);
        metaList.Add(GetUniqueIndex(3, metaList));
        var result= GetTable(schemaName, provider, metaList.ToArray(), metaTable, TableType.MetaId);
        result.LoadMapper();
        return result;
    }
    internal Table GetLog(string schemaName, DatabaseProvider provider)
    {
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
        result.LoadMapper();
        return result;
    }
    internal static Table GetMtmTable(Table partialTable, string physicalName)
    {
        // add @ prefix to logical name
        var metaTable = new Meta(SystemTablePrefix + partialTable.Name);
        metaTable.SetEntityType(EntityType.Table);
        var metaRelation = new Meta(partialTable.Name);
        metaRelation.SetEntityType(EntityType.Relation);
        // add index 
        var metaIndex = new Meta(partialTable.Name);
        metaIndex.SetEntityType(EntityType.Index);
        metaIndex.SetIndexUnique(true);
        metaIndex.SetIndexedColumns(new string[] { partialTable.Name, partialTable.Name });
        var metaArr = new Meta[] { metaRelation, metaRelation, metaIndex };
        var segMent = new ArraySegment<Meta>(metaArr, 0, 3);
        var result = MetaExtensions.ToTable(metaTable, segMent, TableType.Mtm, physicalName) ?? partialTable;
        result.ColumnMapper[1] = 1; // columnMapper 4 Mtm table is always {0,1}
        return result;
    }

    #region private methods 

    private static Table GetTable(string schemaName, DatabaseProvider provider, Meta[] metaArray, Meta metaTable, TableType tableType)
    {
        var ddlBuilder = provider.GetDdlBuilder();
        var emptyTable = MetaExtensions.GetEmptyTable(metaTable, TableType.Meta);
        var emptySchema = MetaExtensions.GetEmptySchema(GetSchema(0, schemaName), provider);
        metaTable.SetEntityBaseline(true);
        return metaTable.ToTable(new ArraySegment<Meta>(metaArray, 0, metaArray.Length),
                tableType, ddlBuilder.GetPhysicalName(emptyTable, emptySchema)) ?? emptyTable;
    }
    private static Meta GetTable(int id, string name) => new()
    {
        Id = id,
        Name = name,
        ObjectType = (byte)EntityType.Table
    };
    private static Meta GetSchema(int id, string name) => new()
    {
        Id = id,
        Name = name,
        ObjectType = (byte)EntityType.Schema
    };
    private Meta GetField(string name, FieldType fieldType, bool notNull)
        => GetField(name, fieldType, 0, notNull);
    private Meta GetField(string name, FieldType fieldType, int fieldSize)
        => GetField(name, fieldType, fieldSize, true);
    private Meta GetField(string name, FieldType fieldType)
        => GetField(name, fieldType, 0, true);
    private Meta GetField(string name, FieldType fieldType, int fieldSize, bool notNull)
    {
        var meta = new Meta
        {
            Id = _ItemId++,
            Name = name,
            Flags = 0,
            ObjectType = (byte)EntityType.Field
        };
        meta.SetFieldNotNull(notNull);
        meta.SetFieldSize(fieldSize);
        meta.SetFieldType(fieldType);
        meta.SetEntityBaseline(true);
        return meta;
    }
    private static Meta GetUniqueIndex(int firstField, List<Meta> lstMeta)
    {
        var meta = new Meta
        {
            Name = string.Empty,
            Flags = 0,
            ObjectType = (byte)EntityType.Index
        };
        var fields = new List<string>();
        for (var i = 0; i < firstField; ++i)
            fields.Add(lstMeta[i].Name);
        meta.SetIndexUnique(true);
        meta.SetIndexedColumns(fields.ToArray());
        return meta;
    }

    #endregion
}
