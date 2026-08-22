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
		if (!ReferenceEquals(constraint.ToTable, other?.ToTable)) return false;
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
}
