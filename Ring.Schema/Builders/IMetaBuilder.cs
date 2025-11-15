namespace Ring.Schema.Builders;

internal interface IMetaBuilder
{
	ValueTask<Meta[]> GetMetaAsync(string filePath, int count, CancellationToken cancellationToken = default);
}
