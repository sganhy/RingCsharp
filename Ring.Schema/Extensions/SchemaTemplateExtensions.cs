using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateExtensions
{
	internal static Dictionary<string, SchemaTemplateItem> ToTagDictionary(this SchemaTemplate template, StringComparer stringComparer)
	{
		// Code size: 99 (0x63)
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

}
