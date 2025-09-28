using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class SchemaTemplate
{
    internal readonly DocumentType Type;
    internal readonly SchemaTemplateItem[] Items;

    public SchemaTemplate(DocumentType type, SchemaTemplateItem[] xmlTemplateItems)
    {
        Type = type;
        Items = xmlTemplateItems;
    }

}
