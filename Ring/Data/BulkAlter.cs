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

namespace Ring.Data;

internal sealed class BulkAlter
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private readonly List<AlterQuery> _queries;
    private readonly Database _schema;

    internal BulkAlter(Database schema)
    {
        _queries = new List<AlterQuery>();
        _schema = schema;
    }

    internal void CreateTable(string tableName)
    {
        var table = _schema.GetTable(tableName);
        if (table == null) ThrowInvalidObjectType(tableName);
        AppendDdlCommand(AlterQueryType.CreateTable, table);
        if (table.HasPrimaryKey()) AppendDdlCommand(AlterQueryType.CreatePrimaryKey, table);
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
             return q1.Type.CompareTo(q2.Type);
         });
        var count = _queries.Count;
        for (var i = 0; i < count; ++i) connection.Execute(_queries[i]);
    }

    #region private methods

    private void AppendDdlCommand(AlterQueryType type, Table table, IColumn? column = null)
    {
        if (type == AlterQueryType.CreatePrimaryKey)
            _queries.Add(new AlterQuery(table, type, _schema.DdlBuiler, column, new Constraint(type.ToConstraintType(), table)));
        else _queries.Add(new AlterQuery(table, type, _schema.DdlBuiler, column, null));
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

    #endregion 
}
