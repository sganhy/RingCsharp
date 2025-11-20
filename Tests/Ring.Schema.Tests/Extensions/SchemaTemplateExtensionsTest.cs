using Bogus;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
using ResourceHelper = Ring.Schema.Helpers.ResourceHelper;
using Xunit;

namespace Ring.Schema.Tests.Extensions;

public sealed class SchemaTemplateExtensionsTest
{
	private readonly Faker _faker = new();

	public SchemaTemplateExtensionsTest() => Expression.Empty();

	[Fact]
	public void GetTemplateItem_RandomEntityTypeId_SchemaTemplateItem()
	{
		// arrange 
		var template = ResourceHelper.GetSchemaTemplate(DocumentType.XmlNative);

		// act 
		Assert.NotNull(template);
		var templateItemTable = SchemaTemplateExtensions.GetTemplateItem(template, EntityType.Table);
		var templateItemField = SchemaTemplateExtensions.GetTemplateItem(template, EntityType.Field);
		var templateItemRelation = SchemaTemplateExtensions.GetTemplateItem(template, EntityType.Relation);
		var templateItemIndex = SchemaTemplateExtensions.GetTemplateItem(template, EntityType.Index);
		var templateItemSchema = SchemaTemplateExtensions.GetTemplateItem(template, EntityType.Schema);

		// assert 
		Assert.NotNull(templateItemTable);
		Assert.NotNull(templateItemField);
		Assert.NotNull(templateItemRelation);
		Assert.NotNull(templateItemIndex);
		Assert.NotNull(templateItemSchema);
		Assert.Equal(EntityType.Table,templateItemTable.EntityType);
		Assert.Equal(EntityType.Field, templateItemField.EntityType);
		Assert.Equal(EntityType.Relation, templateItemRelation.EntityType);
		Assert.Equal(EntityType.Index, templateItemIndex.EntityType);
		Assert.Equal(EntityType.Schema, templateItemSchema.EntityType);
	}

	[Fact]
	public void GetAttribute_RandomValues_AttributeNotFound2()
	{
		// arrange
		var emptyTemplate = new SchemaTemplate(string.Empty, DocumentType.Undefined, Array.Empty<SchemaTemplateItem>(), 11);
		var notFoundCount = 0;

		// act 
		var attribute1 = SchemaTemplateExtensions.GetAttribute(emptyTemplate, _faker.PickRandom<EntityType>(), _faker.PickRandom<SchemaTemplateAttributeType>(), ref notFoundCount);
		var attribute2 = SchemaTemplateExtensions.GetAttribute(emptyTemplate, _faker.PickRandom<EntityType>(), _faker.PickRandom<SchemaTemplateAttributeType>(), ref notFoundCount);
		var attribute3 = SchemaTemplateExtensions.GetAttribute(emptyTemplate, _faker.PickRandom<EntityType>(), _faker.PickRandom<SchemaTemplateAttributeType>(), ref notFoundCount);

		// assert
		Assert.NotNull(attribute1);
		Assert.NotNull(attribute2);
		Assert.NotNull(attribute3);
		Assert.Equal(3, notFoundCount);
	}

}
