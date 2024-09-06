namespace Ring.Tests.Util;

public class NamingConvention
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("hello_world", "hello_world")]
    [InlineData("HellOWorld", "hell_o_world")]
    [InlineData("Hello World", "hello_world")]
    [InlineData("@Meta", "@meta")]
    [InlineData("@", "@")]
    public void SnakeCaseTest(string? name, string? expectedResult)
    {
        var result = Ring.Util.NamingConvention.ToSnakeCase(name);
        Assert.Equal(result, expectedResult);
    }
}
