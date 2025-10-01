using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
using Xunit.Abstractions;

using Index = Ring.Schema.Models.Index;

namespace Ring.Tests.Schema.Extensions;

public sealed class IndexExtensionsTest : BaseTest
{
    public IndexExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    public void ToMeta_Index1_MetaObject()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("race");
        var index1 = table?.GetIndex("name");

        // act 
        Assert.NotNull(index1);
        Assert.NotNull(table);
        var meta = IndexExtensions.ToMeta(index1, table.Id);

        // assert
        Assert.Equal(1, meta.Id);
        Assert.Equal(EntityType.Index, meta.GetEntityType());
        Assert.Equal(1051, meta.ReferenceId);
        Assert.Equal(0, meta.DataType);
        Assert.Equal(8704, meta.Flags);
        Assert.Equal("name", meta.Name);
        Assert.Equal("name;race2book", meta.Value);
        Assert.True(meta.Active);
    }

    [Fact]
    public void ToMeta_Index2_MetaObject()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("class");
        var index2 = table?.GetIndex("prestige");

        // act 
        Assert.NotNull(index2);
        Assert.NotNull(table);
        var meta = IndexExtensions.ToMeta(index2, table.Id);

        // assert
        Assert.Equal(4, meta.Id);
        Assert.Equal(EntityType.Index, meta.GetEntityType());
        Assert.Equal(1031, meta.ReferenceId);
        Assert.Equal(0, meta.DataType);
        Assert.Equal(8192, meta.Flags);
        Assert.Equal("prestige", meta.Name);
        Assert.Equal("prestige", meta.Value);
        Assert.True(meta.Active);
    }

    [Fact]
    internal void GetHashCode_IndexHashEqual_True()
    {
        // arrange 
        var id = _faker.Random.Number(10000);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var columnCount = 5;
        var columns = new List<Column>(columnCount);
        for (var i = 0; i < columnCount; ++i) columns.Add(GetAnonymousColumn());
        var index1 = new Index(id, name, description, columns.ToArray(), _faker.Random.String(), true, false, true, false);
        var index2 = new Index(id, name, description, columns.ToArray(), index1.ColumnList, true, false, true, false);

        // act 
        var hash1 = IndexExtensions.Hash(index1);
        var hash2 = index2.GetHashCode();

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_IndexHashEqual_False()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue,int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var columnList1 = _faker.Random.String(8);
        var columnList2 = _faker.Random.String(9);
        var colCount1 = 5;
        var colCount2 = 7;
        var columns = new List<Column>(colCount1);
        for (var i = 0; i < colCount1; ++i) columns.Add(GetAnonymousColumn());
        var columns2 = new List<Column>(colCount2);
        for (var i = 0; i < colCount2; ++i) columns2.Add(GetAnonymousColumn());
        var index1 = new Index(id, name, description, columns.ToArray(), columnList1, true, false, true, false);
        var index2 = new Index(id, name, description, columns2.ToArray(), columnList2, true, false, true, false);
        var index3 = new Index(id, name, description, columns.ToArray(), columnList1, true, false, true, true);

        // act 
        var hash1 = IndexExtensions.Hash(index1);
        var hash2 = IndexExtensions.Hash(index2);
        var hash3 = IndexExtensions.Hash(index3);

        // assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

    [Fact]
    internal void Equals_2AnonymousIndexes_False()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue, int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var columnList1 = _faker.Random.String(8);
        var columnList2 = _faker.Random.String(9);
        var colCount1 = 5;
        var colCount2 = 7;
        var columns = new List<Column>(colCount1);
        for (var i = 0; i < colCount1; ++i) columns.Add(GetAnonymousColumn());
        var columns2 = new List<Column>(colCount2);
        for (var i = 0; i < colCount2; ++i) columns2.Add(GetAnonymousColumn());
        var index1 = new Index(id, name, description, columns.ToArray(), columnList1, true, false, true, false);
        var index2 = new Index(id, name, description, columns2.ToArray(), columnList2, true, false, true, false);
        var index3 = new Index(id, name, description, columns.ToArray(), columnList1, true, false, true, true);
        var index4 = new Index(id, name, description, columns2.ToArray(), columnList2, true, false, true, false);

        // act 
        var result1 = index1 == index2;
        var result2 = index1.Equals(index3);
        var result3 = index2.Equals((object)index3);
        var result4 = index2 != index4;

        // assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.False(result4);
    }




}
