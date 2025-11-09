using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class SchemaTemplateAttribute
{
	internal readonly string Name;
	internal readonly int TypeId;
	internal readonly SchemaTemplateAttributeType Type;
	internal readonly SchemaTemplateAttributeValue[] AttributeValues; // sorted by upper name !!

	internal SchemaTemplateAttribute(string name, SchemaTemplateAttributeType type, SchemaTemplateAttributeValue[] values)
	{
		TypeId = (int)type;
		Type = type;
		Name = name;
		AttributeValues = values;
	}

#if DEBUG
	public override string ToString() => $"{Name} ({Type})";
#endif

}
