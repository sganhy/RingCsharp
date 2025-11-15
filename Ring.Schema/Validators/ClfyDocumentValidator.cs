using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Validators;

internal class ClfyDocumentValidator : BaseDocumentValidator, IDocumentValidator
{

	internal ClfyDocumentValidator() : 
		base(GetTemplate(DocumentType.XmlClfy), GetTemplate(DocumentType.XmlClfy).ToTagDictionary(StringComparer.Ordinal), DocumentType.XmlClfy)
	{
	}

	public ValueTask<DocumentStats> GetMetaCountAsync(string filePath, CancellationToken cancellationToken = default)
	{
		return GetMetaCountAsync(filePath, TagDictionary, true, Template, cancellationToken);
	}
	public ValueTask<DocumentStats> GetMetaCountAsync(string filePath, Dictionary<string, SchemaTemplateItem> tagDico, bool hasTimeZoneOffsetColumn, SchemaTemplate template, CancellationToken cancellationToken = default)
	{
        throw new NotImplementedException();
    }
}
