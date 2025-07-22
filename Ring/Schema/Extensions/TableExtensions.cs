using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Runtime.CompilerServices;
using System.Text;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;
 
internal static class TableExtensions
{
	// Rider check 2025-07-23
	private const char HashCodeSeparator = (char)9999;
	private static readonly List<Column> EmptyColumnList = new(0);

	/// <summary>
	/// 	Get field by name, case-sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">field name</param>
	/// <returns>Field object</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Field? GetField(this Table table, string name)
	{
		// Code size: 92 (0x5c) - no callvirt
		var span = new ReadOnlySpan<Field>(table.Fields);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
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
		// Code size: 59 (0x3b) - no callvirt
		var span = new ReadOnlySpan<Field>(table.Fields);
		foreach (var field in span) if (string.Equals(name, field.Name, comparisonType)) return field;
		return null;
	}

	/// <summary>
	/// 	Get Fields by id ==> O(n) complexity
	/// </summary>
	internal static Field? GetField(this Table table, int id)
	{
		// Code size: 43 (0x2b)
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
	/// 	Get Fields by id ==> O(n) complexity
	/// </summary>
	internal static Field? GetField(this Table table, Column column) => GetField(table, column.Id);

	/// <summary>
	/// 	Get index field by name, case-sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">field name</param>
	/// <returns>Field index or -1 if not found</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetFieldIndex(this Table table, string name)
	{
		// Code size: 84 (0x54) - no callvirt
		var span = new ReadOnlySpan<Field>(table.Fields);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
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
		// Code size: 92 (0x5c)
		var span = new ReadOnlySpan<Relation>(table.Relations);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
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
	/// 	Get the relation object by id ==> O(n) complexity
	/// </summary>
	/// <returns>Relation object</returns>
	internal static Relation? GetRelation(this Table table, int id)
	{
		foreach (var relation in new ReadOnlySpan<Relation>(table.Relations))
			if (id == relation.Id) return relation;
		return null;
	}

	/// <summary>
	/// 	Get index relation by name, case-sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">relation name</param>
	/// <returns>Field index or -1 if not found</returns>
	internal static int GetRelationIndex(this Table table, string name)
	{
		// Code size: 84 (0x54)
		var span = new ReadOnlySpan<Relation>(table.Relations);

		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return indexerMiddle;
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return -1;
	}

	/// <summary>
	/// 	Get column from logical name - O(log N)
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">Logical column name</param>
	/// <returns>Column object</returns>
	internal static Column? GetColumn(this Table table, string name)
	{
		// Code size: 75 (0x4b)
		var field = table.GetField(name);
		var type = EntityType.Undefined;
		var id=-1;
		if (field != null)
		{
			id = field.Id;
			type = field.SearchableType == SearchableType.None ? EntityType.Field : EntityType.SearchableColumn;
		}
		else 
		{ 
			var relation = table.GetRelation(name);
			if (relation != null)
			{
				id = relation.Id;
				type = EntityType.Relation;
			}
		}
		return type != EntityType.Undefined ? GetColumn(table, id, type) : null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Column? GetColumn(this Table table, int id, EntityType type)
	{
		// Code size: 149 (0x95)
		var colWeight = Meta.ColumnTypeWeight(type);
		var span = new ReadOnlySpan<Column>(table.Columns); // sorted by Id
		int indexerLeft = 0, indexerRight = span.Length - 1;

		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = id - span[indexerMiddle].Id;
			if (indexerCompare == 0)
			{
				// sub search on Column.Type
				var weightCompare = colWeight - Meta.ColumnTypeWeight(span[indexerMiddle].Type);
				if (weightCompare == 0) return span[indexerMiddle];
				else if (weightCompare > 0) indexerLeft = indexerMiddle + 1;
				else indexerRight = indexerMiddle - 1;
			}
			else if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	internal static int GetColumnIndex(this Table table, int id, EntityType type)
	{
		// Code size: 141 (0x8d)
		var colWeight = Meta.ColumnTypeWeight(type);
		var span = new ReadOnlySpan<Column>(table.Columns); // sorted by Id
		int indexerLeft = 0, indexerRight = span.Length - 1;

		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = id - span[indexerMiddle].Id;
			if (indexerCompare == 0)
			{
				// sub search on Column.Type
				var weightCompare = colWeight - Meta.ColumnTypeWeight(span[indexerMiddle].Type);
				if (weightCompare == 0) return indexerMiddle;
				else if (weightCompare > 0) indexerLeft = indexerMiddle + 1;
				else indexerRight = indexerMiddle - 1;
			}
			else if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return -1;
	}

	/// <summary>
	/// 	Get an index object by name ==> O(log n) complexity
	/// </summary>
	/// <returns>Index object</returns>
	internal static Index? GetIndex(this Table table, string name)
	{
		// Code size: 92 (0x5c)
		var span = new ReadOnlySpan<Index>(table.Indexes);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	internal static List<Column> GetPrimaryKey(this Table table)
	{
		// Code size: 68 (0x44)
		if (table.Type == TableType.Business || table.Type == TableType.Lexicon)
		{
			return new List<Column>(1) { table.Columns[0] };
		}
		else
		{
			var index = table.GetFirstUniqueIndex();
			if (index != null) return new List<Column>(index.Columns);
		}
		return EmptyColumnList;
	}

	internal static bool HasPrimaryKey(this Table table) => GetPrimaryKey(table).Count > 0;

	internal static Meta[] ToMeta(this Table table, int schemaId) 
	{
		// Code size: 283 (0x11b)
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
		// first - define an object type
		result.Add(meta);
		return result.ToArray();
	}

	internal static long GetHashCode(this Table table)
	{
		// Code size: 15 (0xf)
		HashHelper.Djb2X(table.GetStringCode(), out long hash);
		return hash;
	}

	internal static string GetStringCode(this Table table)
	{
		// Code size: 237 (0xed) - checked: 2025-07-18
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
		return new StringBuilder()
			.Append(table.Cached)
			.Append(HashCodeSeparator)
			.Append(GetStringCode(table.Fields))
			.Append(HashCodeSeparator)
			.Append(GetStringCode(table.Relations))
			.Append(HashCodeSeparator)
			.Append(GetStringCode(table.Indexes))
			.Append(HashCodeSeparator)
			.Append(table.RecordSize)
			.Append(HashCodeSeparator)
		/* Columns[] Columns - removed from computing !! */
		/* table.PhysicalName - removed from computing !! */
			.Append((int)table.Type)
			.Append(HashCodeSeparator)
			.Append(table.SchemaId)
			.Append(HashCodeSeparator)
			.Append((int)table.PhysicalType) // avoid boxing here !! cast to int
			.Append(HashCodeSeparator)
			.Append(table.Subject)
			.Append(HashCodeSeparator)
			.Append(table.Readonly)
		/* + BaseEntity string code */
			.Append(BaseEntityExtensions.GetStringCode(table))
			.ToString();
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

	private static string GetStringCode(Field[] fields)
	{
		// Code size: 73 (0x49)
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
		// Code size: 73 (0x49)
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
		// Code size: 73 (0x49)
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
