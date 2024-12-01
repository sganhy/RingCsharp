using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class TableTypeExtensionsTest : BaseTest
{
    [Theory]
    [InlineData(TableType.Meta, "@meta")]
    [InlineData(TableType.MetaId, "@meta_id")]
    [InlineData(TableType.Log, "@log")]
    internal void GetLogicalName(TableType tableType, string expectedValue)
    {
        // arrange 
        // act 
        var result = TableTypeExtensions.GetLogicalName(tableType);

        // assert
        Assert.Equal(result, expectedValue);
    }

}
