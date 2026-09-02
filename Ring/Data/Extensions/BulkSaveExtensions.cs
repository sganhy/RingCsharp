using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class BulkSaveExtensions
{
	internal static int Hash(this BulkSave bulkSave)
	{
		var hash = new HashCode();
		foreach (var query in bulkSave.Queries.AsReadOnlySpan()) hash.AddSaveQuery(query);
		return hash.ToHashCode();
	}
}
