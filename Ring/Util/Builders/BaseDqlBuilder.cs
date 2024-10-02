using Ring.Schema.Builders;
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
    private string[] _tableSelect;
    private string? _catalogTable;
    protected readonly IDdlBuilder _ddlBuilder;

    protected BaseDqlBuilder()
    {
        _tableSelect = Array.Empty<string>();
        _tableIndex = Array.Empty<string>();
        _ddlBuilder = Provider.GetDdlBuilder();
    }

    public void Init(DbSchema schema, string[] tableIndex)
    {
        _tableIndex = tableIndex;
        _tableSelect = GetTableSelect(schema);   // pre load selection for all tables
    }

    public string SelectFrom(Table table) => _tableSelect[_tableIndex.GetIndex(table.Name)];
    
    public string Exists(Table table)
    {
        if (_catalogTable==null)
        {
            var tableBuilder = new TableBuilder();
            table = tableBuilder.GetCatalog(EntityType.Table, Provider);
            var result= new StringBuilder(BuildSelect(table));
            //AppendFilter()
            _catalogTable = result.ToString();
        }
        return _catalogTable;
    }
    protected abstract string GetSelection(Field field);

    protected abstract string GetSelection(Relation relation);

    #region private methods 

    private string BuildSelect(Table table)
    {
        var result = new StringBuilder();
        var columnCount = table.Columns.Length;
        var i=0;
        result.Append(SqlSelect);
        // select clause 
        while (i<columnCount)
        {
            var column = table.Columns[i];
            ++i; // just before continue
            if (column.Type==EntityType.Relation) result.Append(GetSelection((Relation)column));
            else result.Append(GetSelection((Field)column));
            result.Append(ColumnDelimiter);
        }
        --result.Length;
        result.Append(SqlFrom);
        result.Append(table.PhysicalName);
        return result.ToString();
    }

    private string[] GetTableSelect(DbSchema schema)
    {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        var result = new string[mtmCount + tableCount];
        var tableSpan = new ReadOnlySpan<Table>(schema.TablesById);

        foreach (var table in tableSpan)
        {
            var index = _tableIndex.GetIndex(table.Name);
            result[index] = BuildSelect(table);
            for (var i=table.Relations.Length-1; i>=0; --i)
            {
                var relation = table.Relations[i];
                if (relation.Type==RelationType.Mtm)
                {
                    index = _tableIndex.GetIndex(relation.ToTable.Name);
                    result[index] = BuildSelect(relation.ToTable);
                }
            }
        }
        return result;
    }

    #endregion

}
