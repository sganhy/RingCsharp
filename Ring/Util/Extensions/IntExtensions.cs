using Ring.Schema.Enums;
using Ring.Util.Enums;

namespace Ring.Util.Extensions;

internal static class IntExtensions
{
	#region constants

	// Xml schema attribute type constants
	private const int XmlAttributeId = (int)XmlSchemaAttributeType.Id;
	private const int XmlAttributeNameId = (int)XmlSchemaAttributeType.Name;
	private const int XmlAttributeBaseLineId = (int)XmlSchemaAttributeType.BaseLine;
	private const int XmlAttributeReadOnlyId = (int)XmlSchemaAttributeType.ReadOnly;
	private const int XmlAttributeCachedId = (int)XmlSchemaAttributeType.Cached;
    private const int XmlAttributeTypeId = (int)XmlSchemaAttributeType.Type;
    private const int XmlAttributeSizeId = (int)XmlSchemaAttributeType.Size;
    private const int XmlAttributeCaseSensitiveId = (int)XmlSchemaAttributeType.CaseSensitive;
    private const int XmlAttributeNotNullId = (int)XmlSchemaAttributeType.NotNull;
    private const int XmlAttributeMultilingualId = (int)XmlSchemaAttributeType.Multilingual;
    private const int XmlAttributeToId = (int)XmlSchemaAttributeType.To;
    private const int XmlAttributeInverseRelationId = (int)XmlSchemaAttributeType.InverseRelation;
    private const int XmlAttributeConstraintId = (int)XmlSchemaAttributeType.Constraint;
    private const int XmlAttributeUniqueId = (int)XmlSchemaAttributeType.Unique;

    #endregion

    internal static XmlSchemaAttributeType ToXmlSchemaAttributeType(this int xmlAttributeId)
	{
        // Code size: 86 (0x56)
        switch (xmlAttributeId)
		{
			case XmlAttributeId: return XmlSchemaAttributeType.Id;
			case XmlAttributeNameId: return XmlSchemaAttributeType.Name;
			case XmlAttributeBaseLineId: return XmlSchemaAttributeType.BaseLine;
			case XmlAttributeReadOnlyId: return XmlSchemaAttributeType.ReadOnly;
			case XmlAttributeCachedId: return XmlSchemaAttributeType.Cached;
            case XmlAttributeTypeId: return XmlSchemaAttributeType.Type;
            case XmlAttributeSizeId: return XmlSchemaAttributeType.Size;
            case XmlAttributeCaseSensitiveId: return XmlSchemaAttributeType.CaseSensitive;
            case XmlAttributeNotNullId: return XmlSchemaAttributeType.NotNull;
            case XmlAttributeMultilingualId: return XmlSchemaAttributeType.Multilingual;
            case XmlAttributeToId: return XmlSchemaAttributeType.To;
            case XmlAttributeInverseRelationId: return XmlSchemaAttributeType.InverseRelation;
            case XmlAttributeConstraintId: return XmlSchemaAttributeType.Constraint;
            case XmlAttributeUniqueId: return XmlSchemaAttributeType.Unique;
        }
        return XmlSchemaAttributeType.Undefined;
	}

}
