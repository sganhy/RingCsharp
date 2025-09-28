using Bogus;
using Ring.Util.Extensions;

namespace Ring.Tests.Util.Extensions;

public sealed class SpanExtensionsTest
{
    private readonly Faker _faker = new();

    [Fact]
    public void GetIndex_RandomIntegerArrayFind259_Ok()
    {
        // arrange
        var capacity = 55;
        var value = 259;
        var lst = Enumerable.Range(0, capacity).Select(r => _faker.Random.Int(-200, 200)).ToList();
        lst.Add(259);
        var arr = lst.ToArray();
        Array.Sort(arr); 

        // act 
        var index = SpanExtensions.GetIndex(new Span<int>(arr), value); // last index

        // assert
        Assert.Equal(arr[index], value);
    }

    [Fact]
    public void GetIndex_RandomIntegerArrayFind259_Ko()
    {
        // arrange
        var capacity = 45;
        var value = 259;
        var arr = Enumerable.Range(0, capacity).Select(r => _faker.Random.Int(-155, 155)).ToArray();
        Array.Sort(arr);

        // act 
        var index = SpanExtensions.GetIndex(new Span<int>(arr), value);

        // assert
        Assert.Equal(-1, index);
    }

}
