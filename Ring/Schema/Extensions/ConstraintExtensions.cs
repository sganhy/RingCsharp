using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;

namespace Ring.Schema.Extensions;

internal static class ConstraintExtensions
{
	/// <summary>
	/// Determines if two Field instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this Constraint constraint, Constraint? other)
	{
		// Code size: 116 (0x74)
		if (!constraint.BaseEntityEquals(other)) return false;
		// other cannot be null here 
		return constraint.Type == other!.Type;
	}

	internal static int Hash(this Constraint constraint)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddConstraint(constraint);
		return hash.ToHashCode();
	}

	internal static Meta ToMeta(this Constraint constraint, int tableId, FieldType? newFieldType = null)
	{
		// Code size: 148 (0x94)
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, constraint.Baseline);
		return new(constraint.Id, (byte)EntityType.Constraint, tableId, (int)constraint.Type, flags, constraint.Name, constraint.Description, null, constraint.Active);
	}

}
