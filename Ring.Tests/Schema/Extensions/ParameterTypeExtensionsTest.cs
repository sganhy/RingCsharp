using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class ParameterTypeExtensionsTest
{
    /// <summary>
    /// Default MinPoolSize should be equal to "1"
    /// </summary>
    [Fact]
    internal void GetDefaultValue_MinPoolSize_1()
    {
        // arrange 
        var paramType = ParameterType.MinPoolSize;

        // act 
        var defaultValue = paramType.GetDefaultValue();

        // assert
        Assert.NotNull(defaultValue);
        Assert.Equal("1", defaultValue);
    }

    /// <summary>
    /// Default MaxPoolSize should be equal to "1"
    /// </summary>
    [Fact]
    internal void GetDefaultValue_MaxPoolSize_1()
    {
        // arrange 
        var paramType = ParameterType.MaxPoolSize;

        // act 
        var defaultValue = paramType.GetDefaultValue();

        // assert
        Assert.NotNull(defaultValue);
        Assert.Equal("1", defaultValue);
    }

}
