using Ring.Schema.Models;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	internal static bool BaseEntityEquals(this BaseEntity value, BaseEntity? other) 
	{
		// Code size: 88 (0x58) - no virtual calls
		return other is not null && value.Id == other.Id && string.Equals(value.Name, other.Name, StringComparison.Ordinal) &&
			string.Equals(value.Description, other.Description, StringComparison.Ordinal) && value.Baseline == other.Baseline && value.Active == other.Active;
	}
}
