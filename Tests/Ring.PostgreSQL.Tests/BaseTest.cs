using Bogus;
using Xunit;

namespace Ring.PostgreSQL.Tests;

public abstract class BaseTest
{
	protected readonly Faker _faker = new();
	private readonly ITestOutputHelper _output;
	protected BaseTest(ITestOutputHelper output) =>_output = output;
	protected void LogAssert(string message) => _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "| ASSERT  | " + message);
	protected void LogAct(string message) => _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|   ACT   | " + message);
	protected void LogArrange(string message) => _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "| ARRANGE | " + message);

}
