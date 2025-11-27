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
	protected int RelationCount;
	protected int IndexCount;
	protected int ErrorCount;
	protected int TableSpaceCount;
	protected int LineCount;

	// template 
	protected readonly static SchemaTemplate DefaultTemplate = new(string.Empty, DocumentType.Undefined, [], 0);
	protected readonly Dictionary<string, SchemaTemplateItem> TagDictionary;
	protected readonly SchemaTemplate Template;
	protected readonly DocumentType DocumentType;
	protected int LoadTemplateErrorCount;

	// attributes
	private readonly SchemaTemplateAttribute _tableIdAttribute;
	private readonly SchemaTemplateAttribute _tableNameAttribute;
	private readonly SchemaTemplateAttribute _fieldTypeAttribute;
	private readonly SchemaTemplateAttribute _fieldCaseSensitiveAttribute;

	internal BaseDocumentValidator(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, DocumentType documentType)
	{
		Template = template;
		TagDictionary = tagDico;
		DocumentType = documentType;
		LoadTemplateErrorCount = 0;

		// load attributes 
		_tableIdAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.Id, ref LoadTemplateErrorCount);
		_tableNameAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount);
		_fieldTypeAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.Type, ref LoadTemplateErrorCount); // field 
		_fieldCaseSensitiveAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.CaseSensitive, ref LoadTemplateErrorCount);
	}

	protected void ResetStats()
	{
		SchemaCount = 0;
		TableCount = 0;
		FieldCount = 0;
		RelationCount = 0;
		IndexCount = 0;
		ErrorCount = 0;
		TableSpaceCount = 0;
		LineCount = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected (FieldType, SearchableType) GetFieldInfo(XmlReader reader)
	{
		// Code size: 126 (0x7e)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var fieldType = FieldType.Undefined;
		var searchableType = SearchableType.None;
		var attributeTypeName = _fieldTypeAttribute.Name;
		var attributeSearchableName = _fieldCaseSensitiveAttribute.Name;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeTypeName, attributeName, comparison)) fieldType = _fieldTypeAttribute.GetFieldType(reader.Value);
				if (string.Equals(attributeSearchableName, attributeName, comparison)) searchableType = _fieldCaseSensitiveAttribute.GetSearchableType(reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (fieldType, searchableType);
	}

	protected (int, string) GetTableInfo(XmlReader reader)
	{
		// Code size: 127 (0x7f)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var id = 0;
		var name = string.Empty;
		var attributeIdName = _tableIdAttribute.Name;
		var attributeNameName = _tableNameAttribute.Name;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty;
				if (string.Equals(attributeIdName, attributeName, comparison)) id = _tableIdAttribute.GetInteger(attributeValue);
				if (string.Equals(attributeNameName, attributeName, comparison)) name = attributeValue;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (id, name);
	}

	protected static SchemaTemplate GetTemplate(DocumentType documentType) => documentType.GetSchemaTemplate() ?? DefaultTemplate;

}
