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
    [InlineData(16,0, 4, true)]
    [InlineData(39,0, 4, false)]
    [InlineData(1, 1, 16, true)]
    [InlineData(2, 1, 17, true)]
    [InlineData(1, 2, 32, true)]
    [InlineData(1, 2, 499, false)]
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

    [Theory]
    [InlineData(8, 9, 1)]
    [InlineData(27, 31, 19)]
    [InlineData(27, 31, 1)]
    [InlineData(0, 31, 1)]
    [InlineData(97, 101, 55)]
    internal void SetBitValue_BitPosition_Result(int firstBitPosition, int secondBitPosition, int falseBitPosition)
    {
        // arrange 
        var chars = new char[10];
        var input = new string(chars);

        // act 
        StringExtensions.SetBitValue(input, firstBitPosition);
        StringExtensions.SetBitValue(input, secondBitPosition);
        var result1 = StringExtensions.GetBitValue(input, firstBitPosition);
        var result2 = StringExtensions.GetBitValue(input, secondBitPosition);
        var result3 = StringExtensions.GetBitValue(input, falseBitPosition);

        // assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);
    }


    [Theory]
    [InlineData("555154", true)]
    [InlineData("84354354531531351354", true)]
    [InlineData("556165154^^^^$$^l5554", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("0", true)]
    [InlineData("0000000000000000000000000", true)]
    [InlineData("じゅういち", false)]
    [InlineData("-112", true)]
    [InlineData("-", false)]
    internal void IsDigits_InputString_BoolResult(string input, bool expectedResult)
    {
        // arrange 
        // act 
        var result=StringExtensions.IsNumber(input);

        // assert
        Assert.Equal(expectedResult,result);
    }

    

}
