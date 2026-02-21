using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Schema.Extensions;

public class SchemaExtensionsTest : BaseTest
{
    private readonly DbSchema _schema ;

    public SchemaExtensionsTest(ITestOutputHelper output) : base(output)
    {
        var metaList = GetSchema1();
        var meta = Meta.Create("Test");
        _schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql) ?? 
            Meta.GetDefaultSchema(meta, DatabaseProvider.PostgreSql);
    }

    [Fact]
    internal void GetTable_Schema_Null()
    {
        // arrange 
        // act 
        var table1 = _schema?.GetTable("book888");
        var table2 = _schema?.GetTable(888888888);

        // assert
        Assert.Null(table1);
        Assert.Null(table2);
    }

    [Fact]
    internal void GetTable_Schema_Table()
    {
        // arrange 
        // act 
        var table1 = _schema?.GetTable("armor");
        var table2 = _schema?.GetTable(1015);

        // assert
        Assert.NotNull(table1);
        Assert.NotNull(table2);
        Assert.Equal("armor", table1.Name);
        Assert.Equal("armor", table2.Name);
    }

}
