using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
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
        _tableIndex = new string[mtmCount + tableCount];
        _tableInsert = new string?[mtmCount + tableCount];
        _tableInsertWithRel = new string?[mtmCount + tableCount];
        for (var i=0; i < tableCount; ++i) _tableIndex[i] = schema.TablesById[i].Name;
    }

    public string Insert(Table table, bool includeRelations) {
        return BuildInsert(table, includeRelations);
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
        if (table.FieldsById.Length > 0) --result.Length;
        result.Append(DmlValues);
        if (includeRelations)
        {
            for (var i=0; i<table.Relations.Length; ++i)
            {
                result.Append(_ddlBuilder.GetPhysicalName(table.Relations[i]));
                result.Append(ColumnDelimiter);
                ++columnCount;
            }
            if (table.Relations.Length > 0) --result.Length;
        }
        for (var i=0; i<columnCount; ++i)
        {
            result.Append(VariableNamePrefix);
            result.Append(i);
            result.Append(ColumnDelimiter);
        }
        --result.Length;
        result.Append(EndParenthesis);
        return result.ToString();
    }

}
