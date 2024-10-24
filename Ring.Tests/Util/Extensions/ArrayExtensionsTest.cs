using AutoFixture;
using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Tests.Util.Extensions;

public class ArrayExtensionsTest
{
    private readonly IFixture _fixture; 
    
    public ArrayExtensionsTest() => _fixture = new Fixture();

    [Fact]
    public void GetIndex_007String_222()
    {
        // arrange 
        var array = new string[255];
        for (var i = 0; i < array.Length; ++i) array[i] = "00" + i.ToString(CultureInfo.InvariantCulture);
        Array.Sort(array, (x, y) => string.CompareOrdinal(x, y));
        var expectedResult = "007";

        // act 
        var result = ArrayExtensions.GetIndex(array, expectedResult);

        // assert
        Assert.Equal(expectedResult, array[result]);
    }

    [Fact]
    public void GetIndex_TestString_MinusOne()
    {
        // arrange 
        var array = new string[128];
        for (var i = 0; i < array.Length; ++i) array[i] = "00" + i.ToString(CultureInfo.InvariantCulture);
        Array.Sort(array, (x, y) => string.CompareOrdinal(x, y));
        var expectedResult = "Test";

        // act 
        var result = ArrayExtensions.GetIndex(array, expectedResult);

        // assert
        Assert.Equal(-1, result);
    }
}
