using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class SchemaTemplateAttribute
{
    internal readonly SchemaTemplateAttributeType Type;
    internal readonly string Name;

    internal SchemaTemplateAttribute(SchemaTemplateAttributeType type, string name)
    {
        Type = type;
        Name = name;
    }

#if DEBUG
    public override string ToString() => $"{Name} ({Type})";
#endif

}
