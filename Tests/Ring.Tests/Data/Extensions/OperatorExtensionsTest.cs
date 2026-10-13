using Ring.Data;
using Ring.Data.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Linq.Expressions;

namespace Ring.Tests.Data.Extensions;

public sealed class OperatorExtensionsTest : BaseTest
{
	public OperatorExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Theory]
    [InlineData(Operator.Equal,"=", DatabaseProvider.PostgreSql)]
    [InlineData(Operator.NotEqual, "<>", DatabaseProvider.PostgreSql)]
    [InlineData(Operator.NotEqual, "!=", DatabaseProvider.Oracle)]
    [InlineData(Operator.Like, " like ", DatabaseProvider.Oracle)]
    internal void ToSql_InputTestValue_OnlyOneTrueFlag(Operator operatorType, string expectedValue, 
        DatabaseProvider databaseProvider)
    {
        // arrange 
        // act 
        var result = OperatorExtensions.ToSql(operatorType, databaseProvider, "TEST");

        // assert
        Assert.Equal(result, expectedValue,true);
    }

	[Fact]
	internal void GetHashCode_OperationalErrorHashEqual_False()
	{
		// arrange 
		var message = _faker.Random.String();
		var tableName = _faker.Random.String();
		var error1 = new OperationalError(OperationalErrorType.ConnectionClosed, message, "SQLSTATE1", "Severity", "Detail", "Hint", tableName, "SchemaName");
		var error2 = new OperationalError(OperationalErrorType.InvalidProtocol, message, "SQLSTATE1", "Severity", "Detail", "Hint", tableName, "SchemaName");
		var error3 = new OperationalError(OperationalErrorType.ConnectionClosed, message, "SQLSTATE2", "Severity", "Detail", "Hint", tableName, "SchemaName");

		// act 
		var hash1 = OperationalErrorExtensions.Hash(error1);
		var hash2 = error2.GetHashCode();
		var hash3 = error3.GetHashCode();

		// assert
		Assert.NotEqual(hash1, hash2);
		Assert.NotEqual(hash1, hash3);
		Assert.NotEqual(hash2, hash3);
	}

	[Fact]
	internal void Equals_2AnonymousOperationalErrors_False()
	{
		// arrange 
		var message = _faker.Random.String();
		var schemaName = _faker.Random.String();
		var error1 = new OperationalError(OperationalErrorType.ConnectionClosed, message, "SQLSTATE1", "Severity1", "Detail1", "Hint1", "TableName1", schemaName);
		var error2 = new OperationalError(OperationalErrorType.InvalidProtocol, message, "SQLSTATE2", "Severity2", "Detail2", "Hint2", "TableName2", schemaName);
		var error3 = new OperationalError(OperationalErrorType.ConnectionClosed, message, "SQLSTATE3", "Severity3", "Detail3", "Hint3", "TableName3", schemaName);
		var error4 = new OperationalError(OperationalErrorType.InvalidProtocol, message, "SQLSTATE2", "Severity2", "Detail2", "Hint2", "TableName2", schemaName);
		var error5 = new OperationalError(OperationalErrorType.ConnectionClosed, message, "SQLSTATE5", "Severity5", "Detail5", "Hint5", "TableName5", schemaName);

		// act 
		var result1 = error1 == error2;
		var result2 = error1.Equals(error3);
		var result3 = error2.Equals((object)error3);
		var result4 = error2 != error4;
		var result5 = error4.IsEquivalentTo(error5);

		// assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
		Assert.False(result5);
	}



}
