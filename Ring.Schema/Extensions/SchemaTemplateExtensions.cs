using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateExtensions
{
	private readonly static SchemaTemplateAttribute DefaultTemplateAttribute = new(string.Empty, SchemaTemplateAttributeType.Undefined, []);

	internal static Dictionary<string, SchemaTemplateItem> ToTagDictionary(this SchemaTemplate? template, StringComparer stringComparer)
	{
		// Code size: 108 (0x6c)
		if (template is null) return [];
		var dict = new Dictionary<string, SchemaTemplateItem>(template.Items.Length*2, stringComparer);
		var items = new ReadOnlySpan<SchemaTemplateItem>(template.Items);
		foreach (var item in items) if (!string.IsNullOrWhiteSpace(item.Tag) && !dict.ContainsKey(item.Tag))  dict.Add(item.Tag, item);
		return dict;
	}

	internal static SchemaTemplateItem? GetTemplateItem(this SchemaTemplate template, EntityType entityType)
	{
		// Code size: 102 (0x66)
		var entityTypeId = (int)entityType;
		var span = new ReadOnlySpan<SchemaTemplateItem>(template.Items);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1; // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = entityTypeId.CompareTo(span[indexerMiddle].EntityTypeId);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	internal static SchemaTemplateAttribute GetAttribute(this SchemaTemplate template, EntityType entityType, SchemaTemplateAttributeType attributeType, ref int attributeNotFound)
	{
		// Code size: 36 (0x24)
		var item = template.GetTemplateItem(entityType);
		if (item != null)
		{
			var attribute = item.GetAttribute(attributeType);
			if (attribute != null) return attribute;
		}
		// log here !!! failed to load schema attribute
		++attributeNotFound;
		return DefaultTemplateAttribute;
	}		 

}
