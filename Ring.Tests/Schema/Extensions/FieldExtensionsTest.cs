using Bogus;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Tests.Schema.Extensions;

public class FieldExtensionsTest : BaseTest
{
    private readonly Faker _faker = new();

    [Fact]
    internal void ToMeta_Field1_MetaValue()
    {
        // arrange 
        var id = _faker.Random.Number();
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var tableId = _faker.Random.Number();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool()? null : _faker.Random.String(); // nullable string
        var size = _faker.Random.Number();
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
        var id = _faker.Random.Number();
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var tableId = _faker.Random.Number();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool()? null: _faker.Random.String();
        var size = _faker.Random.Number();
        var field = new Field(id, name, description, fieldType, size, defaultValue, false, false, false, false, true);

        // act 
#pragma warning disable RCS1196 // Call extension method as instance method
        var meta = FieldExtensions.ToMeta(field, tableId);
#pragma warning restore RCS1196

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
        var id = _faker.Random.Number();
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool()? null: _faker.Random.String();
        var size = _faker.Random.Number();
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
        var id = _faker.Random.Number();
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool() ? null : _faker.Random.String();
        var size = _faker.Random.Number();
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
