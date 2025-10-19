using Ring.Schema.Enums;
using ResourceHelper = Ring.Schema.Helpers.ResourceHelper;
using Xunit;

namespace Ring.Schema.Tests.Resources;

public sealed class ResourceHelperTest
{
    [Fact]
    public void GetSchemaTemplate_NativeXmlTemplateType_Ok()
    {
        // arrange
        // act 
        var template = ResourceHelper.GetSchemaTemplate(DocumentType.XmlNative);

        // assert
        Assert.NotNull(template);
        Assert.Equal(3, template.MaxDepth);
        Assert.Equal("XmlNative.gz", template.ResourceFile);

        Assert.True(template.Items.Length > 2); // TABLE
        Assert.Equal("table", template.Items[0].Tag); // TABLE
        //Assert.Equal(EntityType.Table, template.Items[0].EntityType);
        Assert.Equal("schema", template.Items[0].ParentTag);
        Assert.Equal(1, template.Items[0].Depth);
        Assert.Equal(5, template.Items[0].Attributes.Length);
        Assert.Equal("id", template.Items[0].Attributes[0].Name); // id,name,baseline,readonly,cached 
        Assert.Equal("name", template.Items[0].Attributes[1].Name);
        Assert.Equal("baseline", template.Items[0].Attributes[2].Name);
        Assert.Equal("readonly", template.Items[0].Attributes[3].Name);
        Assert.Equal("cached", template.Items[0].Attributes[4].Name);

        Assert.Equal("field", template.Items[1].Tag); // FIELD
        Assert.Equal(7, template.Items[1].Attributes.Length);
        Assert.Equal(3, template.Items[1].Depth);
        Assert.Equal("field_list", template.Items[1].ParentTag);
        Assert.Equal("name", template.Items[1].Attributes[0].Name); // ,name,baseline,,,type,size,case_sensitive,not_null,multilingual
        Assert.Equal("baseline", template.Items[1].Attributes[1].Name);
        Assert.Equal("type", template.Items[1].Attributes[2].Name);
        Assert.Equal("size", template.Items[1].Attributes[3].Name);
        Assert.Equal("case_sensitive", template.Items[1].Attributes[4].Name);

        Assert.Equal("relation", template.Items[2].Tag); // RELATION
        Assert.Equal(7, template.Items[2].Attributes.Length);
        Assert.Equal(3, template.Items[2].Depth);
        Assert.Equal("name", template.Items[2].Attributes[0].Name); // ,name,baseline,,,type,,,,,to,inverse_relation,constraint
        Assert.Equal("baseline", template.Items[2].Attributes[1].Name);
        Assert.Equal("type", template.Items[2].Attributes[2].Name);
        Assert.Equal("not_null", template.Items[2].Attributes[3].Name);
        Assert.Equal("to", template.Items[2].Attributes[4].Name);
        Assert.Equal("inverse_relation", template.Items[2].Attributes[5].Name);
        Assert.Equal("constraint", template.Items[2].Attributes[6].Name);

        Assert.Equal("index", template.Items[3].Tag); // INDEX
        Assert.Equal("index_list", template.Items[3].ParentTag);
        Assert.Equal(2, template.Items[3].Attributes.Length);

        Assert.Equal("schema", template.Items[4].Tag); // SCHEMA
        Assert.Equal(0, template.Items[4].Depth);
        Assert.Equal("", template.Items[4].ParentTag);
    }
}
