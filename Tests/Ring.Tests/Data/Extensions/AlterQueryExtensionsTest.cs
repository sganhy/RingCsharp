using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Linq.Expressions;

namespace Ring.Tests.Data.Extensions;

public sealed class AlterQueryExtensionsTest : BaseTest
{
    public AlterQueryExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    internal void GetHashCode_AlterQueryEqual_True()
    {
        // arrange 
        var builder = new TableBuilder();
        var metaTable = builder.GetMeta("Test", DatabaseProvider.SqlServer);
        var tableSpace = new TableSpace(_faker.Random.Number(1000), _faker.Random.String(20), _faker.Random.String(20), _faker.Random.String(30),
            false, true, false, Array.Empty<string>(), _faker.Random.String(20), true, true);
        var type = AlterQueryType.CreateTable;
        var id = _faker.Random.Number(100);
        var alterQuery1 = new AlterQuery(id, metaTable, type, metaTable.Columns[2], null, null, tableSpace);
        var alterQuery2 = new AlterQuery(id, metaTable, type, metaTable.Columns[2], null, null, tableSpace);

        // act 
        var hash1 = AlterQueryExtensions.Hash(alterQuery1);
        var hash2 = alterQuery2.GetHashCode();

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_IndexHashEqual_False()
    {
        // arrange 
        var builder = new TableBuilder();
        var metaTable = builder.GetMeta("Test", DatabaseProvider.SqlServer);
        var metaIdTable = builder.GetMetaId("Test", DatabaseProvider.SqlServer);
        var tableSpace = new TableSpace(_faker.Random.Number(1000), _faker.Random.String(20), _faker.Random.String(20), _faker.Random.String(30),
            false, true, false, Array.Empty<string>(), _faker.Random.String(20), true, true);
        var type1 = AlterQueryType.CreateTable;
        var type2 = type1;
        var type3 = AlterQueryType.CreateIndex;
        var alterQuery1 = new AlterQuery(_faker.Random.Number(150), metaTable, type1,  metaTable.Columns[2], null, null, tableSpace);
        var alterQuery2 = new AlterQuery(_faker.Random.Number(250), metaIdTable, type2,  metaTable.Columns[2], null, null, tableSpace);
        var alterQuery3 = new AlterQuery(_faker.Random.Number(250), metaTable, type3,  metaTable.Columns[2], null, null, tableSpace);

        // act 
        var hash1 = AlterQueryExtensions.Hash(alterQuery1);
        var hash2 = AlterQueryExtensions.Hash(alterQuery2);
        var hash3 = AlterQueryExtensions.Hash(alterQuery3);

        // assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

}
