using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Extensions;
using System.Linq.Expressions;
using Xunit;

namespace Ring.PostgreSQL.Tests.Extensions;

public sealed class IntExtensionsTest : BaseTest
{
	public IntExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	public void ToAuthenticationType_AllExistingEnumId_Enum()
	{
		// arrange 
		var relationTypes = Enum.GetValues<AuthenticationType>();
		foreach (var relType in relationTypes)
		{
			// act 
			var relationTypeResult = IntExtensions.ToAuthenticationType((int)relType);
			// assert 
			Assert.Equal(relType, relationTypeResult);
		}
	}
}
