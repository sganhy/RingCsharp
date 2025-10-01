using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Ring.Tests.Schema.Extensions;

public sealed class BaseEntityExtensionsTest : BaseTest
{
    public BaseEntityExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    internal void BaseEntityEquals_AnonymousBaseLineWithNull_False()
    {
        // arrange 
        var id = _faker.Random.Int();
        var meta = new Meta(id, (byte)EntityType.Field, 1011, 16, 10493964 + 16, _faker.Random.String(), _faker.Random.String(), null, true);
        var field = meta.ToField() ?? Meta.GetDefaultField(meta, FieldType.Int);
        var baseEntity = (BaseEntity)field;

        // act 
        var result = BaseEntityExtensions.BaseEntityEquals(baseEntity, null);

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void BaseEntityEquals_2IndenticalBaseEntity_True()
    {
        // arrange 
        var id = _faker.Random.Int();
        var meta1 = new Meta(id, (byte)EntityType.Field, 1011, 16, 10493964 + 16, _faker.Random.String(), _faker.Random.String(), null, true);
        var field1 = meta1.ToField() ?? Meta.GetDefaultField(meta1, FieldType.Int);
        var baseEntity1 = (BaseEntity)field1;
        var meta2 = new Meta(id, (byte)EntityType.Field, 111111111, 111111111, 10493964 + 32, meta1.Name, meta1.Description, null, true);
        var field2 = meta2.ToField() ?? Meta.GetDefaultField(meta1, FieldType.Int);
        var baseEntity2 = (BaseEntity)field2;

        // act 
        var result = BaseEntityExtensions.BaseEntityEquals(baseEntity1, baseEntity2);

        // assert
        Assert.True(result);
    }

}
