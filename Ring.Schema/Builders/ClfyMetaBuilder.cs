
using Ring.Schema.Extensions;

namespace Ring.Schema.Builders;

internal sealed class ClfyMetaBuilder : BaseMetaBuilder, IMetaBuilder
{
	internal ClfyMetaBuilder() : base(
	DocumentType.XmlNative.GetSchemaTemplate() ?? DefaultTemplate,
	DocumentType.XmlNative.GetSchemaTemplate().ToTagDictionary(StringComparer.Ordinal),
	DocumentType.XmlClfy)
	{
	}

	public ValueTask<Meta[]> GetMeta(string FilePath, int count, CancellationToken cancellationToken = default)
	{
        throw new NotImplementedException();
    }
}
