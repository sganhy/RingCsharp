using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class IntExtensions
{
	#region constants

	// Xml schema attribute type constants
	private const int XmlAttributeId = (int)SchemaTemplateAttributeType.Id;
	private const int XmlAttributeNameId = (int)SchemaTemplateAttributeType.Name;
	private const int XmlAttributeBaseLineId = (int)SchemaTemplateAttributeType.BaseLine;
	private const int XmlAttributeReadOnlyId = (int)SchemaTemplateAttributeType.ReadOnly;
	private const int XmlAttributeCachedId = (int)SchemaTemplateAttributeType.Cached;
	private const int XmlAttributeTypeId = (int)SchemaTemplateAttributeType.Type;
	private const int XmlAttributeSizeId = (int)SchemaTemplateAttributeType.Size;
	private const int XmlAttributeCaseSensitiveId = (int)SchemaTemplateAttributeType.CaseSensitive;
	private const int XmlAttributeNotNullId = (int)SchemaTemplateAttributeType.NotNull;
	private const int XmlAttributeMultilingualId = (int)SchemaTemplateAttributeType.Multilingual;
	private const int XmlAttributeToId = (int)SchemaTemplateAttributeType.To;
	private const int XmlAttributeInverseRelationId = (int)SchemaTemplateAttributeType.InverseRelation;
	private const int XmlAttributeConstraintId = (int)SchemaTemplateAttributeType.Constraint;
	private const int XmlAttributeUniqueId = (int)SchemaTemplateAttributeType.Unique;
	private const int XmlAttributeParentId = (int)SchemaTemplateAttributeType.Parent;
	private const int XmlAttributeValueId = (int)SchemaTemplateAttributeType.Value;


	#endregion

	internal static SchemaTemplateAttributeType ToXmlSchemaAttributeType(this int xmlAttributeId)
	{
		// Code size: 146 (0x92)
		switch (xmlAttributeId)
		{
			case XmlAttributeId: return SchemaTemplateAttributeType.Id;
			case XmlAttributeNameId: return SchemaTemplateAttributeType.Name;
			case XmlAttributeBaseLineId: return SchemaTemplateAttributeType.BaseLine;
			case XmlAttributeReadOnlyId: return SchemaTemplateAttributeType.ReadOnly;
			case XmlAttributeCachedId: return SchemaTemplateAttributeType.Cached;
			case XmlAttributeTypeId: return SchemaTemplateAttributeType.Type;
			case XmlAttributeSizeId: return SchemaTemplateAttributeType.Size;
			case XmlAttributeCaseSensitiveId: return SchemaTemplateAttributeType.CaseSensitive;
			case XmlAttributeNotNullId: return SchemaTemplateAttributeType.NotNull;
			case XmlAttributeMultilingualId: return SchemaTemplateAttributeType.Multilingual;
			case XmlAttributeToId: return SchemaTemplateAttributeType.To;
			case XmlAttributeInverseRelationId: return SchemaTemplateAttributeType.InverseRelation;
			case XmlAttributeConstraintId: return SchemaTemplateAttributeType.Constraint;
			case XmlAttributeUniqueId: return SchemaTemplateAttributeType.Unique;
			case XmlAttributeParentId: return SchemaTemplateAttributeType.Parent;
			case XmlAttributeValueId: return SchemaTemplateAttributeType.Value;
		}
		return SchemaTemplateAttributeType.Undefined;
	}

}
