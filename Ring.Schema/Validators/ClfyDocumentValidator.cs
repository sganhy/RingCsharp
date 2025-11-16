using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Validators;

internal class ClfyDocumentValidator : BaseDocumentValidator, IDocumentValidator
{
	private Dictionary<string, int> _tableDictionary = [];

	internal ClfyDocumentValidator() : this(GetTemplate(DocumentType.XmlClfy)) { }
	internal ClfyDocumentValidator(DocumentType documentType) : this(GetTemplate(documentType)) { }
	private ClfyDocumentValidator(SchemaTemplate template) : base(template, template.ToTagDictionary(StringComparer.Ordinal), template.Type) { }

    public Dictionary<string, int> ReferenceTables => _tableDictionary;

    public ValueTask<DocumentStats> GetMetaCountAsync(string filePath, CancellationToken cancellationToken = default)
	{
		var validator = new NativeDocumentValidator(DocumentType.XmlClfy);
		return validator.GetMetaCountAsync(filePath, cancellationToken);
	}
}
