using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDmlBuilder : BaseSqlBuilder, IDmlBuilder
{
    // commands
    protected static readonly string DmlInsert = @"INSERT INTO ";
    protected static readonly string DmlValues = @") VALUES (";

    protected string[] _tableIndex;
    protected string?[] _tableInsert;
    protected string?[] _tableInsertWithRel;
    protected readonly IDdlBuilder _ddlBuilder;

    internal BaseDmlBuilder()
    {
        _tableIndex = Array.Empty<string>();
        _tableInsert = Array.Empty<string?>();
        _tableInsertWithRel = Array.Empty<string?>();
        _ddlBuilder = Provider.GetDdlBuilder();
    }

    public abstract DatabaseProvider Provider { get; }
    public abstract string VariableNamePrefix { get; }

    public void Init(DbSchema schema)
    {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        var mtmTaleDico = new HashSet<string>();
        var mtmIndex = 0;
        _tableIndex = new string[mtmCount + tableCount];
        _tableInsert = new string?[mtmCount + tableCount];
        _tableInsertWithRel = new string?[mtmCount + tableCount];
        for (var i=0; i < tableCount; ++i) _tableIndex[i] = schema.TablesById[i].Name;
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
        var index = _tableIndex.GetIndex(table.Name);
        var result = includeRelations ? _tableInsertWithRel[index] : _tableInsert[index];
        if (result==null)
        {
            result=BuildInsert(table, includeRelations);
            if (includeRelations) _tableInsertWithRel[index] = result;
            else _tableInsert[index] = result;
        }
        return result;
    }

    private string BuildInsert(Table table, bool includeRelations)
    {
        var result = new StringBuilder();
        var columnCount = 0;
        result.Append(DmlInsert);
        result.Append(table.PhysicalName);
        result.Append(SqlSpace);
        result.Append(StartParenthesis);
        for (var i=0; i<table.FieldsById.Length; ++i)
        {
            result.Append(_ddlBuilder.GetPhysicalName(table.FieldsById[i]));
            result.Append(ColumnDelimiter);
            ++columnCount;
        }
        if (table.FieldsById.Length>0) --result.Length;
        if (includeRelations)
        {
            for (var i=0; i<table.Relations.Length; ++i)
            {
                var relation=table.Relations[i];
                if (relation.Type==RelationType.Mto || relation.Type==RelationType.Otop)
                {
                    if (columnCount>0 && !Equals(ColumnDelimiter, result[^1])) result.Append(ColumnDelimiter);
                    result.Append(_ddlBuilder.GetPhysicalName(relation));
                    result.Append(ColumnDelimiter);
                    ++columnCount;
                }
            }
            if (table.Relations.Length>0) --result.Length;
        }
        result.Append(DmlValues);
        for (var i=1; i<=columnCount; ++i)
        {
            result.Append(VariableNamePrefix);
            result.Append(i);
            result.Append(ColumnDelimiter);
        }
        if (columnCount>0) --result.Length;
        result.Append(EndParenthesis);
        return result.ToString();
    }

}
