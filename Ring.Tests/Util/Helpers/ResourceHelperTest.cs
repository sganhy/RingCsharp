using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Helpers;

namespace Ring.Tests.Util.Helpers;

public sealed class ResourceHelperTest
{

    [Fact]
    public void GetSchemaTemplate_NativeXmlTemplateType_Ok()
    {
        // arrange
        // act 
        var template = ResourceHelper.GetSchemaTemplate(XmlTemplateType.Native);

        // assert
        Assert.NotNull(template);
        Assert.True(template.Items.Length>2); // TABLE
        Assert.Equal("TABLE", template.Items[0].Tag); // TABLE
        Assert.Equal(EntityType.Table, template.Items[0].EntityType);
        Assert.Equal("SCHEMA", template.Items[0].ParentTag);
        Assert.Equal(5, template.Items[0].Attributes.Length);
        Assert.Equal("ID", template.Items[0].Attributes[0].Name); // id,name,baseline,readonly,cached 
        Assert.Equal("NAME", template.Items[0].Attributes[1].Name);
        Assert.Equal("BASELINE", template.Items[0].Attributes[2].Name);
        Assert.Equal("READONLY", template.Items[0].Attributes[3].Name);
        Assert.Equal("CACHED", template.Items[0].Attributes[4].Name);
        
        Assert.Equal("FIELD", template.Items[1].Tag); // FIELD
        Assert.Equal(7, template.Items[1].Attributes.Length);
        Assert.Equal("FIELD_LIST", template.Items[1].ParentTag);
        Assert.Equal("NAME", template.Items[1].Attributes[0].Name); // ,name,baseline,,,type,size,case_sensitive,not_null,multilingual
        Assert.Equal("BASELINE", template.Items[1].Attributes[1].Name);
        Assert.Equal("TYPE", template.Items[1].Attributes[2].Name);
        Assert.Equal("SIZE", template.Items[1].Attributes[3].Name);
        Assert.Equal("CASE_SENSITIVE", template.Items[1].Attributes[4].Name);
        
        Assert.Equal("RELATION", template.Items[2].Tag); // RELATION
        Assert.Equal(6, template.Items[2].Attributes.Length);
        Assert.Equal("NAME", template.Items[2].Attributes[0].Name); // ,name,baseline,,,type,,,,,to,inverse_relation,constraint
        Assert.Equal("BASELINE", template.Items[2].Attributes[1].Name); 
        Assert.Equal("TYPE", template.Items[2].Attributes[2].Name);
        Assert.Equal("TO", template.Items[2].Attributes[3].Name);
        Assert.Equal("INVERSE_RELATION", template.Items[2].Attributes[4].Name);
        Assert.Equal("CONSTRAINT", template.Items[2].Attributes[5].Name);

        Assert.Equal("INDEX", template.Items[3].Tag); // INDEX
        Assert.Equal("INDEX_LIST", template.Items[3].ParentTag);
        Assert.Equal(3, template.Items[3].Attributes.Length);

    }


    [Fact]
    public void GetErrorMessage_RecordValueTooLarge_Message()
    {
        // arrange 
        var expectedValue = "Value was either too high or too low for an {0}.";

        // act 
        var result = ResourceHelper.GetErrorMessage(ResourceType.RecordValueTooLarge);

        // assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetErrorMessage_RecordWrongRelationType_Message()
    {
        // arrange 
        var expectedValue = "Relation name '{0}' has a wrong RelationType.";

        // act 
        var result = ResourceHelper.GetErrorMessage(ResourceType.RecordWrongRelationType);

        // assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetErrorMessage_UnRepresentableDateTime_Message()
    {
        // arrange 
        var expectedValue = "Year, Month, and Day parameters describe an un-representable DateTime.";

        // act 
        var result = ResourceHelper.GetErrorMessage(ResourceType.UnRepresentableDateTime);

        // assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetErrorMessage_CreateTableNotOk_Message()
    {
        // arrange 
        var expectedValue = "create table";

        // act 
        var result = ResourceHelper.GetErrorMessage(ResourceType.CreateTableNotOk);

        // assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetErrorMessage_UnsuportedOperation_Message()
    {
        // arrange 
        var expectedValue = "Operation {0}.{1} #{2} is not supported.";

        // act 
        var result = ResourceHelper.GetErrorMessage(ResourceType.UnsuportedOperation);

        // assert
        Assert.Equal(expectedValue, result);
    }

}
