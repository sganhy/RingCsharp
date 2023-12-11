using Microsoft.VisualBasic;
using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Tests.Util.Extensions;

public class StringExtensionsTest
{
    
    public StringExtensionsTest()
    {
    }

    [Fact]
    public void Truncate_NullStriong_Null()
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

    [Theory]
    [InlineData(16,0, 4, true)]
    [InlineData(39,0, 4, false)]
    [InlineData(1, 1, 16, true)]
    [InlineData(2, 1, 17, true)]
    [InlineData(1, 2, 32, true)]
    [InlineData(1, 2, 499, false)]
    public void GetBitValue_BitPosition_Result(char charInput, int elementPosition,int bitPosition, bool expectedResult)
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
    public void SetBitValue_BitPosition_Result(int firstBitPosition, int secondBitPosition, int falseBitPosition)
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
    public void IsDigits_InputString_BoolResult(string input, bool expectedResult)
    {
        // arrange 
        // act 
        var result=StringExtensions.IsNumber(input);

        // assert
        Assert.Equal(expectedResult,result);
    }

    [Theory]
    [InlineData("-11.2", true)]
    [InlineData("555.154", true)]
    [InlineData("84354354.531531351354", true)]
    [InlineData("556165154^^^^$$^l5554", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("0", true)]
    [InlineData("0000000000000000000000000", true)]
    [InlineData("78,888787", false)]
    [InlineData("45.45.45", false)]
    public void IsFloat_InputString_BoolResult(string input, bool expectedResult)
    {
        // arrange 
        // act 
        var result = StringExtensions.IsFloat(input);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("-11.2", '.', "-112")]
    [InlineData("", '.', "")]
    [InlineData("-11,2", '.', "-11,2")]
    [InlineData("", ';', "")]
    [InlineData(null, '8', null)]
    [InlineData("#~#~#~1", '~', "###1")]
    [InlineData("11111", '1', "")]
    [InlineData("  1  ", ' ', "1")]
    public void StripChar_InputString_ReplacedString(string? inputString, char c, string expectedResult)
    {
        // arrange 
        // act 
        var result = StringExtensions.RemoveChar(inputString,c);
        // assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    //[InlineData("20211212", "20211212", "yyyyMMdd", null)]
    //[InlineData("2021-12-12", "20211212", "yyyyMMdd", null)]
    //[InlineData("2021-12", "20211201", "yyyyMMdd", null)]
    [InlineData("20211207T19:36:47", "20211207 19:36:47", "yyyyMMdd HH:mm:ss", null)]
    [InlineData("20211207T193647", "20211207 19:36:47", "yyyyMMdd HH:mm:ss", null)]
    public void ParseIso8601Date_InputString_DateTimeResult(string input, string expectedDate, string dateFormat ,int? offset)
    {
        // arrange 
        var expectedResult = DateTime.ParseExact(expectedDate, dateFormat, CultureInfo.InvariantCulture);

        // act 
        (var dateResult, var timespanResult) = StringExtensions.ParseIso8601Date(input);

        // assert
        Assert.Equal(expectedResult, dateResult);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("45", false)]
    [InlineData("bGlnaHQgd28=", true)]
    [InlineData("bGlnaHQgdw==", true)]
    [InlineData("aWwgw6l0YWl0IHVuZSBmb2lzIGxlcyBVU0E=", true)]
    [InlineData("bGln,HQgd28=", false)]
    public void IsBase64String_InputString_Result(string input, bool expectedResult)
    {
        // arrange 
        // act 
        var result = StringExtensions.IsBase64String(input);

        // assert
        Assert.Equal(expectedResult, result);
    }

}
