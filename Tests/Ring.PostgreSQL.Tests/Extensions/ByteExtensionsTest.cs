using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Extensions;
using System.Linq.Expressions;
using Xunit;

namespace Ring.PostgreSQL.Tests.Extensions;

public sealed class ByteExtensionsTest : BaseTest
{
	public ByteExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	public void ToBackendMessageCode_AllExistingEnumId_Enum()
	{
		// arrange 
		var backendMessageCode = Enum.GetValues<BackendMessageCode>();
		foreach (var messageCode in backendMessageCode)
		{
			// act 
			var relationTypeResult = ByteExtensions.ToBackendMessageCode((byte)messageCode);
			// assert 
			Assert.Equal(messageCode, relationTypeResult);
		}
	}
}
