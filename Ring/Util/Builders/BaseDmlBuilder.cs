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
	private static readonly string FirstParameter = @"1";

	private string?[] _tableDelete;
	private string?[] _tableInsert;
	private string?[] _tableUpdate;

	protected BaseDmlBuilder()
	{
		_tableDelete = Array.Empty<string?>();
		_tableInsert = Array.Empty<string?>();
		_tableUpdate = Array.Empty<string?>();
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
		// Code size: 258 (0x102)
		var columns = new StringBuilder();
		var values = new StringBuilder();
		var spanColumns = new ReadOnlySpan<Column>(table.Columns);
		var columnCount = table.Columns.Length;
		var variableId = 1;

		for (var i = 0; i<columnCount; ++i, ++variableId)
		{
			var column = spanColumns[i];
			columns.Append(column.PhysicalName);
			columns.Append(ColumnDelimiter);
			AppendVariable(values, VariableNameTemplate, variableId, true, column.FieldType);
		}
		if (variableId > 1)
		{
			--columns.Length;
			--values.Length;
		}
		return $"{DmlInsert}{table.PhysicalName} {StartParenthesis}{columns}{DmlValues}{values}{EndParenthesis}";
	}

	private string BuildDelete(Table table)
	{
		// Code size: 251 (0xfb)
		var result = new StringBuilder();
		result.Append(DmlDelete)
			.Append(table.PhysicalName)
			.Append(DmlWhere);
		if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
		{
			result.Append(table.Columns[0].PhysicalName);
			result.Append(DmlEqual);
			result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
		}
		else
		{
			var variableIndex = 1;
			var pk = table.GetPrimaryKey(); // cannot be null here 
			if (pk.Length<=0) throw new NotImplementedException();
			var keyCount = pk.Length;
			for (var i=0; i<keyCount; ++i, ++variableIndex)
			{
				result.Append(pk[i].PhysicalName);
				result.Append(DmlEqual);
				result.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
					variableIndex.ToString(CultureInfo.InvariantCulture));
				// last element?
				if (i< keyCount-1) result.Append(SqlAnd);
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
			result.Append(table.Columns[0].PhysicalName)
				.Append(DmlEqual)
				.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, FirstParameter);
		}
		else 
		{ 
			var variableIndex = 1;
			var pk = table.GetPrimaryKey(); // cannot be null here 
			var keyCount = pk.Length;
			for (var i = 0; i < keyCount; ++i, ++variableIndex)
			{
				var column = pk[i];
				result.Append(column.PhysicalName)
					.Append(DmlEqual)
					.AppendFormat(CultureInfo.InvariantCulture, VariableNameTemplate, 
					variableIndex.ToString(CultureInfo.InvariantCulture));
				// last element?
				if (i < keyCount - 1) result.Append(SqlAnd);
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
