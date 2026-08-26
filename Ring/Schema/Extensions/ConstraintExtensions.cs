using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class ConstraintExtensions
{
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

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

	internal static Meta ToMeta(this Constraint constraint, Table table)
	{
		// Code size: 75 (0x4b)
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, constraint.Baseline);
		string? value = Meta.GetColumnList(GetLogicalNames(constraint, table));
		return new(constraint.Id, (byte)EntityType.Constraint, table.Id, (int)constraint.Type, flags, constraint.Name, constraint.Description, value, constraint.Active);
	}

	#region private methods 

	private static string[] GetLogicalNames(this Constraint constaint, Table table)
	{
		// Code size: 198 (0xc6)
		if (constaint.Columns.Length <= 0) return Array.Empty<string>();
		var result = new string[constaint.Columns.Length + 2];
		var resultIndex = 2;
		result[0] = constaint.MinValue.HasValue ? constaint.MinValue.Value.ToString(DefaultCulture) : string.Empty;
		result[1] = constaint.MaxValue.HasValue ? constaint.MaxValue.Value.ToString(DefaultCulture) : string.Empty;
		foreach (ref readonly var column in constaint.Columns.AsSpan()) result[resultIndex++] = table.GetLogicalName(column) ?? string.Empty;
		return result;
	}

	#endregion 
}
