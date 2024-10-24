using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Util.Builders.SQLite;

namespace Ring.Tests.Data.Extensions;

public sealed class AlterQueryExtensionsTest : BaseExtensionsTest
{
    [Fact]
    public void ToErrorMessage_CreateTable_Message()
    {
        // arrange 
        // Table table, AlterQueryType type, IDdlBuilder builder, IColumn? column=null
        var expectedValue = "Failed to create table 'test.test'.";
        var query = new AlterQuery(GetTable(TableType.Business, "test", "test"),
            AlterQueryType.CreateTable, new DdlBuilder(), null);

        // act 
        var message = AlterQueryExtensions.ToErrorMessage(query, new Exception());

        // assert
        Assert.Equal(expectedValue, message);
    }
}
