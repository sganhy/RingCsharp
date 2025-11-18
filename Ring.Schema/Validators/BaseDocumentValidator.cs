using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Xml;

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static (FieldType, SearchableType) GetFieldInfo(XmlReader reader, SchemaTemplateAttribute attributeType, SchemaTemplateAttribute attributeSearchable)
	{
		// Code size: 106 (0x6a)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var fieldType = FieldType.Undefined;
		var searchableType = SearchableType.None;
		var attributeTypeName = attributeType.Name;
		var attributeSearchableName = attributeSearchable.Name;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeTypeName, attributeName, comparison)) fieldType = attributeType.GetFieldType(reader.Value);
				if (string.Equals(attributeSearchableName, attributeName, comparison)) searchableType = attributeSearchable.GetSearchableType(reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (fieldType, searchableType);
	}

	protected static SchemaTemplate GetTemplate(DocumentType documentType) => documentType.GetSchemaTemplate() ?? DefaultTemplate;


}
