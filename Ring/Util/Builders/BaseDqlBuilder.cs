using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDqlBuilder : BaseSqlBuilder, IDqlBuilder
{
    private string[] _tableIndex;
    private string?[] _tableSelect;
    private readonly IDdlBuilder _ddlBuilder;

    // clauses
    private static readonly string DqlSelect = @"SELECT ";
    private static readonly string DqlFrom = @" FROM ";

    internal BaseDqlBuilder() : base()
    {
        _tableSelect = Array.Empty<string>();
        _tableIndex = Array.Empty<string>();
        _ddlBuilder = Provider.GetDdlBuilder();
    }

    public void Init(DbSchema schema)
    {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        _tableSelect = new string?[mtmCount + tableCount];
        _tableIndex = GetTableIndex(schema);
    }

    public string Select(Table table, bool includeRelations)
    {
        var index = _tableIndex.GetIndex(table.Name);
        var result = _tableSelect[index];
        if (result==null)
        {
            result = BuildSelect(table, includeRelations);
            _tableSelect[index] = result;
        }
        return result;
    }

    #region private methods 

    private string BuildSelect(Table table, bool includeRelations)
    {
        var result = new StringBuilder();
        var columnCount = 0;
        var i=0;
        var itemCount = table.Fields.Length;
        result.Append(DqlSelect);
        while (i<itemCount)
        {
            result.Append(_ddlBuilder.GetPhysicalName(table.Fields[table.Mapper[i]]));
            result.Append(ColumnDelimiter);
            ++columnCount;
            ++i;
        }
        if (table.Fields.Length > 0) --result.Length;
        if (includeRelations)
        {
            var hasRelation = false;
            itemCount = table.Relations.Length;
            i=0;
            while (i<itemCount)
            {
                var relation = table.Relations[i];
                if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
                {
                    if (columnCount > 0 && !Equals(ColumnDelimiter, result[^1])) result.Append(ColumnDelimiter);
                    result.Append(_ddlBuilder.GetPhysicalName(relation));
                    result.Append(ColumnDelimiter);
                    ++columnCount;
                    hasRelation = true;
                }
                ++i;
            }
            if (hasRelation) --result.Length;
        }
        if (columnCount==0) --result.Length;
        result.Append(DqlFrom);
        result.Append(table.PhysicalName);
        return result.ToString();
    }

    #endregion 

}
