using Ring.Schema.Enums;
using Ring.Util.Extensions;

namespace Ring.Tests.Util.Extensions;
public  sealed class DateTimeExtensionsTest
{
    [Fact]
    public void ToString_ShortDateTime_22221222()
    {
        // arrange 
        var dt = new DateTime(2222, 12, 22, 23, 59, 59);

        // act 
        var result = DateTimeExtensions.ToString(dt, FieldType.ShortDateTime, null);

        // assert
        Assert.Equal("2222-12-22", result);
    }


    [Fact]
    public void ToString_DateTime_DateTimeNow()
    {
        // arrange 
        var dt = DateTime.Now;
        var expectedValue = dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff").Replace(' ', 'T') + "Z";

        // act 
        var result = DateTimeExtensions.ToString(dt, FieldType.DateTime, null);

        // assert
        Assert.Equal(expectedValue, result);
    }
}
