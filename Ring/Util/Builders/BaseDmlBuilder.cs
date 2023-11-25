using Ring.Schema.Enums;
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
    protected static readonly string DmlInsert = @"INSERT INTO ";
    protected static readonly string DmlValues = @") VALUES (";
    protected static readonly string DmlUpdate = @"UPDATE ";
    protected static readonly string DmlSet = @" SET ";
    protected static readonly string DmlDelete = @"DELETE FROM ";
    protected static readonly string DmlWhere = @" WHERE ";
    protected static readonly string DmlAnd = " AND ";
    protected static readonly char DmlEqual = '=';

    protected string[] _tableIndex;
    protected string?[] _tableDelete;
    protected string?[] _tableInsert;
    protected string?[] _tableInsertWithRel;
    protected readonly IDdlBuilder _ddlBuilder;
    protected readonly Field _defaultField;

    internal BaseDmlBuilder()
    {
        _tableIndex = Array.Empty<string>();
        _tableDelete = Array.Empty<string?>();
        _tableInsert = Array.Empty<string?>();
        _tableInsertWithRel = Array.Empty<string?>();
        _ddlBuilder = Provider.GetDdlBuilder();
        _defaultField = MetaExtensions.GetEmptyField(new Meta(string.Empty), FieldType.Int);
    }

    public abstract DatabaseProvider Provider { get; }
    public abstract string VariableNameTemplate { get; }

    public void Init(DbSchema schema)
    {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        var mtmTaleDico = new HashSet<string>();
        var mtmIndex = 0;
        _tableIndex = new string[mtmCount + tableCount];
        _tableDelete = new string?[mtmCount + tableCount];
        _tableInsert = new string?[mtmCount + tableCount];
        _tableInsertWithRel = new string?[mtmCount + tableCount];
        for (var i = 0; i < tableCount; ++i) _tableIndex[i] = schema.TablesById[i].Name;
        for (var i = 0; i < schema.TablesById.Length; ++i)
            for (var j = schema.TablesById[i].Relations.Length - 1; j >= 0; --j)
            {
                var relation = schema.TablesById[i].Relations[j];
                if (relation.Type == RelationType.Mtm && !mtmTaleDico.Contains(relation.ToTable.Name))
                {
                    _tableIndex[mtmIndex + tableCount] = relation.ToTable.Name;
                    mtmTaleDico.Add(relation.ToTable.Name);
                    ++mtmIndex;
                }
            }
        Array.Sort(_tableIndex);
    }

    public string Insert(Table table, bool includeRelations) {
        // avoid lock
        var index = _tableIndex.GetIndex(table.Name);
        var result = includeRelations ? _tableInsertWithRel[index] : _tableInsert[index];
        if (result==null)
        {
            result = BuildInsert(table, includeRelations);
            if (includeRelations) _tableInsertWithRel[index] = result;
            else _tableInsert[index] = result;
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

    private string BuildInsert(Table table, bool includeRelations)
    {
        var result = new StringBuilder();
        var columnCount = 0;
        result.Append(DmlInsert);
        result.Append(table.PhysicalName);
        result.Append(SqlSpace);
        result.Append(StartParenthesis);
        for (var i = 0; i < table.FieldsById.Length; ++i)
        {
            result.Append(_ddlBuilder.GetPhysicalName(table.FieldsById[i]));
            result.Append(ColumnDelimiter);
            ++columnCount;
        }
        if (table.FieldsById.Length > 0) --result.Length;
        if (includeRelations)
        {
            for (var i = 0; i < table.Relations.Length; ++i)
            {
                var relation = table.Relations[i];
                if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
                {
                    if (columnCount > 0 && !Equals(ColumnDelimiter, result[^1])) result.Append(ColumnDelimiter);
                    result.Append(_ddlBuilder.GetPhysicalName(relation));
                    result.Append(ColumnDelimiter);
                    ++columnCount;
                }
            }
            if (table.Relations.Length > 0) --result.Length;
        }
        result.Append(DmlValues);
        for (var i = 1; i <= columnCount; ++i)
        {
            result.Append(string.Format(CultureInfo.InvariantCulture, VariableNameTemplate, i));
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
                result.Append(string.Format(CultureInfo.InvariantCulture, VariableNameTemplate, 1));
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
                        var field = MetaExtensions.GetEmptyField(new Meta(firstUniqueIndex?.Columns[i]
                                        ?? string.Empty), FieldType.Int);
                        result.Append(_ddlBuilder.GetPhysicalName(field));
                        result.Append(DmlEqual);
                        result.Append(string.Format(CultureInfo.InvariantCulture, VariableNameTemplate, variableIndex));
                        // last element?
                        if (i< keyCount-1) result.Append(DmlAnd);
                    }
                }
                break;
            default:
                // throw exception
                break;

        }
        return result.ToString();
    }

    #endregion 

}
