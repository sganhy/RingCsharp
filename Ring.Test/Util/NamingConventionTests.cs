using Xunit;
using Ring.Util;

namespace Ring.UnitTests.Util
{
	public class NamingConventionTests
	{

		[Theory]
		[InlineData(null, null)]
		[InlineData("", "")]
        [InlineData("HelloWorld", "hello_world")]
		[InlineData("hello_world", "hello_world")]
		[InlineData("HellOWorld", "hell_o_world")]
		[InlineData("Hello World", "hello_world")]
		public void SnakeCaseTest(string name, string expectedResult)
		{
		    var result = NamingConvention.ToSnakeCase(name);
            Assert.Equal(result, expectedResult);
        }
	}
}
