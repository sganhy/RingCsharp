using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Ring.Tests.Schema.Extensions;

public sealed class ColumnExtensionsTest : BaseTest
{
    public ColumnExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    internal void GetHashCode_IndexHashEqual_False()
    {
        // arrange 
        var id = _faker.Random.Number(int.MinValue, int.MaxValue);
        var physicalName = _faker.Random.String();
        var column1 = new Column(EntityType.Alias, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics,id,-33);
        var column2 = new Column(EntityType.Constraint, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics, id, -33);
        var column3 = new Column(EntityType.Alias, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics, id, -44);

        // act 
        var hash1 = ColumnExtensions.Hash(column1);
        var hash2 = column2.GetHashCode();
        var hash3 = column3.GetHashCode();

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
        var physicalName = _faker.Random.String();
        var column1 = new Column(EntityType.Alias, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics, id, -33);
        var column2 = new Column(EntityType.Constraint, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics, id, -33);
        var column3 = new Column(EntityType.Alias, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics, id, -44);
        var column4 = new Column(EntityType.Constraint, FieldType.ByteArray, physicalName, SearchableType.IgnoreCaseAndDiacritics, id, -33);

        // act 
        var result1 = column1 == column2;
        var result2 = column1.Equals(column3);
        var result3 = column2.Equals((object)column3);
        var result4 = column2 != column4;

        // assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.False(result4);
    }
}
