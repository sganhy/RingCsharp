using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Helpers;

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
		var result = ResourceHelper.GetParameter(ParameterType.MinPoolSize).ToParameter();

		// assert
		Assert.NotNull(result);
        Assert.Equal((int)ParameterType.MinPoolSize, result.Id);
        Assert.Equal(ParameterType.MinPoolSize, result.Type);
        Assert.Equal("@MinConnPoolSize", result.Name);
        Assert.Equal("Minimum number of connections in the database pool.", result.Description);
		Assert.Equal("1", result.Value);
		Assert.Equal(EntityType.Schema, result.ReferenceType);
    }

	[Fact]
	public void GetParameter_MaxPoolSize_ParameterObject()
	{
		// arrange 
		// act 
		var result = ResourceHelper.GetParameter(ParameterType.MaxPoolSize).ToParameter();

		// assert
		Assert.NotNull(result);
		Assert.Equal((int)ParameterType.MaxPoolSize, result.Id);
		Assert.Equal(ParameterType.MaxPoolSize, result.Type);
		Assert.Equal("@MaxConnPoolSize", result.Name);
		Assert.Equal("Maximum number of connections in the database pool.", result.Description);
		Assert.Equal("2", result.Value);
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
        Assert.Equal("Unsupported parameter type : 'Undefined'.", ex.Message);
    }

	[Fact]
	public void GetParameter_SchemaVersion_ParameterObject()
	{
		// arrange 
		// act 
		var result = ResourceHelper.GetParameter(ParameterType.SchemaVersion).ToParameter();

		// assert
		Assert.NotNull(result);
		Assert.Equal((int)ParameterType.SchemaVersion, result.Id);
		Assert.Equal(ParameterType.SchemaVersion, result.Type);
		Assert.Equal("@SchemaVersion", result.Name);
		Assert.Equal("Database schema version.", result.Description);
		Assert.Equal("1.01", result.Value);
		Assert.Equal(EntityType.Schema, result.ReferenceType);
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
