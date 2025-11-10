using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Helpers;
using System.Linq.Expressions;
using Xunit;

namespace Ring.Schema.Tests.Extensions;

public sealed class SchemaTemplateAttributeExtensionsTest
{
	public SchemaTemplateAttributeExtensionsTest() => Expression.Empty();

	[Fact]
	public void GetFieldType_TypeXmlNative_FieldType()
	{
		// arrange 
		var template = ResourceHelper.GetSchemaTemplate(DocumentType.XmlNative);
		Assert.NotNull(template);
		var templateItem = template.GetTemplateItem(EntityType.Field);
		Assert.NotNull(templateItem);
		var templateAttributes = templateItem.GetAttribute(SchemaTemplateAttributeType.Type);
		Assert.NotNull(templateAttributes);

		// act 
		var fieldTypeLong = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "lonG");
		var fieldTypeInt = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "InT ");
		var fieldTypeShort = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, " ShoRt");
		var fieldTypeByte = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, " bytE   ");
		var fieldTypeFloat = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "floAt");
		var fieldTypeDouble = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, " double");
		var fieldTypeString = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "String");
		var fieldTypeShortDateTime = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "Date ");
		var fieldTypeDateTime = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "DATETIME");
		var fieldTypeDateTimeOffset = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "DATETIMEOffset");
		var fieldTypeByteArray = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "Array");
		var fieldTypeUndefined1 = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "XXXXXXXXXXXXX0123");
		var fieldTypeUndefined2 = SchemaTemplateAttributeExtensions.GetFieldType(templateAttributes, "568754874");

		// assert 
		Assert.Equal(FieldType.Long, fieldTypeLong);
		Assert.Equal(FieldType.Int, fieldTypeInt);
		Assert.Equal(FieldType.Short, fieldTypeShort);
		Assert.Equal(FieldType.Byte, fieldTypeByte);
		Assert.Equal(FieldType.Float, fieldTypeFloat);
		Assert.Equal(FieldType.Double, fieldTypeDouble);
		Assert.Equal(FieldType.String, fieldTypeString);
		Assert.Equal(FieldType.Date, fieldTypeShortDateTime);
		Assert.Equal(FieldType.DateTime, fieldTypeDateTime);
		Assert.Equal(FieldType.DateTimeOffset, fieldTypeDateTimeOffset);
		Assert.Equal(FieldType.ByteArray, fieldTypeByteArray);
		Assert.Equal(FieldType.Undefined, fieldTypeUndefined1);
		Assert.Equal(FieldType.Undefined, fieldTypeUndefined2);
	}

	[Fact]
	public void GetRelationType_TypeXmlNative_RelationType()
	{
		// arrange 
		var template = ResourceHelper.GetSchemaTemplate(DocumentType.XmlNative);
		Assert.NotNull(template);
		var templateItem = template.GetTemplateItem(EntityType.Relation);
		Assert.NotNull(templateItem);
		var templateAttributes = templateItem.GetAttribute(SchemaTemplateAttributeType.Type);
		Assert.NotNull(templateAttributes);

		// act 
		var relTypeOtm = SchemaTemplateAttributeExtensions.GetRelationType(templateAttributes, " otM");
		var relTypeMtm = SchemaTemplateAttributeExtensions.GetRelationType(templateAttributes, " MTM ");
		var relTypeMto = SchemaTemplateAttributeExtensions.GetRelationType(templateAttributes, " Mto ");
		var relTypeOtop = SchemaTemplateAttributeExtensions.GetRelationType(templateAttributes, " OTOP ");
		var relTypeOtof = SchemaTemplateAttributeExtensions.GetRelationType(templateAttributes, "OTOf");
		var relTypeUndefined = SchemaTemplateAttributeExtensions.GetRelationType(templateAttributes, "xXx0154");

		// assert 
		Assert.Equal(RelationType.Otm, relTypeOtm);
		Assert.Equal(RelationType.Mtm, relTypeMtm);
		Assert.Equal(RelationType.Mto, relTypeMto);
		Assert.Equal(RelationType.Otop, relTypeOtop);
		Assert.Equal(RelationType.Otop, relTypeOtop);
		Assert.Equal(RelationType.Otof, relTypeOtof);
		Assert.Equal(RelationType.Undefined, relTypeUndefined);
	}

	[Fact]
	public void GetSearchableType_CaseSensitiveXmlNative_SearchableType()
	{
		// arrange 
		var template = ResourceHelper.GetSchemaTemplate(DocumentType.XmlNative);
		Assert.NotNull(template);
		var templateItem = template.GetTemplateItem(EntityType.Field);
		Assert.NotNull(templateItem);
		var templateAttributes = templateItem.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		Assert.NotNull(templateAttributes);

		// act 
		var searchCaseSensitiveYes = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, " yes ");
		var searchCaseSensitiveTrue = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, " True ");
		var searchCaseSensitive1 = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, "1");
		var searchCaseSensitiveRandom = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, "wwwsqdfqsdkl,sd");
		var searchCaseSensitiveFalse = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, "False ");
		var searchCaseSensitive0 = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, "0 ");
		var searchCaseSensitiveNo = SchemaTemplateAttributeExtensions.GetSearchableType(templateAttributes, "no");

		// assert 
		Assert.Equal(SearchableType.None, searchCaseSensitiveYes);
		Assert.Equal(SearchableType.None, searchCaseSensitiveTrue);
		Assert.Equal(SearchableType.None, searchCaseSensitiveRandom);
		Assert.Equal(SearchableType.None, searchCaseSensitive1);
		Assert.Equal(SearchableType.IgnoreCase, searchCaseSensitiveFalse);
		Assert.Equal(SearchableType.IgnoreCase, searchCaseSensitive0);
		Assert.Equal(SearchableType.IgnoreCase, searchCaseSensitiveNo);
	}


}
