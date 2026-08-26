using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Runtime.CompilerServices;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;
 
internal static class TableExtensions
{
	// Rider check 2025-07-23
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
		// Code size: 90 (0x5a) - no virtual calls
		var span = new ReadOnlySpan<Field>(table.Fields);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
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
		// Code size: 59 (0x3b) - no virtual calls
		var span = new ReadOnlySpan<Field>(table.Fields);
		foreach (var field in span) if (string.Equals(name, field.Name, comparisonType)) return field;
		return null;
	}

	/// <summary>
	/// 	Get Fields by id ==> O(log n) complexity
	/// </summary>
	internal static Field? GetField(this Table table, int id)
	{
		// Code size: 28 (0x1c)
		var column = GetColumn(table, id, EntityType.Field);
		return column.HasValue ? table.Fields[column.Value.RecordIndex] : null;
	}

	/// <summary>
	/// 	Get index field by name, case-sensitive search ==> O(log n) complexity
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="name">field name</param>
	/// <returns>Field index or -1 if not found</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetFieldIndex(this Table table, string name)
	{
		// Code size: 82 (0x52) - no virtual calls
		var span = new ReadOnlySpan<Field>(table.Fields);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
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
		// Code size: 90 (0x5a) - no virtual calls
		var span = new ReadOnlySpan<Relation>(table.Relations);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
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
		// Code size: 49 (0x31)
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
		// Code size: 54 (0x36)
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
		// Code size: 82 (0x52)
		var span = new ReadOnlySpan<Relation>(table.Relations);

		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return indexerMiddle;
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return -1;
	}

	/// <summary>
	/// 	Get column bye logical name - O(log N)
	/// </summary>
	/// <param name="table">table object</param>
	/// <param name="logicalName">Logical column name</param>
	/// <returns>Column object</returns>
	internal static Column? GetColumn(this Table table, string logicalName)
	{
		// Code size: 75 (0x4b)
		var field = table.GetField(logicalName);
		var type = EntityType.Undefined;
		var id=-1;
		if (field is not null)
		{
			id = field.Id;
			type = field.SearchableType == SearchableType.None ? EntityType.Field : EntityType.SearchableColumn;
		}
		else 
		{ 
			var relation = table.GetRelation(logicalName);
			if (relation is not null)
			{
				id = relation.Id;
				type = EntityType.Relation;
			}
		}
		return type != EntityType.Undefined ? GetColumn(table, id, type) : null;
	}

	internal static string? GetDescription(this Table table,in Column column)
	{
		// Code size: 67 (0x43)
		if (column.Type == EntityType.Relation) return table.Relations[column.RecordIndex-table.Fields.Length].Description;
		else if (column.Type == EntityType.Field) return table.Fields[column.RecordIndex].Description;
		// no descriptions for searchable fields 
		return null;
	}

	internal static string? GetLogicalName(this Table table,in Column column)
	{
		// Code size: 67 (0x43)
		if (column.Type == EntityType.Relation) return table.Relations[column.RecordIndex - table.Fields.Length].Name;
		else if (column.Type == EntityType.Field || column.Type == EntityType.SearchableColumn) return table.Fields[column.RecordIndex].Name;
		return null;
	}

	/// <summary>
	/// 	Get column bye id - O(log N)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Column? GetColumn(this Table table, int id, EntityType type)
	{
		// Code size: 151 (0x97) - no virtual calls
		var colWeight = Meta.ColumnTypeWeight(type);
		var span = new ReadOnlySpan<Column>(table.Columns); // sorted by Id
		int indexerLeft = 0, indexerRight = span.Length - 1;

		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			ref readonly var candidate = ref span[indexerMiddle]; // one indexer call, aliased — no Column copy yet
			var indexerCompare = id - candidate.Id;
			if (indexerCompare == 0)
			{
				// sub search on Column.Type
				var weightCompare = colWeight - Meta.ColumnTypeWeight(candidate.Type);
				if (weightCompare == 0) return candidate; // the only copy: building the Column? to return
				if (weightCompare > 0) indexerLeft = indexerMiddle + 1;
				else indexerRight = indexerMiddle - 1;
			}
			else if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	internal static int GetColumnIndex(this Table table, int id, EntityType type)
	{
		// Code size: 135 (0x87)
		var colWeight = Meta.ColumnTypeWeight(type);
		var span = new ReadOnlySpan<Column>(table.Columns); // sorted by Id
		int indexerLeft = 0, indexerRight = span.Length - 1;

		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = id - span[indexerMiddle].Id;
			if (indexerCompare == 0)
			{
				// sub search on Column.Type
				var weightCompare = colWeight - Meta.ColumnTypeWeight(span[indexerMiddle].Type);
				if (weightCompare == 0) return indexerMiddle;
				if (weightCompare > 0) indexerLeft = indexerMiddle + 1;
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
		// Code size: 90 (0x5a)
		var span = new ReadOnlySpan<Index>(table.Indexes);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
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
			return new List<Column>(1) { table.Columns[0] };

		var index = table.GetFirstUniqueIndex();
		return index is not null ? new List<Column>(index.Columns) : EmptyColumnList;
	}

	internal static Meta[] ToMeta(this Table table, int schemaId) 
	{
		// Code size: 322 (0x142)
		var result = new List<Meta>(table.Fields.Length+table.Relations.Length+table.Indexes.Length+1);
		int i;
		for (i=0; i < table.Fields.Length; ++i) result.Add(table.Fields[i].ToMeta(table.Id));
		for (i=0; i < table.Relations.Length; ++i) result.Add(table.Relations[i].ToMeta(table.Id));
		for (i=0; i < table.Indexes.Length; ++i) result.Add(table.Indexes[i].ToMeta(table));
		var flags = 0L;

		// set Table Flags
		flags = Meta.SetTableCached(flags, table.Cached);
		flags = Meta.SetTableReadonly(flags, table.Readonly);
		flags = Meta.SetTableAllowAttributeExtension(flags, table.AllowAttributeExtension);
		flags = Meta.SetPhysicalDeletion(flags, table.AllowHardDeletion);
		flags = Meta.SetPreparedStatement(flags, table.UsePreparedStatement);
		// set BaseEntity Flags
		flags = Meta.SetEntityBaseline(flags, table.Baseline);

		var meta = new Meta(table.Id, (byte)EntityType.Table, schemaId, (int)table.Type, flags, table.Name, table.Description, null, table.Active);
		// first - define an object type
		result.Add(meta);
		return result.ToArray();
	}

	internal static int Hash(this Table table)
	{
		var hash = new HashCode();
		hash.AddTable(table);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Table instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this Table table, Table? other)
	{
		// Code size: 227 (0xe3)
		if (!table.BaseEntityEquals(other)) return false;
		// other cannot be null here 
		/*
		int ObjectIndex; ==> nok - not considered
		bool Cached; 
		Field[] Fields;
		Relation[] Relations;
		Index[] Indexes;
		int RecordSize;
		Column[] Columns; ==> nok - not considered
		PhysicalType PhysicalType;
		int SchemaId;
		string? Subject;
		TableType Type;
		CacheId CacheId;  ==> nok - not considered
		string PhysicalName;  ==> nok - not considered
		bool AllowHardDeletion;
		bool Readonly;
		bool UsePreparedStatement;
		bool AllowAttributeExtension;
		*/
		
		/*
		❌ What's intentionally excluded (as per your comments):

			ObjectIndex - instance - specific identifier
			Columns[] -derived from Fields and Relations
			CacheId - runtime cache identifier
			PhysicalName - physical / implementation detail, not logical definition
		*/

		// other cannot be null here !!
		return table.Cached == other!.Cached && table.RecordSize == other.RecordSize && table.PhysicalType == other.PhysicalType && table.SchemaId == other.SchemaId &&
			string.Equals(table.Subject, other.Subject, StringComparison.Ordinal) && table.Type == other.Type && table.AllowHardDeletion == other.AllowHardDeletion &&
			table.Readonly == other.Readonly && table.UsePreparedStatement == other.UsePreparedStatement && table.AllowAttributeExtension == other.AllowAttributeExtension &&
			table.Fields.ArraysEqual(other.Fields) && table.Relations.ArraysEqual(other.Relations) && table.Indexes.ArraysEqual(other.Indexes);
	}

	#region private methods 

	/// <summary>
	/// 	Get first unique index
	/// </summary>
	private static Index? GetFirstUniqueIndex(this Table table)
	{
		// Code size: 52 (0x34)
		var span = new ReadOnlySpan<Index>(table.Indexes);
		foreach (var index in span) if (index.Unique) return index;
		return null;
	}

	#endregion

}
