using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class ParameterExtensionsTest
{
    private readonly Parameter[] _parameterCollection;
    private readonly IFixture _fixture;
    private const int schemaId = 888;

    public ParameterExtensionsTest()
    {
        _fixture = new Fixture();
        // create collection of Parameter 
        var result = new List<Parameter>();
        foreach (var element in Enum.GetValues(typeof(ParameterType)))
            result.Add(new Parameter((int)element, _fixture.Create<string>(), _fixture.Create<string>(), (ParameterType)element,
                _fixture.Create<FieldType>(), ((ParameterType)element).GetDefaultValue() ?? string.Empty, schemaId,
                true,true)); 
        // sort by id 
        _parameterCollection = result.OrderBy(o => o.Hash).ToArray();
    }


    /// <summary>
    /// Default MinPoolSize should be equal to "1"
    /// </summary>
    [Fact]
    internal void GetParameter_MinPoolSize_1()
    {
        // arrange 
        var paramType = ParameterType.MinPoolSize;

        // act 
        var param = _parameterCollection.GetParameter(paramType, schemaId);

        // assert
        Assert.NotNull(param);
        Assert.Equal("1", param.Value);
    }

    /// <summary>
    /// Default MaxPoolSize should be equal to "1"
    /// </summary>
    [Fact]
    internal void GetParameter_MaxPoolSize_1()
    {
        // arrange 
        var paramType = ParameterType.MaxPoolSize;

        // act 
        var param = _parameterCollection.GetParameter(paramType, schemaId);

        // assert
        Assert.NotNull(param);
        Assert.Equal("1", param.Value);
    }

    /// <summary>
    /// Default MinPoolSize should be equal to 1
    /// </summary>
    [Fact]
    internal void GetMinPoolSize_Parameters_1()
    {
        // arrange 
        // act 
        var value = _parameterCollection.GetMinPoolSize(schemaId);

        // assert
        Assert.Equal(1, value);
    }

    /// <summary>
    /// Default MaxPoolSize should be equal to 1
    /// </summary>
    [Fact]
    internal void GetMaxPoolSize_Parameters_1()
    {
        // arrange 
        // act 
        var value = _parameterCollection.GetMaxPoolSize(schemaId);

        // assert
        Assert.Equal(1, value);
    }

    [Fact]
    internal void GetParameterHash_Parameter1_ReferenceId()
    {
        // arrange 
        var paramType = ParameterType.DefaultLanguage;
        var paramTypeId = (int)paramType;
        var referenceId = _fixture.Create<int>();

        // act 
        var value = ParameterExtensions.GetParameterHash(null, paramType, referenceId);

        // assert
        Assert.Equal(referenceId, value&int.MaxValue);
        Assert.Equal(paramTypeId, value>>32);
    }

    [Fact]
    internal void GetParameterHash_Parameter2_ReferenceId()
    {
        // arrange 
        var paramType = ParameterType.Undefined;
        var paramTypeId = (int)paramType;
        var referenceId = _fixture.Create<int>();

        // act 
        var value = ParameterExtensions.GetParameterHash(null, paramType, referenceId);

        // assert
        Assert.Equal(referenceId, value & int.MaxValue);
        Assert.Equal(paramTypeId, value >> 32);
    }

}
