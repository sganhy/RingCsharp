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
