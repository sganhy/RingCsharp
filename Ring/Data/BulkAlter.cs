﻿using Ring.Data.Enums;
using Ring.Data.Models;
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
        AppendQuery(AlterQueryType.CreateTable, table);
    }

    internal void AlterTableAdd(string tableName, string columnName)
    {
        var table = _schema.GetTable(tableName);
        if (table == null) ThrowInvalidObjectType(tableName);
        IColumn? field = table.GetField(columnName);
        IColumn? relation = table.GetRelation(columnName);
        if (field==null && relation==null) ThrowInvalidFieldName(tableName, columnName);
        AppendQuery(AlterQueryType.AlterTableAddColumn, table, field??relation);
    }

    #region private methods

    private void AppendQuery(AlterQueryType type, Table table, IColumn? column=null) 
        => _queries.Add(new AlterQuery(table, type, _schema.DdlBuiler, column));

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