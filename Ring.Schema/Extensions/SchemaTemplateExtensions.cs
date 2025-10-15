using Ring.Schema.Models;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateExtensions
{
    internal static Dictionary<string, string> ToTagDictionary(this SchemaTemplate template, StringComparer stringComparer)
    { 
        var dict = new Dictionary<string, string>(template.Items.Length*2, stringComparer);
        var items = new ReadOnlySpan<SchemaTemplateItem>(template.Items);
        foreach (var item in items) if (!string.IsNullOrWhiteSpace(item.Tag) && !dict.ContainsKey(item.Tag))  dict.Add(item.Tag, item.ParentTag);
        return dict;
    }
}
