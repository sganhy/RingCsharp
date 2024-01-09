using Ring.Data;
using Ring.Data.Extensions;
using Ring.Schema.Enums;

namespace Ring.Tests.Data.Extensions;

public sealed class OperationTypeExtensionsTest
{
    [Theory]
    [InlineData(OperatorType.Equal,"=", DatabaseProvider.PostgreSql)]
    [InlineData(OperatorType.NotEqual, "<>", DatabaseProvider.PostgreSql)]
    [InlineData(OperatorType.NotEqual, "!=", DatabaseProvider.Oracle)]
    [InlineData(OperatorType.Like, " like ", DatabaseProvider.Oracle)]
    internal void ToSql_Input_OnlyOneTrueFlag(OperatorType operatorType, string expectedValue, 
        DatabaseProvider databaseProvider)
    {
        // arrange 
        // act 
        var result = OperationTypeExtensions.ToSql(operatorType, databaseProvider);

        // assert
        Assert.Equal(result, expectedValue,true);
    }

}
