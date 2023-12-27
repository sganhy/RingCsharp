using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class IndexExtensionsTest : BaseExtensionsTest
{
    [Fact]
    public void ToMeta_Index1_MetaObject()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("race");
        var index1 = table?.GetIndex("name");

        // act 
        Assert.NotNull(index1);
        Assert.NotNull(table);
        var meta = IndexExtensions.ToMeta(index1, table.Id);

        // assert
        Assert.NotNull(meta);
        Assert.Equal(1, meta.GetEntityId());
        Assert.Equal(3, meta.ObjectType);
        Assert.Equal(1051, meta.ReferenceId);
        Assert.Equal(0, meta.DataType);
        Assert.Equal(8704, meta.Flags);
        Assert.Equal("name", meta.GetEntityName());
        Assert.Equal("name;race2book", meta.Value);
        Assert.True(meta.IsEntityActive());
    }

    [Fact]
    public void ToMeta_Index2_MetaObject()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("class");
        var index2 = table?.GetIndex("prestige");

        // act 
        Assert.NotNull(index2);
        Assert.NotNull(table);
        var meta = IndexExtensions.ToMeta(index2, table.Id);

        // assert
        Assert.NotNull(meta);
        Assert.Equal(4, meta.GetEntityId());
        Assert.Equal(3, meta.ObjectType);
        Assert.Equal(1031, meta.ReferenceId);
        Assert.Equal(0, meta.DataType);
        Assert.Equal(8192, meta.Flags);
        Assert.Equal("prestige", meta.GetEntityName());
        Assert.Equal("prestige", meta.Value);
        Assert.True(meta.IsEntityActive());
    }

}
