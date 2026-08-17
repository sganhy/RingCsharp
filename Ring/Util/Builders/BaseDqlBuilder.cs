using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDqlBuilder : BaseSqlBuilder, IDqlBuilder
{
	private string[] _tableSelect; // include relations: yes , include searchable: no
	protected readonly IDdlBuilder _ddlBuilder;

	protected BaseDqlBuilder()
	{
		_tableSelect = Array.Empty<string>();
		_ddlBuilder = Provider.GetDdlBuilder();
	}

	public void Init(DbSchema schema)
	{
		_tableSelect = GetTableSelect(schema);   // pre load selection for all tables
	}
	public string SelectFrom(Table table) => _tableSelect[table.ObjectIndex]; // Code size: 14 (0xe)
	protected abstract string GetSelection(in Column column);

	#region private methods 

	private string BuildSelect(Table table, bool includeRelations)
	{
		// Code size: 157 (0x9d) - drivers are built 
		var result = new StringBuilder();
		var columns = table.Columns.AsSpan();

		//var includeSearchables = false; // exclude searchable columns for now, as they are not needed
		result.Append(SqlSelect);

		// select clause 
		foreach (var column in columns) 
		{
			//before: (column.Type == EntityType.SearchableColumn && includeSearchables)
			if (column.Type == EntityType.SearchableColumn) continue;
			if (column.Type == EntityType.Relation && !includeRelations) continue;
			result.Append(GetSelection(column)).Append(ColumnDelimiter);
		}
		--result.Length;
		result.Append(SqlFrom).Append(table.PhysicalName);
		return result.ToString();
	}

	private string[] GetTableSelect(DbSchema schema)
	{
		// Code size: 152 (0x98)
		var result = new string[schema.ObjectCount];
		var tableSpan = new ReadOnlySpan<Table>(schema.TablesById);

		foreach (var table in tableSpan)
		{
			var index = table.ObjectIndex;
			result[index] = BuildSelect(table, true);
			for (var i=table.Relations.Length-1; i>=0; --i)
			{
				var relation = table.Relations[i];
				if (relation.Type==RelationType.Mtm)
				{
					index = relation.ToTable.ObjectIndex;
					result[index] = BuildSelect(relation.ToTable, true); // reserved for future use, but not used for now
				}
			}
		}
		return result;
	}

	#endregion

}
