using AutoFixture;
using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Tests.Util.Extensions;

public class ArrayExtensionsTest
{
    private readonly IFixture _fixture; 
    
    public ArrayExtensionsTest() => _fixture = new Fixture();

    [Fact]
    public void Exists_TestString_True()
    {
        // arrange 
        var array = _fixture.CreateMany<string>(512).ToArray();
        array[0] = "Test";
        Array.Sort(array, (x, y) => string.CompareOrdinal(x, y));

        // act 
        var result = ArrayExtensions.Exists(array,"Test");

        // assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_TestString_False()
    {
        // arrange 
        var array = new string[512];
        for (var i=0; i< array.Length;++i) array[i] = "00" + i.ToString(CultureInfo.InvariantCulture);
        Array.Sort(array, (x, y) => string.CompareOrdinal(x, y));

        // act 
        var result = ArrayExtensions.Exists(array, "Test");

        // assert
        Assert.False(result);
    }


}
