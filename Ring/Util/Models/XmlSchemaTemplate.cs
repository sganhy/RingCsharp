using Ring.Schema.Enums;

namespace Ring.Util.Models;

internal sealed class XmlSchemaTemplate
{
	internal readonly DocumentType Type;
	internal readonly XmlSchemaTemplateItem[] Items;

	public XmlSchemaTemplate(DocumentType type, XmlSchemaTemplateItem[] xmlTemplateItems)
	{
		Type = type;
		Items = xmlTemplateItems;
	}

}
