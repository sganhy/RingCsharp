namespace Ring.Schema.Builders;

internal interface IMetaBuilder
{
	ValueTask<Meta[]> GetMetaAsync(string filePath, int count, Dictionary<string, int> referenceTables, CancellationToken cancellationToken = default);
}
