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
        var alterQuery1 = new AlterQuery(id, metaTable, type, 2, null, null, tableSpace);
        var alterQuery2 = new AlterQuery(id, metaTable, type, 2, null, null, tableSpace);

        // act 
        var hash1 = AlterQueryExtensions.Hash(alterQuery1);
        var hash2 = alterQuery2.GetHashCode();

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_AlterQueryHashEqual_False()
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
        var alterQuery1 = new AlterQuery(_faker.Random.Number(150), metaTable, type1,  2, null, null, tableSpace);
        var alterQuery2 = new AlterQuery(_faker.Random.Number(250), metaIdTable, type2,  2, null, null, tableSpace);
        var alterQuery3 = new AlterQuery(_faker.Random.Number(250), metaTable, type3,  2, null, null, tableSpace);

        // act 
        var hash1 = AlterQueryExtensions.Hash(alterQuery1);
        var hash2 = AlterQueryExtensions.Hash(alterQuery2);
        var hash3 = AlterQueryExtensions.Hash(alterQuery3);

        // assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

	[Fact]
	internal void Equals_2AnonymousAlterQueries_False()
	{
		// arrange 
		var id = _faker.Random.Number(int.MinValue, int.MaxValue);
		var builder = new TableBuilder();
		var metaTable = builder.GetMeta("Test", DatabaseProvider.SqlServer);
		var metaIdTable = builder.GetMetaId("Test", DatabaseProvider.SqlServer);
		var tableSpace = new TableSpace(_faker.Random.Number(1000), _faker.Random.String(20), _faker.Random.String(20), _faker.Random.String(30),
			false, true, false, Array.Empty<string>(), _faker.Random.String(20), true, true);
		var type1 = AlterQueryType.CreateTable;
		var type2 = type1;
		var type3 = AlterQueryType.CreateIndex;
		var alterQuery1 = new AlterQuery(150, metaTable, type1, 2, null, null, tableSpace);
		var alterQuery2 = new AlterQuery(id, metaIdTable, type2, 2, null, null, tableSpace);
		var alterQuery3 = new AlterQuery(250, metaTable, type3, 2, null, null, tableSpace);
		var alterQuery4 = new AlterQuery(id, metaIdTable, type2, 2, null, null, tableSpace);
		var alterQuery5 = new AlterQuery(1250, metaIdTable, type3, 2, null, null, tableSpace);

		// act 
		var result1 = alterQuery1 == alterQuery2;
		var result2 = alterQuery1.Equals(alterQuery3);
		var result3 = alterQuery2.Equals((object)alterQuery3);
		var result4 = alterQuery2 != alterQuery4;
		var result5 = AlterQueryExtensions.IsEquivalentTo(alterQuery4, alterQuery5);

		// assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
		Assert.False(result5);
	}

	[Theory]
	[InlineData(AlterQueryType.CreateSchema, AlterQueryType.CreateTable, -1)]
	[InlineData(AlterQueryType.CreateTable, AlterQueryType.CreateSchema, 1)]
	[InlineData(AlterQueryType.CreateTable, AlterQueryType.CreateTable, 0)]
	[InlineData(AlterQueryType.CreatePrimaryKey, AlterQueryType.CreateIndex, -1)]
	[InlineData(AlterQueryType.Undefined, AlterQueryType.CreateIndex, 1)]
	internal void Compare_RandomAlterQueryType_ExpectedSign(AlterQueryType typeX, AlterQueryType typeY, int expectedSign)
	{
		// Arrange
		var comparer = default(AlterQuery);
		var x = new AlterQuery(id: 1, table: null!, type: typeX, columnIndex: -1, constraint: null, index: null, tableSpace: null);
		var y = new AlterQuery(id: 2, table: null!, type: typeY, columnIndex: -1, constraint: null, index: null, tableSpace: null);

		// Act
		int result = comparer.Compare(x, y);

		// Assert
		if (expectedSign < 0) Assert.True(result < 0);
		else if (expectedSign > 0) Assert.True(result > 0);
		else Assert.Equal(0, result);
	}

}
