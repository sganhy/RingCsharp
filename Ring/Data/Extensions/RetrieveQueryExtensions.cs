using Ring.Data.Models;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class RetrieveQueryExtensions
{
	internal static int Hash(this RetrieveQuery retrieveQuery)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddRetrieveQuery(retrieveQuery);
		return hash.ToHashCode();
	}


	/// <summary>
	/// Determines if two AlterQuery instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this RetrieveQuery retrieveQuery, RetrieveQuery? other)
	{
		if (!other.HasValue) return false;
		var otherValue = other.Value;
		//TODO => Compare the properties of retrieveQuery and otherValue to determine equivalence
		return true;
	}
}
