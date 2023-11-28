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

    [Theory]
    [InlineData(16,0, 5, true)]
    [InlineData(39,0, 5, false)]
    internal void GetBitValue_BitPosition_Result(char charInput, int elementPosition,int bitPosition, bool expectedResult)
    {
        // arrange 
        var chars = new char[10];
        chars[elementPosition] = charInput;
        var input = new string(chars);

        // act 
        var result = StringExtensions.GetBitValue(input, bitPosition);

        // assert
        Assert.Equal(expectedResult, result);
    }


}
