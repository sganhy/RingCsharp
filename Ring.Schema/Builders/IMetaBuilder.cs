namespace Ring.Schema.Builders;

internal interface IMetaBuilder
{
	ValueTask<Meta[]> GetMeta(string FilePath, int count, CancellationToken cancellationToken = default);
}
