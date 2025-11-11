using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Validators;

internal class ClfyDocumentValidator : BaseDocumentValidator, IDocumentValidator
{

	internal ClfyDocumentValidator() : base(
		DocumentType.XmlClfy.GetSchemaTemplate() ?? DefaultTemplate,
		DocumentType.XmlClfy.GetSchemaTemplate().ToTagDictionary(StringComparer.Ordinal),
		DocumentType.XmlClfy)
	{
	}

	public ValueTask<DocumentStats> GetMetaCountAsync(string FilePath, CancellationToken cancellationToken = default)
	{
		return GetMetaCountAsync(FilePath, TagDictionary, true, Template, cancellationToken);
	}
	public ValueTask<DocumentStats> GetMetaCountAsync(string FilePath, Dictionary<string, SchemaTemplateItem> tagDico, bool hasTimeZoneOffsetColumn, SchemaTemplate template, CancellationToken cancellationToken = default)
	{
        throw new NotImplementedException();
    }
}
