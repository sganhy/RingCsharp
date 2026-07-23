namespace Ring.Data.Extensions;

internal static class BulkAlterExtensions
{
	internal static int Hash(this BulkAlter bulkAlter)
	{
		var span = bulkAlter.Queries.AsReadOnlySpan();
		var hash = new HashCode();
		foreach (var query in span) hash.Add(query.GetHashCode());
		return hash.ToHashCode();
	}

}
