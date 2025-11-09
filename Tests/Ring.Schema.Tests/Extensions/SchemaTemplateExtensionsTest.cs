using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Helpers;
using System.Linq.Expressions;
using Xunit;

namespace Ring.Schema.Tests.Extensions;

public sealed class SchemaTemplateExtensionsTest
{
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

}
