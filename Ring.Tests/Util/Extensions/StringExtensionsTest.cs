using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Tests.Util.Extensions;

public class StringExtensionsTest
{

    [Fact]
    public void Truncate_NullString_Null()
    {
        // arrange 
        // act 
        var result = StringExtensions.Truncate(null,250);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public void Truncate_TestStriong_TestString()
    {
        // arrange 
        var input = "Test";
            
        // act 
        var result = StringExtensions.Truncate(input, 250);

        // assert
        Assert.Equal(input, result);
    }

    [Fact]
    public void Truncate_TestStriong_TruncTestString()
    {
        // arrange 
        var input = "Test22";

        // act 
        var result = StringExtensions.Truncate(input, 3);

        // assert
        Assert.Equal("Tes", result);
    }
       
}
