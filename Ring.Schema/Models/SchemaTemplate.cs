using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class SchemaTemplate
{
	internal readonly string ResourceFile;
	internal readonly DocumentType Type;
	internal readonly SchemaTemplateItem[] Items; // sorted by EntityTypeId
	internal readonly int MaxDepth;

	internal SchemaTemplate(string resourceFile, DocumentType type, SchemaTemplateItem[] xmlTemplateItems, int maxDepth)
	{
		ResourceFile = resourceFile;
		Type = type;
		Items = xmlTemplateItems;
		MaxDepth = maxDepth;
	}

}
