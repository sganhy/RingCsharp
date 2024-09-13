﻿using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Globalization;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDmlBuilder : BaseSqlBuilder, IDmlBuilder
{
    // commands
    private static readonly string DmlInsert = @"INSERT INTO ";
    private static readonly string DmlValues = @") VALUES (";
    private static readonly string DmlUpdate = @"UPDATE ";
    private static readonly string DmlSet = @" SET {0}";
    private static readonly string DmlDelete = @"DELETE FROM ";
    private static readonly string DmlWhere = @" WHERE ";
    private static readonly string DmlAnd = " AND ";
    private static readonly char DmlEqual = '=';
    private static readonly string FirstParameter = @"1";

    private string[] _tableIndex;
    private string?[] _tableDelete;
    private string?[] _tableInsert;
    private string?[] _tableUpdate;
    private readonly IDdlBuilder _ddlBuilder;
    private readonly Field _defaultField;

    protected BaseDmlBuilder() 
    {
        _tableIndex = Array.Empty<string>();
        _tableDelete = Array.Empty<string?>();
        _tableInsert = Array.Empty<string?>();
        _tableUpdate = Array.Empty<string?>();
        _ddlBuilder = Provider.GetDdlBuilder();
        _defaultField = MetaExtensions.GetEmptyField(new Meta(string.Empty), FieldType.Int);
    }

    public abstract string VariableNameTemplate { get; }

    public void Init(DbSchema schema)
    {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        _tableIndex = GetTableIndex(schema);
        _tableDelete = new string?[mtmCount + tableCount];
        _tableInsert = new string?[mtmCount + tableCount];
        _tableUpdate = new string?[mtmCount + tableCount];
    }

    public string Insert(Table table) {
        // avoid lock
        var index = _tableIndex.GetIndex(table.Name);
        var result = _tableInsert[index];
        if (result==null)
        {
            result = BuildInsert(table);
            _tableInsert[index] = result;
        }
        return result;
    }

    public string Update(Table table) {
        var index = _tableIndex.GetIndex(table.Name);
        var result = _tableUpdate[index];
        if (result==null)
        {
            result = BuildUpdate(table);
            _tableUpdate[index] = result;
        }
        return result;
    }

    public string Delete(Table table) {
        // avoid lock
        var index = _tableIndex.GetIndex(table.Name);
        var result = _tableDelete[index];
        if (result==null)
        {
            result = BuildDelete(table);
            _tableDelete[index] = result;
        }
        return result;
    }

    #region private methods 

    private string BuildInsert(Table table)
    {
        var result = new StringBuilder();
        var columnCount = table.Columns.Length;

        result.Append(DmlInsert);
        result.Append(table.PhysicalName);
        result.Append(SqlSpace);
        result.Append(StartParenthesis);
        for (var i = 0; i<columnCount; ++i)
        {
            var column = table.Columns[i];
            if (column.Type == EntityType.Relation) result.Append(_ddlBuilder.GetPhysicalName((Relation)column));
            else result.Append(_ddlBuilder.GetPhysicalName((Field)column));
            result.Append(ColumnDelimiter);
        }
        if (columnCount>0) --result.Length;
        result.Append(DmlValues);
        for (var i=1; i<=columnCount; ++i)
        {
            result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, i.ToString(CultureInfo.InvariantCulture));
            result.Append(ColumnDelimiter);
        }
        if (columnCount > 0) --result.Length;
        result.Append(EndParenthesis);
        return result.ToString();
    }

    private string BuildDelete(Table table)
    {
        var result = new StringBuilder();
        result.Append(DmlDelete);
        result.Append(table.PhysicalName);
        result.Append(DmlWhere);
        switch (table.Type)
        {
            case TableType.Business:
            case TableType.Lexicon:
                result.Append(_ddlBuilder.GetPhysicalName(table.GetPrimaryKey()??_defaultField));
                result.Append(DmlEqual);
                result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
                break;
            case TableType.Mtm:
            case TableType.Meta:
            case TableType.MetaId:
                {
                    var variableIndex = 1;
                    var firstUniqueIndex = table.GetFirstKey(); // cannot be null here 
                    var keyCount = firstUniqueIndex?.Columns.Length??0;
                    for (var i=0; i<keyCount; ++i, ++variableIndex)
                    {
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
                        var field = MetaExtensions.GetEmptyField(new Meta(firstUniqueIndex?.Columns[i]
                                        ?? string.Empty), FieldType.Int);
#pragma warning restore S2589 
                        result.Append(_ddlBuilder.GetPhysicalName(field));
                        result.Append(DmlEqual);
                        result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
                            variableIndex.ToString(CultureInfo.InvariantCulture));
                        // last element?
                        if (i< keyCount-1) result.Append(DmlAnd);
                    }
                }
                break;
            default:
                throw new NotImplementedException();
        }
        return result.ToString();
    }

    private string BuildUpdate(Table table)
    {
        var result = new StringBuilder();
        result.Append(DmlUpdate);
        result.Append(table.PhysicalName);
        result.Append(DmlSet);
        result.Append(DmlWhere);
        switch (table.Type)
        {
            case TableType.Business:
            case TableType.Lexicon:
                result.Append(_ddlBuilder.GetPhysicalName(table.GetPrimaryKey() ?? _defaultField));
                result.Append(DmlEqual);
                result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
                break;
            case TableType.Meta:
            case TableType.MetaId:
                {
                    var variableIndex = 1;
                    var firstUniqueIndex = table.GetFirstKey(); // cannot be null here 
                    var keyCount = firstUniqueIndex?.Columns.Length ?? 0;
                    for (var i = 0; i < keyCount; ++i, ++variableIndex)
                    {
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
                        var field = MetaExtensions.GetEmptyField(new Meta(firstUniqueIndex?.Columns[i]
                                        ?? string.Empty), FieldType.Int);
#pragma warning restore S2589
                        result.Append(_ddlBuilder.GetPhysicalName(field));
                        result.Append(DmlEqual);
                        result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
                            variableIndex.ToString(CultureInfo.InvariantCulture));
                        // last element?
                        if (i < keyCount - 1) result.Append(DmlAnd);
                    }
                }
                break;
            default: // mtm not supported !!!
                throw new NotImplementedException();
        }
        return result.ToString();
    }

    #endregion 

}
