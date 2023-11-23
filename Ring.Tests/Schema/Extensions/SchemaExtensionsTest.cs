using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Schema.Extensions;

public class SchemaExtensionsTest : BaseExtensionsTest
{
    private readonly DbSchema? _schema ; 
    public SchemaExtensionsTest()
    {
        var metaList = GetSchema1();
        _schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql);
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
