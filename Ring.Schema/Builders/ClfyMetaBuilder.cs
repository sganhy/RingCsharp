
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders;

internal sealed class ClfyMetaBuilder : BaseMetaBuilder, IMetaBuilder
{
	internal ClfyMetaBuilder() : this(GetTemplate(DocumentType.XmlClfy)) { }
	internal ClfyMetaBuilder(DocumentType documentType) : this(GetTemplate(documentType)) { } // reuse same logic with another document type
	private ClfyMetaBuilder(SchemaTemplate template) : base(template, template.ToTagDictionary(StringComparer.Ordinal), template.Type) { }

	public ValueTask<Meta[]> GetMetaAsync(string filePath, int count, Dictionary<string, int> referenceTable, CancellationToken cancellationToken = default)
	{
		var builder = new NativeMetaBuilder(DocumentType.XmlClfy);
		return builder.GetMetaAsync(filePath, count, referenceTable, cancellationToken);
	}
}
