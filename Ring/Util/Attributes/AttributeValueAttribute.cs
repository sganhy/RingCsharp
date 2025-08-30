using Ring.Schema.Enums;

namespace Ring.Util.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
internal sealed class AttributeValueAttribute : Attribute
{
	internal int FromIndex { get; }
	internal int ToIndex { get; }
	internal EntityType Type { get; }
	internal FieldType ValueType { get; }

	internal AttributeValueAttribute(EntityType type, int fromIndex, int toIndex, FieldType valueType)
	{
		Type = type;
		FromIndex = fromIndex;
		ToIndex = toIndex;
		ValueType = valueType;
	}

}
