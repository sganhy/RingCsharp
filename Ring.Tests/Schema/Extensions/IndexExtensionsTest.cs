using Bogus;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

using Index = Ring.Schema.Models.Index;

namespace Ring.Tests.Schema.Extensions;

public sealed class IndexExtensionsTest : BaseTest
{
    private readonly Faker _faker = new();

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
        var columns = _faker.Random.WordsArray(5).ToArray();
        var index1 = new Index(id, name, description, columns, true, false, true, false);
        var index2 = new Index(id, name, description, columns, true, false, true, false);

        // act 
        var hash1 = IndexExtensions.GetHashCode(index1);
        var hash2 = IndexExtensions.GetHashCode(index2);

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_IndexHashEqual_False()
    {
        // arrange 
        var id = _faker.Random.Number();
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var columns = _faker.Random.WordsArray(5).ToArray();
        var columns2 = _faker.Random.WordsArray(7).ToArray();
        var index1 = new Index(id, name, description, columns, true, false, true, false);
        var index2 = new Index(id, name, description, columns2, true, false, true, false);
        var index3 = new Index(id, name, description, columns, true, false, true, true);

        // act 
        var hash1 = IndexExtensions.GetHashCode(index1);
        var hash2 = IndexExtensions.GetHashCode(index2);
        var hash3 = IndexExtensions.GetHashCode(index3);

        // assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

}
