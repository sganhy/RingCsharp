using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Ring.Tests.Schema.Extensions;

public class FieldExtensionsTest : BaseTest
{
    public FieldExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    internal void ToMeta_Field1_MetaValue()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue,int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var tableId = _faker.Random.Number(0,int.MaxValue);
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool()? null : _faker.Random.String(); // nullable string
        var size = _faker.Random.Number(0,int.MaxValue);
        var field = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.None, true, true, true, false);

        // act 
        var meta = FieldExtensions.ToMeta(field, tableId);

        // assert
        Assert.Equal(meta.Id, id);
        Assert.Equal(meta.Name, name);
        Assert.Equal(meta.Description, description);
        Assert.Equal(meta.GetFieldType(), fieldType);
        Assert.Equal(meta.GetFieldSize(), size);
        Assert.Equal(meta.ReferenceId, tableId);
        Assert.True(meta.IsEntityBaseline());
        Assert.True(meta.IsFieldNotNull());
        Assert.True(meta.IsFieldMultilingual());
        Assert.False(meta.Active);
    }

    [Fact]
    internal void ToMeta_Field2_MetaValue()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue,int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var tableId = _faker.Random.Number(int.MinValue,int.MaxValue);
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool()? null: _faker.Random.String();
        var size = _faker.Random.Number(0,int.MaxValue);
        var field = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCaseAndDiacritics, false, false, false, true);

        // act 
        var meta = FieldExtensions.ToMeta(field, tableId);

        // assert
        Assert.Equal(meta.Id, id);
        Assert.Equal(meta.Name, name);
        Assert.Equal(meta.Description, description);
        Assert.Equal(meta.GetFieldType(), fieldType);
        Assert.Equal(meta.GetFieldSize(), size);
        Assert.Equal(meta.ReferenceId, tableId);
        Assert.False(meta.IsEntityBaseline());
        Assert.False(meta.IsFieldNotNull());
        Assert.False(meta.IsFieldMultilingual());
        Assert.True(meta.Active);
    }

    [Fact]
    internal void GetHashCode_FieldHashEqual_True()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue,int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool()? null: _faker.Random.String();
        var size = _faker.Random.Number(int.MinValue,int.MaxValue);
        var field1 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCaseAndDiacritics, false, false, false, true);
        var field2 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCaseAndDiacritics, false, false, false, true);

        // act 
        var hash1 = FieldExtensions.Hash(field1);
        var hash2 = FieldExtensions.Hash(field2);

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_FieldHashEqual_False()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue,int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool() ? null : _faker.Random.String();
        var size = _faker.Random.Number(int.MinValue,int.MaxValue);
        var field1 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, false, true);
        var field2 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, true, true);
        var field3 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCaseAndDiacritics, false, false, false, true);
        var field4 = new Field(id*-1, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, false, true);

        // act 
        var hash1 = FieldExtensions.Hash(field1);
        var hash2 = FieldExtensions.Hash(field2);
        var hash3 = FieldExtensions.Hash(field3);
        var hash4 = field4.GetHashCode();

        // assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
        Assert.NotEqual(hash1, hash4);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Français", "FRANCAIS")]
    [InlineData("mulțumesc", "MULTUMESC")]
    [InlineData("Zürich, Ökonom", "ZURICH, OKONOM")]
    [InlineData("Åke ȘŠш", "AKE SSШ")]
    internal void GetSearchableValue_IngoreCaseAndDiacritics(string? value, string? expectedValue)
    {
        // arrange 
        // act 
        var result = FieldExtensions.GetSearchableValue(null,SearchableType.IgnoreCaseAndDiacritics,value);

        // assert
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Français", "FRANÇAIS")]
    internal void GetSearchableValue_IngoreCase(string? value, string? expectedValue)
    {
        // arrange 
        // act 
        var result = FieldExtensions.GetSearchableValue(null, SearchableType.IgnoreCase, value);

        // assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    internal void IsEquivalentTo_AnonymousFields_False()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue, int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool() ? null : _faker.Random.String();
        var size = _faker.Random.Number(int.MinValue, int.MaxValue);
        var field1 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, false, true);
        var field2 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, true, true);
        var field3 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCaseAndDiacritics, false, false, false, true);
        var field4 = new Field(id * -1, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, false, true);

        // act 
        var result1 = field1 == field2;
        var result2 = FieldExtensions.IsEquivalentTo(field1, field2);
        var result3 = field2 == field3;
        var result4 = field1.Equals((object)field4);
        var result5 = FieldExtensions.IsEquivalentTo(field1, null);
        var result6 = field4.Equals(null);

        // assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.False(result4);
        Assert.False(result5);
        Assert.False(result6);
    }

    [Fact]
    internal void IsEquivalentTo_AnonymousField_True()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue, int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool() ? null : _faker.Random.String();
        var size = _faker.Random.Number(int.MinValue, int.MaxValue);
        var field1 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, false, true);
        var field2 = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, false, false, false, true);

        // act 
        var result = field1 != field2;
        

        // assert
        Assert.False(result);
    }


}
