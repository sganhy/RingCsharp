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

    // catalogs 
    private static readonly Dictionary<EntityType,Catalog> _postreSqlCatalog= new () {
        { EntityType.Table, new Catalog { FieldSchemaName="table_schema", FieldEntityName= "table_name", ViewName="tables" } }
    };


#pragma warning disable CA1822 // Mark members as static
    internal Table GetMeta(string schemaName, DatabaseProvider provider)
    {
#pragma warning restore CA1822 

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
        result.LoadColumnMapper();
        return result;
    }

#pragma warning disable CA1822 // Mark members as static
    internal Table GetMetaId(string schemaName, DatabaseProvider provider)
    {
#pragma warning restore CA1822 
        var metaList = new List<Meta> {
            GetField(FieldId, FieldType.Int),
            GetField(FieldSchemaId, FieldType.Int),
            GetField(FieldObjectType, FieldType.Byte),
            GetField(FieldValue, FieldType.Long),
        };
        var metaTable = GetTable((int)TableType.MetaId, TableMetaIdName);
        metaList.Add(GetUniqueIndex(3, metaList));
        var result= GetTable(schemaName, provider, metaList.ToArray(), metaTable, TableType.MetaId);
        result.LoadColumnMapper();
        return result;
    }
#pragma warning disable CA1822 // Mark members as static
    internal Table GetLog(string schemaName, DatabaseProvider provider)
#pragma warning restore CA1822
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
        result.LoadColumnMapper();
        return result;
    }

#pragma warning disable CA1822 // Mark members as static
    internal Table GetCatalog(EntityType entityType, DatabaseProvider provider)
    {
#pragma warning restore CA1822
        var tableType = GetTablType(entityType);
        var metaList = new List<Meta>(){ GetField(GetSchemaFieldName(provider,entityType), FieldType.String) };
        if (entityType != EntityType.Schema)
            metaList.Add(GetField(GetEntityFieldName(provider, entityType), FieldType.String));
        var catalog = GetTable((int)tableType, GetCatalogViewName(provider, entityType));
        var result = GetTable(GetCatalogSchemaName(provider), provider, metaList.ToArray(), catalog, 
            tableType,PhysicalType.View);
        result.LoadColumnMapper();
        return result;
    }

    internal static Table GetMtm(Table partialTable, string physicalName)
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
        var result = MetaExtensions.ToTable(metaTable, segMent, TableType.Mtm, PhysicalType.Table, physicalName) ?? partialTable;
        result.ColumnMapper[0]=0; // columnMapper 4 Mtm table is always {0,1}
        result.ColumnMapper[1]=1; // columnMapper 4 Mtm table is always {0,1}
        return result;
    }

    #region private methods 

    private static Table GetTable(string schemaName, DatabaseProvider provider, Meta[] metaArray, Meta metaTable, TableType tableType, 
        PhysicalType? physicalType=null)
    {
        var ddlBuilder = provider.GetDdlBuilder();
        var emptyTable = MetaExtensions.GetEmptyTable(metaTable, tableType);
        var emptySchema = MetaExtensions.GetEmptySchema(GetSchema(0, schemaName), provider);
        metaTable.SetEntityBaseline(true);
        for (var i=0; i<metaArray.Length; ++i) metaArray[i].SetEntityId(i);
        return metaTable.ToTable(new ArraySegment<Meta>(metaArray, 0, metaArray.Length),
                tableType, physicalType ?? PhysicalType.Table, ddlBuilder.GetPhysicalName(emptyTable, emptySchema)) ?? emptyTable;
    }
    private static Meta GetTable(int id, string name) 
    {
        var result = new Meta();
        result.SetEntityId(id);
        result.SetEntityName(name);
        result.SetEntityType(EntityType.Table);
        return result;
    }
    private static Meta GetSchema(int id, string name) 
    {
        var result = new Meta();
        result.SetEntityId(id);
        result.SetEntityName(name);
        result.SetEntityType(EntityType.Schema);
        return result;
    }
    private static Meta GetField(string name, FieldType fieldType, bool notNull)
        => GetField(name, fieldType, 0, notNull);
    private static Meta GetField(string name, FieldType fieldType, int fieldSize)
        => GetField(name, fieldType, fieldSize, true);
    private static Meta GetField(string name, FieldType fieldType)
        => GetField(name, fieldType, 0, true);
    private static Meta GetField(string name, FieldType fieldType, int fieldSize, bool notNull)
    {
        var meta = new Meta();
        meta.SetEntityType(EntityType.Field);
        meta.SetEntityName(name);
        meta.SetFieldCaseSensitive(true);
        meta.SetFieldNotNull(notNull);
        meta.SetFieldSize(fieldSize);
        meta.SetFieldType(fieldType);
        meta.SetEntityBaseline(true);
        return meta;
    }
    private static Meta GetUniqueIndex(int firstField, List<Meta> lstMeta)
    {
        var meta = new Meta();
        var fields = new List<string>();
        for (var i = 0; i < firstField; ++i) fields.Add(lstMeta[i].GetEntityName());
        meta.SetEntityName(string.Empty);
        meta.SetEntityType(EntityType.Index);
        meta.SetIndexUnique(true);
        meta.SetIndexedColumns(fields.ToArray());
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
    private static string GetCatalogSchemaName(DatabaseProvider provider)
    {
        string result;
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (provider)
        {
            case DatabaseProvider.PostgreSql:
            case DatabaseProvider.MySql:
            case DatabaseProvider.SqlServer:
                result = "information_schema";
                break;
            default:
                result = string.Empty;
                break;
        }
#pragma warning restore IDE0066
        return result;
    }
    private static string GetCatalogViewName(DatabaseProvider provider, EntityType entityType)
    {
        var result = string.Empty;
        switch (provider)
        {
            case DatabaseProvider.PostgreSql:
            case DatabaseProvider.MySql:
                result = _postreSqlCatalog[entityType].ViewName;
                break;
        }
        return result;
    }
    private static string GetSchemaFieldName(DatabaseProvider provider, EntityType entityType)
    {
        var result = string.Empty;
        switch (provider)
        {
            case DatabaseProvider.PostgreSql:
            case DatabaseProvider.MySql:
                result = _postreSqlCatalog[entityType].FieldSchemaName;
                break;
        }
        return result;
    }

    private static string GetEntityFieldName(DatabaseProvider provider, EntityType entityType)
    {
        var result = string.Empty;
        switch (provider)
        {
            case DatabaseProvider.PostgreSql:
            case DatabaseProvider.MySql:
                result = _postreSqlCatalog[entityType].FieldEntityName;
                break;
        }
        return result;
    }

    #endregion
}
