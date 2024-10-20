using Ring.Data;
using Ring.Data.Extensions;
using Ring.Schema.Enums;

namespace Ring.Tests.Data.Extensions;

public sealed class OperationTypeExtensionsTest
{
    [Theory]
    [InlineData(Operator.Equal,"=", DatabaseProvider.PostgreSql)]
    [InlineData(Operator.NotEqual, "<>", DatabaseProvider.PostgreSql)]
    [InlineData(Operator.NotEqual, "!=", DatabaseProvider.Oracle)]
    [InlineData(Operator.Like, " like ", DatabaseProvider.Oracle)]
    internal void ToSql_InputTestValue_OnlyOneTrueFlag(Operator operatorType, string expectedValue, 
        DatabaseProvider databaseProvider)
    {
        // arrange 
        // act 
        var result = OperationTypeExtensions.ToSql(operatorType, databaseProvider, "TEST");

        // assert
        Assert.Equal(result, expectedValue,true);
    }

}
