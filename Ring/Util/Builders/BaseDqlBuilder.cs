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

    public string SelectFrom(Table table)
    {
        var index = _tableIndex.GetIndex(table.Name);
        var result = _tableSelect[index];
        if (result==null)
        {
            result = BuildSelect(table);
            _tableSelect[index] = result;
        }
        return result;
    }

    #region private methods 

    private string BuildSelect(Table table)
    {
        var result = new StringBuilder();
        var mapperCount = table.ColumnMapper.Length;
        var fieldCount = table.Fields.Length;
        var i=0;
        int index;
        result.Append(DqlSelect);
        while (i<mapperCount)
        {
            index = table.ColumnMapper[i];
            ++i; // just before continue
            if (index >= fieldCount) result.Append(_ddlBuilder.GetPhysicalName(table.Relations[index - fieldCount]));
            else result.Append(_ddlBuilder.GetPhysicalName(table.Fields[index]));
            result.Append(ColumnDelimiter);
        }
        --result.Length;
        result.Append(DqlFrom);
        result.Append(table.PhysicalName);
        return result.ToString();
    }

    #endregion 

}
