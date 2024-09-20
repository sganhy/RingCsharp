using AutoFixture;
using Ring.Util.Extensions;
using System.Dynamic;
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
