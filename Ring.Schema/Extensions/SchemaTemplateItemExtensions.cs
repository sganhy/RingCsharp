using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateItemExtensions
{

    internal static SchemaTemplateAttribute GetAttribute(this SchemaTemplateItem schemaTemplateItem, SchemaTemplateAttributeType schemaTemplateAttributeType)
    { 
        // Code size: 66 (0x42)
        foreach (var attribute in schemaTemplateItem.Attributes)
        {
            if (attribute.Type == schemaTemplateAttributeType)
            {
                return attribute;
            }
        }
        throw new InvalidOperationException($"SchemaTemplateItem '{schemaTemplateItem.Tag}' does not contain a 'Name' attribute.");
    }
}
