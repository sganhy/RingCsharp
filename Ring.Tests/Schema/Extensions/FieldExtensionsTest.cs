using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Tests.Schema.Extensions;

public class FieldExtensionsTest : BaseTest
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
        Assert.Equal(meta.Id, id);
        Assert.Equal(meta.Name, name);
        Assert.Equal(meta.Description, description);
        Assert.Equal(meta.GetFieldType(), fieldType);
        Assert.Equal(meta.GetFieldSize(), size);
        Assert.Equal(meta.ReferenceId, tableId);
        Assert.True(meta.IsEntityBaseline);
        Assert.True(meta.IsFieldNotNull());
        Assert.True(meta.IsFieldMultilingual());
        Assert.False(meta.Active);
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
        Assert.Equal(meta.Id, id);
        Assert.Equal(meta.Name, name);
        Assert.Equal(meta.Description, description);
        Assert.Equal(meta.GetFieldType(), fieldType);
        Assert.Equal(meta.GetFieldSize(), size);
        Assert.Equal(meta.ReferenceId, tableId);
        Assert.False(meta.IsEntityBaseline);
        Assert.False(meta.IsFieldNotNull());
        Assert.False(meta.IsFieldMultilingual());
        Assert.True(meta.Active);
    }


    [Fact]
    internal void GetHashCode_FieldHashEqual_True()
    {
        // arrange 
        var id = _fixture.Create<int>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var tableId = _fixture.Create<int>();
        var fieldType = _fixture.Create<FieldType>();
        var defaultValue = _fixture.Create<string?>();
        var size = _fixture.Create<int>();
        var field1 = new Field(id, name, description, fieldType, size, defaultValue, false, false, false, false, true);
        var field2 = new Field(id, name, description, fieldType, size, defaultValue, false, false, false, false, true);

        // act 
        var hash1 = FieldExtensions.GetHashCode(field1);
        var hash2 = FieldExtensions.GetHashCode(field2);

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_FieldHashEqual_False()
    {
        // arrange 
        var id = _fixture.Create<int>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var tableId = _fixture.Create<int>();
        var fieldType = _fixture.Create<FieldType>();
        var defaultValue = _fixture.Create<string?>();
        var size = _fixture.Create<int>();
        var field1 = new Field(id, name, description, fieldType, size, defaultValue, false, false, false, false, true);
        var field2 = new Field(id, name, description, fieldType, size, defaultValue, false, false, false, true, true);
        var field3 = new Field(id, name, description, fieldType, size, defaultValue, true, false, false, false, true);

        // act 
        var hash1 = FieldExtensions.GetHashCode(field1);
        var hash2 = FieldExtensions.GetHashCode(field2);
        var hash3 = FieldExtensions.GetHashCode(field3);

        // assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

}
