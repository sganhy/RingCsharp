using System.Globalization;
using Xunit.Abstractions;
using Ring.Util.Extensions;
using System.Linq.Expressions;

namespace Ring.Tests.Util.Extensions;

public class StringExtensionsTest : BaseTest
{
    public StringExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    public void Truncate_NullString_Null()
    {
        // arrange 
        // act 
        LogAct("result <-- StringExtensions.Truncate(null,250)");
        var result = StringExtensions.Truncate(null,250);

        // assert
        LogAssert("result is null");
        Assert.Null(result);
    }

    [Fact]
    public void Truncate_TestString_TestString()
    {
        // arrange 
        LogArrange("input <-- 'Test'");
        var input = "Test";

        // act 
        LogAct("result <-- StringExtensions.Truncate(input, 250)");
        var result = StringExtensions.Truncate(input, 250);

        // assert
        LogAssert($"result == 'Test'");
        Assert.Equal(input, result);
    }

    [Fact]
    public void Truncate_TestString_TruncTestString()
    {
        // arrange 
        LogArrange("input <-- 'Test22'");
        var input = "Test22";

        // act 
        LogAct("result <-- StringExtensions.Truncate(input, 3)");
        var result = StringExtensions.Truncate(input, 3);

        // assert
        LogAssert($"result == 'Tes'");
        Assert.Equal("Tes", result);
    }
       
}
