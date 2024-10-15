using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public sealed class IntExtensionsTest : BaseExtensionsTest
{
    [Fact]
    public void ToRelationType_AllExistingEnumId_Enum()
    {
        // arrange 
        var relationTypes = Enum.GetValues<RelationType>();
        foreach (var relType in relationTypes)
        {
            // act 
            var relationTypeResult = IntExtensions.ToRelationType((int)relType);
            // assert 
            Assert.Equal(relType, relationTypeResult);
        }
    }

    [Fact]
    public void ToRelationType_125_Undefined()
    {
        // arrange 
        // act 
        var relationTypeResult = IntExtensions.ToRelationType(125);
        // assert 
        Assert.Equal(RelationType.Undefined, relationTypeResult);
    }

    [Fact]
    public void ToFieldType_AllExistingEnumId_Enum()
    {
        // arrange 
        var fieldTypes = Enum.GetValues<FieldType>();
        foreach (var fieldType in fieldTypes)
        {
            // act 
            var fieldTypeResult = IntExtensions.ToFieldType((int)fieldType);
            // assert 
            Assert.Equal(fieldType, fieldTypeResult);
        }
    }

    [Fact]
    public void ToFieldType_126_Undefined()
    {
        // arrange 
        // act 
        var relationTypeResult = IntExtensions.ToFieldType(126);
        // assert 
        Assert.Equal(FieldType.Undefined, relationTypeResult);
    }

    [Fact]
    public void ToTableType_AllExistingEnumId_Enum()
    {
        // arrange 
        var tableTypes = Enum.GetValues<TableType>();
        foreach (var tableType in tableTypes)
        {
            // act 
            var tableTypeResult = IntExtensions.ToTableType((int)tableType);
            // assert 
            Assert.Equal(tableType, tableTypeResult);
        }
    }

    [Fact]
    public void ToTableType_124_Undefined()
    {
        // arrange 
        // act 
        var tableTypeResult = IntExtensions.ToTableType(124);
        // assert 
        Assert.Equal(TableType.Undefined, tableTypeResult);
    }


    [Fact]
    public void ToEntityType_AllExistingEnumId_Enum()
    {
        // arrange 
        var entityTypes = Enum.GetValues<EntityType>();
        foreach (var tableType in entityTypes)
        {
            // act 
            var entityTypeResult = IntExtensions.ToEntityType((int)tableType);
            // assert 
            Assert.Equal(tableType, entityTypeResult);
        }
    }

    [Fact]
    public void ToEntityType_125_Undefined()
    {
        // arrange 
        // act 
        var entityTypeResult = IntExtensions.ToEntityType(125);
        // assert 
        Assert.Equal(EntityType.Undefined, entityTypeResult);
    }

    [Fact]
    public void ToParameterType_AllExistingEnumId_Enum()
    {
        // arrange 
        var parameterTypes = Enum.GetValues<ParameterType>();
        foreach (var tableType in parameterTypes)
        {
            // act 
            var parameterTypeResult = IntExtensions.ToParameterType((int)tableType);
            // assert 
            Assert.Equal(tableType, parameterTypeResult);
        }
    }

    [Fact]
    public void ToParameterType_2147483645_Undefined()
    {
        // arrange 
        // act 
        var parameterTypeResult = IntExtensions.ToParameterType(int.MaxValue-2);
        // assert 
        Assert.Equal(ParameterType.Undefined, parameterTypeResult);
    }

}