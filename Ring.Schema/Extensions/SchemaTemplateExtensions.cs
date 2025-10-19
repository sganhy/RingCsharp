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
    internal static SchemaTemplateItem GetItem(this SchemaTemplate template, EntityType entityType)
    {
        // Code size: 107 (0x6b)
        for (var i = 0; i < template.Items.Length; ++i)
		{
			var templ = template.Items[i];
            if (templ.EntityType == entityType) return templ;
		}
        throw new InvalidOperationException($"Template: {template.ResourceFile} doesn't contain definition of entityType: {entityType}");
    }
}
