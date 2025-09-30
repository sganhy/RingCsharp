using Ring.Schema.Models;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	internal static bool BaseEntityEquals(this BaseEntity value, BaseEntity? other) 
	{
		// Code size: 88 (0x58)
		if (other==null) return false;
		return value.Id == other.Id && string.Equals(value.Name, other.Name, StringComparison.Ordinal) &&
            string.Equals(value.Description, other.Description, StringComparison.Ordinal) && value.Baseline == other.Baseline && value.Active == other.Active;
	}

	internal static HashCode GetHashCodeInstance(this BaseEntity value)
	{
        // Code size: 93 (0x5d)
        var hash = new HashCode();
		hash.Add(value.Id);
		hash.Add(value.Name, StringComparer.Ordinal);
        if (value.Description!=null) hash.Add(value.Description, StringComparer.Ordinal);
        hash.Add(value.Baseline);
		hash.Add(value.Active);
		return hash;
	}

}
