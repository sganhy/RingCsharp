using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Data.Common;
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
	protected abstract string WrapVariable(string variable, FieldType fieldType);

	public void Init(DbSchema schema)
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
        // Code size: 430 (0x1ae)
        var result = new StringBuilder();
		var resultValues = new StringBuilder();
		var spanColumns = new ReadOnlySpan<IColumn>(table.Columns);
		var columnCount = table.Columns.Length;
		var variableId = 1;

		result.Append(DmlInsert)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append(StartParenthesis);
		
		for (var i = 0; i<columnCount; ++i, ++variableId)
		{
			var column = spanColumns[i];
			if (column.Type == EntityType.Relation) result.Append(column.PhysicalName);
			else
			{
				var field = (Field)column;
				result.Append(field.PhysicalName);
				#region  add searchable field 
				if (field.IsSearchable())
				{
					result.Append(ColumnDelimiter);
					result.Append(_ddlBuilder.GetSecondColumn(field));
					AppendVariable(resultValues, VariableNameTemplate, variableId++, false, column.FieldType);
				}
                #endregion
                #region time zone extra field?
                if (field.Type == FieldType.LongDateTime)
                {
                    var timeZoneField = _ddlBuilder.GetSecondColumn(field);
                    if (!string.IsNullOrEmpty(timeZoneField))
                    {
                        result.Append(ColumnDelimiter);
                        result.Append(timeZoneField);
                        resultValues.Append(string.Format(CultureInfo.InvariantCulture, VariableNameTemplate,
                        (variableId).ToString(CultureInfo.InvariantCulture)));
                        ++variableId;
                    }
                }
                #endregion
            }
            AppendVariable(resultValues, VariableNameTemplate, variableId, true, column.FieldType);
			result.Append(ColumnDelimiter);
		}
		if (columnCount>0)
		{
			--result.Length;
			--resultValues.Length;
		} 
		result.Append(DmlValues)
			.Append(resultValues)
			.Append(EndParenthesis);
		return result.ToString();
	}

	private string BuildDelete(Table table)
	{
		var result = new StringBuilder();
		result.Append(DmlDelete)
			.Append(table.PhysicalName)
			.Append(DmlWhere);
		if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
		{
			result.Append(table.Fields[table.RecordIndexes[0]].PhysicalName);
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
				result.Append(column.PhysicalName);
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
		// Code size: 279 (0x117)
		var result = new StringBuilder();
		result.Append(DmlUpdate)
			.Append(table.PhysicalName)
			.Append(DmlSet)
			.Append(DmlWhere);

		if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
		{
			result.Append(table.Fields[table.RecordIndexes[0]].PhysicalName)
				.Append(DmlEqual)
				.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
		}
		else 
		{ 
			var variableIndex = 1;
			var pk = table.GetPrimaryKey(); // cannot be null here 
			var keyCount = pk.Count;
			for (var i = 0; i < keyCount; ++i, ++variableIndex)
			{
				var column = Meta.GetEmptyField(new Meta(pk[i].Name), FieldType.Int);
				result.Append(column.PhysicalName)
					.Append(DmlEqual)
					.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
					variableIndex.ToString(CultureInfo.InvariantCulture));
				// last element?
				if (i < keyCount - 1) result.Append(DmlAnd);
			}
		}
		return result.ToString();
	}

	private void AppendVariable(StringBuilder subResult, string variableNameTemplate, int id, bool callWrap, FieldType fieldType)
	{
		// Code size: 65 (0x41)
		var variable = string.Format(CultureInfo.InvariantCulture, variableNameTemplate, (id).ToString(CultureInfo.InvariantCulture));
		if (callWrap) subResult.Append(WrapVariable(variable, fieldType));
		else subResult.Append(variable);
		subResult.Append(ColumnDelimiter);
	}
	
	#endregion 

}
