using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Validators;

internal abstract class BaseDocumentValidator
{
	// stats
	protected int SchemaCount;
	protected int TableCount;
	protected int FieldCount;
	protected int UndefinedFieldTypeCount;
	protected int RelationCount;
	protected int IndexCount;
	protected int WrongParentCount;
	protected int TableSpaceCount;
	protected int LineCount;

	// template 
	protected readonly static SchemaTemplate DefaultTemplate = new(string.Empty, DocumentType.Undefined, [], 0);
	protected readonly Dictionary<string, SchemaTemplateItem> TagDictionary;
	protected readonly SchemaTemplate Template;
	protected readonly DocumentType DocumentType;
	

	internal BaseDocumentValidator(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, DocumentType documentType)
	{
		Template = template;
		TagDictionary = tagDico;
		DocumentType = documentType;
	}

	protected void ResetStats()
	{
		SchemaCount = 0;
		TableCount = 0;
		FieldCount = 0;
		UndefinedFieldTypeCount = 0;
		RelationCount = 0;
		IndexCount = 0;
		WrongParentCount = 0;
		TableSpaceCount = 0;
		LineCount = 0;
	}

	protected static SchemaTemplate GetTemplate(DocumentType documentType) => documentType.GetSchemaTemplate() ?? DefaultTemplate;


}
