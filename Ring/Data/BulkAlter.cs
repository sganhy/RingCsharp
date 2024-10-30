using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;

namespace Ring.Data;

internal sealed class BulkAlter
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private readonly List<AlterQuery> _queries;
    private readonly Database _schema;
    private readonly Dictionary<EntityType, Dictionary<string, TableSpace>> _tablespaces; // <entityType, <tableName or default>, TableSpaceInfo>

    internal BulkAlter(Database schema)
    {
        _queries = new List<AlterQuery>();
        _schema = schema;
        _tablespaces = GetTableSpaceDictionary(schema);
    }

    internal List<AlterQuery> Queries => _queries;

    internal void CreateTable(string tableName)
    {
        var table = _schema.GetTable(tableName);
        if (table == null) ThrowInvalidObjectType(tableName);
        AppendDdlCommand(AlterQueryType.CreateTable, table);
        if (table.HasPrimaryKey()) AppendDdlCommand(AlterQueryType.CreatePrimaryKey, table);
        foreach (var index in table.Indexes) AppendDdlCommand(AlterQueryType.CreateIndex, table, index);
    }

    internal void CreateIndex(string tableName, string indexName)
    {
        var table = _schema.GetTable(tableName);
        if (table == null) ThrowInvalidObjectType(tableName);
        // TODO throw exception if null 
        var index = table.GetIndex(indexName);
        AppendDdlCommand(AlterQueryType.CreateIndex, table, index);
    }

    internal void AlterTableAdd(string tableName, string columnName)
    {
        var table = _schema.GetTable(tableName);
        if (table == null) ThrowInvalidObjectType(tableName);
        IColumn? field = table.GetField(columnName);
        IColumn? relation = table.GetRelation(columnName);
        if (field==null && relation==null) ThrowInvalidFieldName(tableName, columnName);
        AppendDdlCommand(AlterQueryType.AlterTableAddColumn, table, field??relation);
    }

    internal void Apply(IRingConnection connection)
    {
        // sort by Type
        _queries.Sort(delegate (AlterQuery q1, AlterQuery q2)
         {
             if (q1.Type == q2.Type) return q1.Id.CompareTo(q2.Id);
             return q1.Type.CompareTo(q2.Type);
         });
        var count = _queries.Count;
        for (var i = 0; i < count; ++i) connection.Execute(_queries[i]);
    }

    #region private methods

    private void AppendDdlCommand(AlterQueryType type, Table table, IColumn? column = null)
    {
        switch (type)
        {
            case AlterQueryType.CreatePrimaryKey:
                _queries.Add(new AlterQuery(table.Id, table, type, _schema.DdlBuiler, null, 
                    new Constraint(type.ToConstraintType(), table), null, GetTableSpace(table, EntityType.Constraint)));
                break;
            case AlterQueryType.CreateTable:
                _queries.Add(new AlterQuery(table.Id, table, type, _schema.DdlBuiler, null, 
                    null, null, GetTableSpace(table, EntityType.Table)));
                break;
        }
    }

    private void AppendDdlCommand(AlterQueryType type, Table table, Index? index)
    {
        switch (type)
        {
            case AlterQueryType.CreateIndex:
                if (table.Type != TableType.Business && index?.Unique==true && index.IsPrimaryKey(table)) break; 
                _queries.Add(new AlterQuery(table.Id, table, type, _schema.DdlBuiler, null,null, index, 
                    GetTableSpace(table, EntityType.Index)));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowInvalidObjectType(string objectType) =>
        throw new ArgumentException(string.Format(DefaultCulture,
                  ResourceHelper.GetErrorMessage(ResourceType.BulkAlterInvalidTableName), objectType));

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowInvalidFieldName(string objectType, string fieldName) =>
        throw new ArgumentException(string.Format(DefaultCulture,
                  ResourceHelper.GetErrorMessage(ResourceType.BulkAlterInvalidFieldName), fieldName, objectType));

    private TableSpace? GetTableSpace(Table table, EntityType entityType)
    {
        if (_tablespaces.ContainsKey(entityType))
        {
            var subDico = _tablespaces[entityType];
            var key = table.Name;
            if (subDico.ContainsKey(key)) return subDico[key];
            // find default tablespace: not connected to a specific table
            key = string.Empty;
            if (subDico.ContainsKey(key)) return subDico[key];
        }
        return null;
    }

    private static Dictionary<EntityType, Dictionary<string, TableSpace>> GetTableSpaceDictionary(Database schema)
    {
        var result = new Dictionary<EntityType, Dictionary<string, TableSpace>>()
        { 
            { EntityType.Index, new Dictionary<string, TableSpace>()},
            { EntityType.Table, new Dictionary<string, TableSpace>()},
            { EntityType.Constraint, new Dictionary<string, TableSpace>()}
        }; 
        var span = new ReadOnlySpan<TableSpace>(schema.TableSpaces);

        // constraint is consider as index for the moment, can be modified in the future
        foreach (var tablespace in span)
        {
            // if TableName.Length == 0 then it's a default tablespace 
            if (tablespace.TableName.Length == 0) AddTableSpace(result, tablespace, string.Empty);
            else 
            {
                var spanTables = new ReadOnlySpan<string>(tablespace.TableName);
                foreach (var table in spanTables) AddTableSpace(result, tablespace, table);
            }
        }

        // no tablespace for contrainst use the index one
        if (!result[EntityType.Constraint].ContainsKey(string.Empty) &&
            result[EntityType.Index].ContainsKey(string.Empty))
            result[EntityType.Constraint].Add(string.Empty, result[EntityType.Index][string.Empty]);

        return result;
    }

    private static void AddTableSpace(Dictionary<EntityType, Dictionary<string, TableSpace>> dico, TableSpace tablespace, 
        string tableName)
    {
        if (tablespace.Index && !dico[EntityType.Index].ContainsKey(tableName))
            dico[EntityType.Index].Add(tableName, tablespace);
        if (tablespace.Table && !dico[EntityType.Table].ContainsKey(tableName))
            dico[EntityType.Table].Add(tableName, tablespace);
        if (tablespace.Constraint && !dico[EntityType.Constraint].ContainsKey(tableName))
            dico[EntityType.Constraint].Add(tableName, tablespace);
    }

    #endregion

}
