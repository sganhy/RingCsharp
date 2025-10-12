using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using Xunit.Abstractions;

namespace Ring.Tests.Util.Helpers;

public sealed class ResourceHelperTest : BaseTest
{
    private readonly ResourceHelper _sut;

    public ResourceHelperTest(ITestOutputHelper output) : base(output) 
    {
        _sut = new ResourceHelper();
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
    public void GetErrorMessage_RecordUnkownRecordType_Message()
    {
        // check multiline message
        // arrange  
        var expectedValue = "This Record object has an unknown RecordType.  The RecordType \nproperty must be set before performing this operation.";

        // act 
        var result = _sut.GetMessage(ResourceType.RecordUnkownRecordType);

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

    [Fact]
    public void GetParameter_MinPoolSize_ParameterObject()
    {
        // arrange 
        // act 
        var result = ResourceHelper.GetParameter(ParameterType.MinPoolSize);

        // assert
        Assert.Equal((int)ParameterType.MinPoolSize, result.Id);
        Assert.Equal(ParameterType.MinPoolSize, result.Type);
        Assert.Equal("@minConnPoolSize", result.Name);
        Assert.Equal("Mininimum database connection pool", result.Description);
        Assert.Equal("1", result.DefaultValue);
        Assert.Equal(EntityType.Schema, result.ReferenceType);
    }

    [Fact]
    public void GetParameter_Undefined_ThrowArgumentException()
    {
        // arrange 
        // act 
        var ex = Assert.Throws<ArgumentException>(() => {
            var test = ResourceHelper.GetParameter(ParameterType.Undefined);
        });

        // assert
        Assert.Equal("Unsupported parameter type : Undefined.", ex.Message);
    }

    [Fact]
    public void GetDescription_FileNotFound_Description()
    {
        // arrange 
        var expectedValue = "File '{0}' not found.";

        // act 
        var result = _sut.GetDescription(LogType.FileNotFound);

        // assert
        Assert.Equal(expectedValue, result);
    }
}
