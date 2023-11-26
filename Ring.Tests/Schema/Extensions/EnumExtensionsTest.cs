using Ring.Schema.Attributes;
using Ring.Schema.Enums;
using Ring.Util.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class EnumExtensionsTest
{

    /// <summary>
    /// Default MinPoolSize should be equal to "1"
    /// </summary>
    [Fact]
    internal void GetCustomAttribute_MinPoolSize_Attribute()
    {
        // arrange 
        var paramType = ParameterType.MinPoolSize;

        // act 
        var attribute = paramType.GetCustomAttribute<ParameterTypeAttribute>();

        // assert
        Assert.NotNull(attribute);
        Assert.Equal("1", attribute.DefaultValue);
        Assert.Equal(EntityType.Schema, attribute.TargetEntity);
        Assert.Equal(FieldType.Int, attribute.ParameterDataType);
    }

    /// <summary>
    /// Default MaxPoolSize should be equal to "1"
    /// </summary>
    [Fact]
    internal void GetCustomAttribute_MaxPoolSize_Attribute()
    {
        // arrange 
        var paramType = ParameterType.MaxPoolSize;

        // act 
        var attribute = paramType.GetCustomAttribute<ParameterTypeAttribute>();

        // assert
        Assert.NotNull(attribute);
        Assert.Equal("1", attribute.DefaultValue);
        Assert.Equal(EntityType.Schema, attribute.TargetEntity);
        Assert.Equal(FieldType.Int, attribute.ParameterDataType);
    }

}
