using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System;

namespace Ring.Tests.Schema.Extensions;

public class ParameterTypeExtensionsTest
{
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

    [Fact]
    internal void GetValueType_MinPoolSize_Int()
    {
        // arrange 
        var paramType = ParameterType.MinPoolSize;

        // act 
        var defaultValue = paramType.GetValueType();

        // assert
        Assert.Equal(FieldType.Int, defaultValue);
    }

    [Fact]
    internal void GetValueType_MaxPoolSize_Int()
    {
        // arrange 
        var paramType = ParameterType.MaxPoolSize;

        // act 
        var defaultValue = paramType.GetValueType();

        // assert
        Assert.Equal(FieldType.Int, defaultValue);
    }

    [Fact]
    internal void GetName_SchemaVersion_Version()
    {
        // arrange 
        var paramType = ParameterType.SchemaVersion;
        var expetedName = "@version";

        // act 
        var result = paramType.GetName();

        // assert
        Assert.Equal(expetedName, result);
    }


    [Fact]
    internal void GetDescription_DefaultLanguage_Description()
    {
        // arrange 
        var paramType = ParameterType.DefaultLanguage;
        var expetedName = "Default language";

        // act 
        var result = paramType.GetDescription();

        // assert
        Assert.Equal(expetedName, result);
    }

}
