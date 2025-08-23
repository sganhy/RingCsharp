using Ring.Util.Enums;

namespace Ring.Util.Models;

internal sealed class XmlSchemaAttribute
{
	internal readonly XmlSchemaAttributeType Type;
	internal readonly string Name;

	internal XmlSchemaAttribute(XmlSchemaAttributeType type, string name)
	{
		Type = type;
		Name = name;
	}

#if DEBUG
	public override string ToString() => $"{Name} ({Type})";
#endif

}
