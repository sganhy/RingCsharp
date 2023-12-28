using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Tests.Schema.Extensions;

public class FieldExtensionsTest : BaseExtensionsTest
{
    private readonly IFixture _fixture;

    public FieldExtensionsTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    internal void ToMeta_Field1_MetaValue()
    {
        // arrange 
        var id = _fixture.Create<int>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var tableId = _fixture.Create<int>();
        var fieldType = _fixture.Create<FieldType>();
        var defaultValue = _fixture.Create<string?>();
        var size = _fixture.Create<int>();
        var field = new Field(id, name, description, fieldType, size, defaultValue, true, true, true, true, false);

        // act 
        var meta = FieldExtensions.ToMeta(field, tableId);

        // assert
        Assert.Equal(meta.GetEntityId(), id);
        Assert.Equal(meta.GetEntityName(), name);
        Assert.Equal(meta.GetEntityDescription(), description);
        Assert.Equal(meta.GetFieldType(), fieldType);
        Assert.Equal(meta.GetFieldSize(), size);
        Assert.Equal(meta.GetEntityRefId(), tableId);
        Assert.True(meta.IsEntityBaseline());
        Assert.True(meta.IsFieldNotNull());
        Assert.True(meta.IsFieldMultilingual());
        Assert.False(meta.IsEntityActive());
    }

    [Fact]
    internal void ToMeta_Field2_MetaValue()
    {
        // arrange 
        var id = _fixture.Create<int>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var tableId = _fixture.Create<int>();
        var fieldType = _fixture.Create<FieldType>();
        var defaultValue = _fixture.Create<string?>();
        var size = _fixture.Create<int>();
        var field = new Field(id, name, description, fieldType, size, defaultValue, false, false, false, false, true);

        // act 
        var meta = FieldExtensions.ToMeta(field, tableId);

        // assert
        Assert.Equal(meta.GetEntityId(), id);
        Assert.Equal(meta.GetEntityName(), name);
        Assert.Equal(meta.GetEntityDescription(), description);
        Assert.Equal(meta.GetFieldType(), fieldType);
        Assert.Equal(meta.GetFieldSize(), size);
        Assert.Equal(meta.GetEntityRefId(), tableId);
        Assert.False(meta.IsEntityBaseline());
        Assert.False(meta.IsFieldNotNull());
        Assert.False(meta.IsFieldMultilingual());
        Assert.True(meta.IsEntityActive());
    }

}
