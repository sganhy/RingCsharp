using Ring.Data.Models;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class SaveQueryExtensions
{
	internal static int GetHashCode(this SaveQuery saveQuery)
	{
		// Code size: 15 (0xf)
		var hash = 55;
		return hash;
	}

	internal static int Hash(this in SaveQuery saveQuery)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddSaveQuery(saveQuery);
		return hash.ToHashCode();
	}

	internal static bool IsEquivalentTo(this in SaveQuery alterQuery, SaveQuery? other)
	{
		return true;
	}

}
