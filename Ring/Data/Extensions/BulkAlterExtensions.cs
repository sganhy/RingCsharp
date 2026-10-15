using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class BulkAlterExtensions
{
	internal static int Hash(this BulkAlter bulkAlter)
	{
		// Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddBulkAlter(bulkAlter);
		return hash.ToHashCode();
	}
}
