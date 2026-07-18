using Ring.Data;
using Ring.Data.Extensions;
using Ring.Schema.Enums;
using Ring.Tests.MockUps;
using System.Linq.Expressions;

namespace Ring.Tests.Data.Extensions;

public sealed class IConnectionExtensionsTest: BaseTest
{

	public IConnectionExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	internal void GetId_RandomConnectionString_DifferentIds()
	{
		// arrange 
		var connectionString1 = 1;
		var connectionString2 = 2;
		IConnection conn1 = new ConnectionMock(1,DatabaseProvider.Oracle, connectionString1.ToString());
		IConnection conn2 = new ConnectionMock(1,DatabaseProvider.MySql, connectionString2.ToString());

		// act 
		var result1 = IConnectionExtensions.GetId(conn1, connectionString1); // 1 
		var	result2 = IConnectionExtensions.GetId(conn2, connectionString2); // 2 
		var result3 = IConnectionExtensions.GetId(conn1, connectionString1); // 2 
		var result4 = IConnectionExtensions.GetId(conn1, connectionString1); // 3
		var result5 = IConnectionExtensions.GetId(conn2, connectionString2); // 2 

		// assert
		Assert.Equal(1L, result1);
		Assert.Equal(1L, result2);
		Assert.Equal(2L, result3);
		Assert.Equal(3L, result4);
		Assert.Equal(2L, result5);
	}

}
