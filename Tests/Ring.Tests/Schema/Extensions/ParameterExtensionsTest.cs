using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Data;
using System.Linq.Expressions;

namespace Ring.Tests.Schema.Extensions;

public class ParameterExtensionsTest : BaseTest
{

	public ParameterExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	internal void GetHashCode_ParameterHashEqual_False()
	{
		// arrange 
		var id = _faker.Random.Number(int.MinValue, int.MaxValue);
		var name = _faker.Random.String();
		var description = _faker.Random.String();
		
		var param1 = new Parameter(id, name, description,ParameterType.SchemaVersion,FieldType.Int, null, EntityType.Schema, 1, true, true);
		var param2 = new Parameter(id, name, description, ParameterType.MinPoolSize, FieldType.Int, null, EntityType.Schema, 1, false, true);
		var param3 = new Parameter(id, name, description, ParameterType.SchemaVersion, FieldType.Long, null, EntityType.Schema, 1, true, true);

		// act 
		var hash1 = ParameterExtensions.Hash(param1);
		var hash2 = param2.GetHashCode();
		var hash3 = param3.GetHashCode();

		// assert
		Assert.NotEqual(hash1, hash2);
		Assert.NotEqual(hash1, hash3);
		Assert.NotEqual(hash2, hash3);
	}

	[Fact]
	internal void Equals_2AnonymousParameters_False()
	{
		// arrange 
		var id = _faker.Random.Number(int.MinValue, int.MaxValue);
		var name = _faker.Random.String();
		var description = _faker.Random.String();

		var colCount1 = 5;
		var colCount2 = 7;
		var columns = new List<Column>(colCount1);
		for (var i = 0; i < colCount1; ++i) columns.Add(GetAnonymousColumn());
		var columns2 = new List<Column>(colCount2);
		for (var i = 0; i < colCount2; ++i) columns2.Add(GetAnonymousColumn());
		var param1 = new Parameter(id, name, description, ParameterType.SchemaVersion, FieldType.Int, null, EntityType.Schema, 1, true, true);
		var param2 = new Parameter(id, name, description, ParameterType.MinPoolSize, FieldType.Int, null, EntityType.Schema, 1, false, true);
		var param3 = new Parameter(id, name, description, ParameterType.SchemaVersion, FieldType.Long, null, EntityType.Schema, 1, true, true);
		var param4 = new Parameter(id, name, description, ParameterType.MinPoolSize, FieldType.Int, null, EntityType.Schema, 1, false, true);
		var param5 = new Parameter(id, name, description, ParameterType.SchemaVersion, FieldType.ByteArray, null, EntityType.Schema, 1, false, true);

		// act 
		var result1 = param1 == param2;
		var result2 = param1.Equals(param3);
		var result3 = ParameterExtensions.IsEquivalentTo(param2, param3);
		var result4 = param2 != param4;
		var result5 = param2.Equals((object)param3);

		// assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
		Assert.False(result5);
	}

}
