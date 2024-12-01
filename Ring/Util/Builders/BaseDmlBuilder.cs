using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Globalization;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDmlBuilder : BaseSqlBuilder, IDmlBuilder
{
    // commands
    private const char DmlEqual = '=';
    private static readonly string DmlInsert = @"INSERT INTO ";
    private static readonly string DmlValues = @") VALUES (";
    private static readonly string DmlUpdate = @"UPDATE ";
    private static readonly string DmlSet = @" SET {0}";
    private static readonly string DmlDelete = @"DELETE FROM ";
    private static readonly string DmlWhere = @" WHERE ";
    private static readonly string DmlAnd = " AND ";
    private static readonly string FirstParameter = @"1";

    private string?[] _tableDelete;
    private string?[] _tableInsert;
    private string?[] _tableUpdate;
    private readonly IDdlBuilder _ddlBuilder;
    private readonly Field _defaultField;

    protected BaseDmlBuilder()
    {
        _tableDelete = Array.Empty<string?>();
        _tableInsert = Array.Empty<string?>();
        _tableUpdate = Array.Empty<string?>();
        _ddlBuilder = Provider.GetDdlBuilder();
        _defaultField = Meta.GetEmptyField(new Meta(string.Empty), FieldType.Int);
    }

    public abstract string VariableNameTemplate { get; }

    public void Init(DbSchema schema, string[] tableIndex)
    {
        _tableDelete = new string?[schema.ObjectCount];
        _tableInsert = new string?[schema.ObjectCount];
        _tableUpdate = new string?[schema.ObjectCount];
    }

    public string Insert(Table table) {
        // avoid lock
        var index = table.ObjectIndex;
        var result = _tableInsert[index];
        if (result==null)
        {
            result = BuildInsert(table);
            _tableInsert[index] = result;
        }
        return result;
    }

    public string Update(Table table) {
        var index = table.ObjectIndex;
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
        var index = table.ObjectIndex;
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
        if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
        {
            result.Append(_ddlBuilder.GetPhysicalName(table.Fields[table.RecordIndexes[0]] ?? _defaultField));
            result.Append(DmlEqual);
            result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
        }
        else
        {
            var variableIndex = 1;
            var pk = table.GetPrimaryKey(); // cannot be null here 
            if (pk.Count<=0) throw new NotImplementedException();
            var keyCount = pk.Count;
            for (var i=0; i<keyCount; ++i, ++variableIndex)
            {
                var column = Meta.GetEmptyField(new Meta(pk[i].Name),FieldType.Int);
                result.Append(_ddlBuilder.GetPhysicalName(column));
                result.Append(DmlEqual);
                result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
                    variableIndex.ToString(CultureInfo.InvariantCulture));
                // last element?
                if (i< keyCount-1) result.Append(DmlAnd);
            }
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

        if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
        {
            result.Append(_ddlBuilder.GetPhysicalName(table.Fields[table.RecordIndexes[0]]));
            result.Append(DmlEqual);
            result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
        }
        else 
        { 
            var variableIndex = 1;
            var pk = table.GetPrimaryKey(); // cannot be null here 
            var keyCount = pk.Count;
            for (var i = 0; i < keyCount; ++i, ++variableIndex)
            {
                var column = Meta.GetEmptyField(new Meta(pk[i].Name), FieldType.Int);
                result.Append(_ddlBuilder.GetPhysicalName(column));
                result.Append(DmlEqual);
                result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
                    variableIndex.ToString(CultureInfo.InvariantCulture));
                // last element?
                if (i < keyCount - 1) result.Append(DmlAnd);
            }
        }
        return result.ToString();
    }

    #endregion 

}
