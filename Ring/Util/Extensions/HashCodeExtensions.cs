using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using Index = Ring.Schema.Models.Index;

namespace Ring.Util.Extensions;

internal static class HashCodeExtensions
{
	internal static void AddField(this ref HashCode hashCode, Field field)
	{
		// Code size: 85 (0x55) 
		AddBaseEntity(ref hashCode, field);
		// Field-specific properties
		hashCode.Add((int)field.Type);
		hashCode.Add(field.Size);
		hashCode.Add(field.DefaultValue, StringComparer.Ordinal);
		hashCode.Add((int)field.SearchableType);
		hashCode.Add(field.AllowTruncation.ToInt());
		hashCode.Add(field.NotNull.ToInt());
		hashCode.Add(field.Multilingual.ToInt());
	}

	internal static void AddIndex(this ref HashCode hashCode, Index index)
	{
		// Code size: 59 (0x3b)
		AddBaseEntity(ref hashCode, index);
		// Index-specific properties
		hashCode.Add(index.Unique.ToInt());
		hashCode.Add(index.Bitmap.ToInt());
		hashCode.Add(index.ColumnList, StringComparer.Ordinal);
	}

	internal static void AddRelation(this ref HashCode hashCode, Relation relation)
	{
		// Code size: 170 (0xaa)
		AddBaseEntity(ref hashCode, relation);
		// Relation-specific properties
		hashCode.Add(relation.HasConstraint.ToInt());
		hashCode.Add(relation.NotNull.ToInt());
		hashCode.Add(relation.ToTable.Id); // pair of identification for a table
		hashCode.Add(relation.ToTable.SchemaId);
		hashCode.Add((int)relation.Type);
		hashCode.Add((int)relation.FieldType);
		// avoid recursion here calling AddRelation again
		if (!ReferenceEquals(relation.InverseRelation, relation))
		{
			hashCode.Add(relation.InverseRelation.Name);
			hashCode.Add(relation.InverseRelation.ToTable.Id); // pair of identification for a table
			hashCode.Add(relation.InverseRelation.ToTable.SchemaId);
		}			
	}

	internal static void AddColumn(this ref HashCode hashCode, Column column)
	{
		hashCode.Add(column.Id);
		hashCode.Add((int)column.Type);
		hashCode.Add((int)column.FieldType);
		hashCode.Add((int)column.SearchableType);
		hashCode.Add(column.PhysicalName);
		hashCode.Add(column.RecordIndex);
	}

	internal static void AddTable(this ref HashCode hashCode, Table table)
	{
		// Code size: 192 (0xc0)
		/* table definition: 
			internal readonly int ObjectIndex;
			internal readonly bool Cached;
			internal readonly Field[] Fields; // sorted by name.
			internal readonly Relation[] Relations; // sorted by name.
			internal readonly Index[] Indexes;
			internal readonly int RecordSize;
			internal readonly Column[] Columns;		 // mix Fields and Relations.
			internal readonly PhysicalType PhysicalType;
			internal readonly int SchemaId;
			internal readonly string? Subject;
			internal readonly TableType Type;
			internal readonly CacheId CacheId;
			internal readonly string PhysicalName;
			internal readonly bool AllowHardDeletion;
			internal readonly bool Readonly;
			internal readonly bool UsePreparedStatement;
			internal readonly bool AllowAttributeExtension; 
		*/
		AddBaseEntity(ref hashCode, table);
		// Table-specific properties
		//hashCode.Add(table.ObjectIndex); // position of table in the schema, not connected to the identity of table ==> nok
		hashCode.Add(table.Cached.ToInt()); // ok 
		AddFields(ref hashCode, table.Fields); // check fields => ok
		AddRelations(ref hashCode, table.Relations); // check relations => ok
		AddIndexes(ref hashCode, table.Indexes); // check indexes => ok
		//hashCode.Add(table.RecordSize); // computed value => nok
		//AddColumns(ref hashCode, table.Columns); // columns => nok
		hashCode.Add((int)table.PhysicalType); // ok 
		hashCode.Add(table.SchemaId); // ok 
		hashCode.Add(table.Subject); // ok 
		hashCode.Add((int)table.Type); // ok 
		// hashCode.Add((int)table.CacheId); // nok 
		// hashCode.Add((int)table.PhysicalName); // computed => nok
		hashCode.Add(table.AllowHardDeletion.ToInt()); // ok 
		hashCode.Add(table.Readonly.ToInt()); // ok 
		hashCode.Add(table.UsePreparedStatement.ToInt()); // ok 
		hashCode.Add(table.AllowAttributeExtension.ToInt()); // ok 
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddBaseEntity(ref HashCode hashCode, BaseEntity baseEntity)
	{
		// Code size: 79 (0x4f) - no virtual call
		hashCode.Add(baseEntity.Id);
		hashCode.Add(baseEntity.Name, StringComparer.Ordinal);
		if (baseEntity.Description is not null) hashCode.Add(baseEntity.Description, StringComparer.Ordinal);
		hashCode.Add(baseEntity.Baseline);
		hashCode.Add(baseEntity.Active);
	}

	private static void AddFields(ref HashCode hashCode, ReadOnlySpan<Field> fields)
	{
		// Code size: 38 (0x26)
		foreach (var field in fields) AddField(ref hashCode, field);
	}

	private static void AddRelations(ref HashCode hashCode, ReadOnlySpan<Relation> relations)
	{
		// Code size: 38 (0x26)
		foreach (var relation in relations) AddRelation(ref hashCode, relation);
	}

	private static void AddIndexes(ref HashCode hashCode, ReadOnlySpan<Index> indexes)
	{
		// Code size: 38 (0x26)
		foreach (var index in indexes) AddIndex(ref hashCode, index);
	}

	#endregion
}
