using Ring.Schema.Enums;
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

    #region private methods 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddBaseEntity(ref HashCode hashCode, BaseEntity baseEntity)
	{
		// Code size: 79 (0x4f) - no virtual call
		hashCode.Add(baseEntity.Id);
		hashCode.Add(baseEntity.Name, StringComparer.Ordinal);
		if (baseEntity.Description != null) hashCode.Add(baseEntity.Description, StringComparer.Ordinal);
		hashCode.Add(baseEntity.Baseline);
		hashCode.Add(baseEntity.Active);
	}

	#endregion 
}
