using Bogus.DataSets;
using Xunit.Abstractions;
using Ring.Schema.Models;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Linq.Expressions;

namespace Ring.Tests.Schema.Extensions;

public sealed class BaseEntityExtensionsTest : BaseTest
{
    public BaseEntityExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    internal void GetStringCode_AnonymousInput_StringCode()
    {
        // arrange 
        const char sep = (char)8998;
        var id = _faker.Random.Number(int.MinValue, int.MaxValue);
        var name = _faker.Random.String();
        var description = _faker.Random.String();
        var fieldType = _faker.PickRandom<FieldType>();
        var defaultValue = _faker.Random.Bool() ? null : _faker.Random.String(); // nullable string
        var size = _faker.Random.Number(0, int.MaxValue);
        var field = new Field(id, name, description, fieldType, size, defaultValue, SearchableType.IgnoreCase, true, true, true, false);
        var expectedValue = $"{field.Active}{sep}{field.Baseline}{sep}{field.Description}{sep}{field.Id}{sep}{field.Name}";

        // act 
        var result = BaseEntityExtensions.GetStringCode(field).ToString();

        // assert
        Assert.Equal(expectedValue, result);
    }

}
