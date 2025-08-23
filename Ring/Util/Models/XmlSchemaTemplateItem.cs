using Ring.Schema.Enums;
using System.Xml.Linq;

namespace Ring.Util.Models;

internal sealed class XmlSchemaTemplateItem
{
	internal readonly EntityType EntityType;
	internal readonly string Tag;
	internal readonly string ParentTag;
	internal readonly string ChildDescriptionTag;
	internal readonly XmlSchemaAttribute [] Attributes;

	internal XmlSchemaTemplateItem(EntityType entityType, string startTag, string parentTag, string childDescriptionTag, XmlSchemaAttribute[] attributes)
	{
		EntityType = entityType;
		Tag = startTag;
		ParentTag = parentTag;
		ChildDescriptionTag = childDescriptionTag;
		Attributes = attributes;
	}

#if DEBUG
	public override string ToString() => $"{Tag} ({EntityType})";
#endif
}