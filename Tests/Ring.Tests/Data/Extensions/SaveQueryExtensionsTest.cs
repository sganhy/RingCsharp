using Ring.Data;
using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using System.Linq.Expressions;

namespace Ring.Tests.Data.Extensions;

public sealed class SaveQueryExtensionsTest : BaseTest
{
	public SaveQueryExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	internal void GetHashCode_SaveQueryHashEqual_False()
	{
		// arrange 
		var tableName = _faker.Random.String();
		var tablebuilder = new TableBuilder();
		var metaTable = tablebuilder.GetMeta(tableName, DatabaseProvider.SqlServer);
		var data1 = new string?[] { "value1", "value2" };
		var data2 = Array.Empty<string?>();
		var data3 = new string?[] { "value1" };

		//Table table, SaveQueryType type, string?[] data, int offset
		var saveQuery1 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data1, 0);
		var saveQuery2 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data2, 0);
		var saveQuery3 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data3, 0);

		// act 
		var hash1 = SaveQueryExtensions.Hash(saveQuery1);
		var hash2 = saveQuery2.GetHashCode();
		var hash3 = saveQuery3.GetHashCode();

		// assert
		Assert.NotEqual(hash1, hash2);
		Assert.NotEqual(hash1, hash3);
		Assert.NotEqual(hash2, hash3);
	}

	[Fact]
	internal void Equals_2AnonymousOperationalErrors_False()
	{
		// arrange 
		var tableName = _faker.Random.String();
		var tablebuilder = new TableBuilder();
		var metaTable = tablebuilder.GetMeta(tableName, DatabaseProvider.SqlServer);
		var data1 = new string?[] { "value1", "value2" };
		var data2 = Array.Empty<string?>();
		var data3 = new string?[] { "value1" };

		//Table table, SaveQueryType type, string?[] data, int offset
		var saveQuery1 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data1, 0);
		var saveQuery2 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data2, 0);
		var saveQuery3 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data3, 0);
		var saveQuery4 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data2, 0);
		var saveQuery5 = new SaveQuery(metaTable, SaveQueryType.InsertRecord, data2, 2);

		// act 
		var result1 = saveQuery1 == saveQuery2;
		var result2 = saveQuery1.Equals(saveQuery3);
		var result3 = saveQuery2.Equals((object)saveQuery3);
		var result4 = saveQuery2 != saveQuery4;
		var result5 = saveQuery4.IsEquivalentTo(saveQuery5);

		// assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
		Assert.False(result5);
	}


}
