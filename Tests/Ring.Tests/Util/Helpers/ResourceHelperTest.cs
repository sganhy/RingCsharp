using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using Xunit.Abstractions;

namespace Ring.Tests.Util.Helpers;

public sealed class ResourceHelperTest : BaseTest
{

    public ResourceHelperTest(ITestOutputHelper output) : base(output) 
    {
    }

	[Theory]
	[InlineData(ResourceType.RecordValueTooLarge, "Value was either too high or too low for an {0}.")]
	[InlineData(ResourceType.RecordWrongRelationType, "Relation name '{0}' has a wrong RelationType.")]
	[InlineData(ResourceType.UnRepresentableDateTime, "Year, Month, and Day parameters describe an un-representable DateTime.")]
	[InlineData(ResourceType.RecordUnkownRecordType, "This Record object has an unknown RecordType.  The RecordType \nproperty must be set before performing this operation.")]
	[InlineData(ResourceType.CreateTableNotOk, "create table")]
	[InlineData(ResourceType.UnsuportedOperation, "Operation {0}.{1} #{2} is not supported.")]
	internal void GetMessage_ResourceType_Message(ResourceType resourceType, string expectedMessage)
	{
		// arrange 
		// act 
		var result = ResourceHelper.GetMessage(resourceType);
		// assert
		Assert.Equal(expectedMessage, result);
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
	public void GetParameter_RingVersion_ParameterObject()
	{
		// arrange 
		// act 
		var result = ResourceHelper.GetParameter(ParameterType.Ring0Version);

		// assert
		Assert.Equal((int)ParameterType.Ring0Version, result.Id);
		Assert.Equal(ParameterType.Ring0Version, result.Type);
		Assert.Equal("@RingVersion", result.Name);
		Assert.Equal("Ring 0 version", result.Description);
		Assert.Null(result.DefaultValue);
		Assert.Equal("1", result.Value);
		Assert.True(result.Baseline);
	}


	[Theory]
	[InlineData(ResourceType.UnknownMessageResourceType, "GetMessage")]
	internal void GetMethod_ResourceType_MethodInfo(ResourceType resourceType, string expectedMessage)
	{
		// arrange 
		// act 
		var result = ResourceHelper.GetMethodInfo(resourceType);
		// assert
		Assert.Equal(expectedMessage, result);
	}


}
