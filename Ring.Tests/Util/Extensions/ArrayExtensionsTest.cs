using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Tests.Util.Extensions;

public class ArrayExtensionsTest
{
    [Fact]
    public void GetIndex_007String_222()
    {
        // arrange 
        var array = new string[255];
        for (var i = 0; i < array.Length; ++i) array[i] = "00" + i.ToString(CultureInfo.InvariantCulture);
        Array.Sort(array, (x, y) => string.CompareOrdinal(x, y));
        const string expectedResult = "007";

        // act 
#pragma warning disable RCS1196 // Call extension method as instance method
        var result = ArrayExtensions.GetIndex(array, expectedResult);
#pragma warning restore RCS1196

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
        const string expectedResult = "Test";

        // act 
#pragma warning disable RCS1196 // Call extension method as instance method
        var result = ArrayExtensions.GetIndex(array, expectedResult);
#pragma warning restore RCS1196

        // assert
        Assert.Equal(-1, result);
    }
}
