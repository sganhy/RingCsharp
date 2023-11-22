using Ring.Util.Extensions;

namespace Ring.Tests.Util.Extensions;

public class StringExtensionsTest
{
    
    public StringExtensionsTest()
    {
    }

    [Fact]
    internal void Truncate_NullStriong_Null()
    {
        // arrange 
        // act 
        var result = StringExtensions.Truncate(null,250);

        // assert
        Assert.Null(result);
    }

    [Fact]
    internal void Truncate_TestStriong_TestString()
    {
        // arrange 
        var input = "Test";
            
        // act 
        var result = StringExtensions.Truncate(input, 250);

        // assert
        Assert.Equal(input, result);
    }

    [Fact]
    internal void Truncate_TestStriong_TruncTestString()
    {
        // arrange 
        var input = "Test22";

        // act 
        var result = StringExtensions.Truncate(input, 3);

        // assert
        Assert.Equal("Tes", result);
    }

}
