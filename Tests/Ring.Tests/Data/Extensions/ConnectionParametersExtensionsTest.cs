using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Enums;
using System.Linq.Expressions;

namespace Ring.Tests.Data.Extensions;

public sealed class ConnectionParametersExtensionsTest : BaseTest
{
	public ConnectionParametersExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	internal void GetHashCode_ConnectionParametersHashEqual_False()
	{
		// arrange 
		var host = _faker.Random.String();
		var port = _faker.Random.Number(1, 65535);
		var databaseName1 = "1";
		var databaseName2 = "2";
		var userName = _faker.Random.String();
		var applicationName = _faker.Random.String();

		// DatabaseProvider databaseProvider, string host, string databaseName, int port, string userName, string password, int timeOut, string applicationName, string clientEncoding, int sqlSendBufferSize
		var parameters1 = new ConnectionParameters(DatabaseProvider.SqlServer, host, databaseName1, port, userName, _faker.Random.String(), 
			_faker.Random.Number(1, 65535), applicationName, _faker.Random.String(), _faker.Random.Number(1, 65535));
		var parameters2 = new ConnectionParameters(DatabaseProvider.Oracle, host, databaseName2, port, userName, _faker.Random.String(), 
			_faker.Random.Number(1, 65535), applicationName, _faker.Random.String(), _faker.Random.Number(1, 65535));
		
		// act 
		var hash1 = ConnectionParametersExtensions.Hash(parameters1);
		var hash2 = ConnectionParametersExtensions.Hash(parameters2);

		// assert	
		Assert.NotEqual(hash1, hash2);
	}

	[Fact]
	internal void GetHashCode_ConnectionParametersHashEqual_True()
	{
		// arrange 
		var host = _faker.Random.String();
		var port = _faker.Random.Number(1, 65535);
		var databaseName1 = "DB_TEST";
		var databaseName2 = "DB_TEST";
		var userName = _faker.Random.String();
		var applicationName = _faker.Random.String();

		// DatabaseProvider databaseProvider, string host, string databaseName, int port, string userName, string password, int timeOut, string applicationName, string clientEncoding, int sqlSendBufferSize
		var parameters1 = new ConnectionParameters(DatabaseProvider.SqlServer, host, databaseName1, port, userName, _faker.Random.String(),
			_faker.Random.Number(1, 65535), applicationName, _faker.Random.String(), _faker.Random.Number(1, 65535));
		var parameters2 = new ConnectionParameters(DatabaseProvider.Oracle, host, databaseName2, port, userName, _faker.Random.String(),
			_faker.Random.Number(1, 65535), applicationName, _faker.Random.String(), _faker.Random.Number(1, 65535));

		// act 
		var hash1 = parameters1.GetHashCode();
		var hash2 = parameters2.GetHashCode();

		// assert	
		Assert.Equal(hash1, hash2);
	}

}
