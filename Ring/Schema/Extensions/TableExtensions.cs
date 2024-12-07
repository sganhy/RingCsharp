using System.Runtime.CompilerServices;
using System.Text;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;

internal static class TableExtensions
{
	private const char HashCodeSeparator = (char)9999;

	/// <summary>
	/// 	Get field by name, case sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">field name</param>
	/// <returns>Field object</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Field? GetField(this Table table, string name)
	{
		var span = new ReadOnlySpan<Field>(table.Fields);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	/// <summary>
	/// 	Get field by name, case unsensitive search ==> O(n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">field name</param>
	/// <param name="comparisonType">StringComparison enum</param>
	/// <returns>Field object</returns>
	internal static Field? GetField(this Table table, string name, StringComparison comparisonType)
	{
		foreach (var field in new ReadOnlySpan<Field>(table.Fields)) 
			if (string.Equals(name, field.Name, comparisonType)) return field;
		return null;
	}

	/// <summary>
	/// 	Get Fields by id ==> O(n) complexity
	/// </summary>
	internal static Field? GetField(this Table table, int id)
	{
		var i = 0;
		var fieldCount = table.Fields.Length;
		while (i < fieldCount)
		{
			var field = table.Fields[i];
			if (field.Id == id) return field;
			++i;
		}
		return null;
	}

	/// <summary>
	/// 	Get index field by name, case sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">field name</param>
	/// <returns>Field index or -1 if not found</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetFieldIndex(this Table table, string name)
	{
		var span = new ReadOnlySpan<Field>(table.Fields);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return indexerMiddle;
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return -1;
	}

	/// <summary>
	/// 	Get relation object by name ==> O(log n) complexity
	/// </summary>
	/// <param name="table">Table object</param>
	/// <param name="name">Relation name</param>
	/// <returns>Relation object</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Relation? GetRelation(this Table table, string name)
	{
		var span = new ReadOnlySpan<Relation>(table.Relations);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	/// <summary>
	/// 	Get relation object by name ==> O(n) complexity
	/// </summary>
	/// <param name="table">Table object</param>
	/// <param name="name">Relation name</param>
	/// <param name="comparisonType">Comparison Type</param>
	/// <returns>Relation object</returns>
	internal static Relation? GetRelation(this Table table, string name, StringComparison comparisonType)
	{
		for (var i = table.Relations.Length - 1; i >= 0; --i)
		{
			var relation = table.Relations[i];
			if (string.Equals(name, relation.Name, comparisonType)) return relation;
		}
		return null;
	}

	/// <summary>
	/// 	Get relation object by id ==> O(n) complexity
	/// </summary>
	/// <returns>Relation object</returns>
	internal static Relation? GetRelation(this Table table, int id)
	{
		foreach (var relation in new ReadOnlySpan<Relation>(table.Relations))
			if (id == relation.Id) return relation;
		return null;
	}

	/// <summary>
	/// 	Get index relation by name, case sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">relation name</param>
	/// <returns>Field index or -1 if not found</returns>
	internal static int GetRelationIndex(this Table table, string name)
	{
		var span = new ReadOnlySpan<Relation>(table.Relations);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return indexerMiddle;
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return -1;
	}

	/// <summary>
	/// 	Get index object by name ==> O(log n) complexity
	/// </summary>
	/// <returns>Index object</returns>
	internal static Index? GetIndex(this Table table, string name)
	{
		var span = new ReadOnlySpan<Index>(table.Indexes);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	internal static List<IColumn> GetPrimaryKey(this Table table)
	{
		var result = new List<IColumn>();
		if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
		{
			result.Add(table.Fields[table.RecordIndexes[0]]);
		}
		else
		{
			var index = table.GetFirstUniqueIndex();
			if (index != null)
				foreach (string column in index.Columns) result.Add(GetColumn(table, column));
		}
		return result;
	}

	internal static bool HasPrimaryKey(this Table table) => GetPrimaryKey(table).Count > 0;

	internal static Meta[] ToMeta(this Table table, int schemaId) {
		var result = new List<Meta>(table.Fields.Length+table.Relations.Length+table.Indexes.Length+1);
		int i;
		for (i=0; i < table.Fields.Length; ++i) result.Add(table.Fields[i].ToMeta(table.Id));
		for (i=0; i < table.Relations.Length; ++i) result.Add(table.Relations[i].ToMeta(table.Id));
		for (i=0; i < table.Indexes.Length; ++i) result.Add(table.Indexes[i].ToMeta(table.Id));
		var flags = 0L;
		// set Table Flags
		flags = Meta.SetTableCached(flags, table.Cached);
		flags = Meta.SetTableReadonly(flags, table.Readonly);
		// set BaseEntity Flags
		flags = Meta.SetEntityBaseline(flags, table.Baseline);
		var meta = new Meta(table.Id, (byte)EntityType.Table, schemaId, (int)table.Type, flags, table.Name, table.Description, null, table.Active);
		// first - define Object type
		result.Add(meta);
		return result.ToArray();
	}

	/// <summary>
	/// 	Load Table.RecordIndexes[] & Table.Columns[]
	/// </summary>
	internal static void LoadColumnMapper(this Table table)
	{
		var fieldCount = table.Fields.Length;
		var relationCount = table.Relations.Length;
		var columnCount = fieldCount + relationCount; // potential max column count
		var index = 0;
		var colPosition = 0;
		var relationIndex = fieldCount;
		var i =0;
		// copy
		var columns = new IColumn[columnCount];
		for (; i < fieldCount; ++i) columns[i] = table.Fields[i];
		for (i=0; i < relationCount; ++i) columns[i+fieldCount] = table.Relations[i];
		// sort by Id
		Array.Sort(columns, (x, y) => x.Id.CompareTo(y.Id));
		i = 0;
		while (i<columnCount)
		{
			index = table.GetFieldIndex(columns[i].Name);
			if (index < 0)
			{
				var relation = columns[i];
				if (relation.RelationType == RelationType.Mto || relation.RelationType == RelationType.Otop)
				{
					table.RecordIndexes[colPosition] = relationIndex;
					table.Columns[colPosition] = columns[i];
					++colPosition;
					++relationIndex;
				}
			}
			else
			{
				table.Columns[colPosition] = columns[i];
				table.RecordIndexes[colPosition] = index;
				++colPosition;
			}
			++i;
		}
	}

	/// <summary>
	/// 	Compute index of relation(s) to Record._data[]; default value equal to -1
	/// </summary>
	internal static void LoadRelationRecordIndex(this Table table)
	{
		var fieldCount = table.Fields.Length;
		var relationCount = table.Relations.Length;
		var i = 0;
		var currentIndex = 0;
		while (i < relationCount)
		{
			var relation = table.Relations[i];
			if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
			{
				table.Relations[i] = relation.SetRecordIndex(currentIndex + fieldCount);
				++currentIndex;
			}
			else table.Relations[i] = relation.SetRecordIndex(-1);
			++i;
		}
	}

	internal static long GetHashCode(this Table table)
	{
		HashHelper.Djb2X(table.GetStringCode(), out long hash);
		return hash;
	}

	internal static string GetStringCode(this Table table)
	{
		/*
		* readonly bool Cached
		* readonly Field[] Fields
		* readonly Relation[] Relations
		* readonly Index[] Indexes
		* readonly int[] RecordIndexes
		* readonly int RecordSize
		* readonly IColumn[] Columns
		* readonly string PhysicalName
		* readonly PhysicalType PhysicalType
		* readonly int SchemaId
		* readonly string? Subject
		* readonly TableType Type
		* readonly CacheId CacheId
		* readonly bool Readonly
		*/
		var result = new StringBuilder();
		result.Append(table.Cached);
		result.Append(HashCodeSeparator);
		result.Append(GetStringCode(table.Fields));
		result.Append(HashCodeSeparator);
		result.Append(GetStringCode(table.Relations));
		result.Append(HashCodeSeparator);
		result.Append(GetStringCode(table.Indexes));
		result.Append(HashCodeSeparator);
		result.AppendJoin(HashCodeSeparator, table.RecordIndexes);		 // int[] RecordIndexes
		result.Append(HashCodeSeparator);
		result.Append(table.RecordSize);
		result.Append(HashCodeSeparator);
		// IColumn[] Columns - removed from computing !!
		result.Append(table.PhysicalName);
		result.Append(HashCodeSeparator);
		result.Append(table.Type.ToString());
		result.Append(HashCodeSeparator);
		result.Append(table.SchemaId);
		result.Append(HashCodeSeparator);
		result.Append(table.PhysicalType);
		result.Append(HashCodeSeparator);
		result.Append(table.Subject);
		result.Append(HashCodeSeparator);
		result.Append(table.Readonly);
		// BaseEntity
		result.Append(BaseEntityExtensions.GetStringCode(table));
		return result.ToString();
	}


	#region private methods 

	/// <summary>
	/// 	Get first unique index
	/// </summary>
	private static Index? GetFirstUniqueIndex(this Table table)
	{
		if (table.Indexes.Length > 0)
			for (var i = 0; i < table.Indexes.Length; ++i)
				if (table.Indexes[i].Unique) return table.Indexes[i];
		return null;
	}

	private static IColumn GetColumn(this Table table, string name)
	{
		var col = table.GetField(name??string.Empty);
		return col != null ? col : table.GetRelation(name??string.Empty);
	}

	private static string GetStringCode(Field[] fields)
	{
		var span = fields.AsSpan();
		var result = new StringBuilder();
		foreach (var field in span)
		{
			result.Append(field.GetStringCode());
			result.Append(HashCodeSeparator);
		}
		return result.ToString();
	}

	private static string GetStringCode(Relation[] relations)
	{
		var span = relations.AsSpan();
		var result = new StringBuilder();
		foreach (var relation in span)
		{
			result.Append(relation.GetStringCode());
			result.Append(HashCodeSeparator);
		}
		return result.ToString();
	}

	private static string GetStringCode(Index[] indexes)
	{
		var span = indexes.AsSpan();
		var result = new StringBuilder();
		foreach (var index in span)
		{
			result.Append(index.GetStringCode());
			result.Append(HashCodeSeparator);
		}
		return result.ToString();
	}

	#endregion

}
