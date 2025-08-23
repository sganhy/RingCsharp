using Ring.Util.Enums;

namespace Ring.Util.Models;

internal sealed class XmlSchemaTemplate
{
	internal readonly XmlTemplateType Type;
	internal readonly XmlSchemaTemplateItem[] Items;

	public XmlSchemaTemplate(XmlTemplateType type, XmlSchemaTemplateItem[] xmlTemplateItems)
	{
		Type = type;
		Items = xmlTemplateItems;
	}

}
