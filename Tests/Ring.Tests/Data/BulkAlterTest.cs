using Ring.Data;
using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Data;
public sealed class BulkAlterTest : BaseTest
{
	private readonly DbSchema _schema;
	private readonly Table _table;

	public BulkAlterTest(ITestOutputHelper output) : base(output)
	{
		var metaList = GetSchema1();
		var meta = Meta.Create("Test");
		_schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql, Encoding.UTF8) ?? Meta.GetDefaultSchema(meta, DatabaseProvider.PostgreSql);
		_table = Meta.GetDefaultTable(meta);
	}

	[Fact]
	public void Sort_EmptyQueries_DoesNothing()
	{
		// arrange 
		using var bulk = new BulkAlter(_schema);

		// arrange 
		bulk.Sort();

		// assert
		bulk.Dispose();
		Assert.True(bulk.Disposed);
		Assert.Equal(0, bulk.Queries.Count);
	}

	[Fact]
	public void Sort_SingleQuery_DoesNothing()
	{
		// arrange 
		var list1 = new PooledList<AlterQuery>();
		var id = _faker.Random.Number(1, 100);
		var query = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateTable, -1, null, null, null);
		list1.Add(query);
		using var bulk = new BulkAlter(_schema, list1);

		// arrange 
		bulk.Sort();

		// assert
		Assert.Equal(1, bulk.Queries.Count);
	}

	[Fact]
	public void Sort_AlreadySorted_KeepsExactOrder()
	{
		// arrange 
		var list1 = new PooledList<AlterQuery>();
		for (var i = 0; i < 50; i++)
		{
			var id = _faker.Random.Number(1, 10000);
			var type = _faker.PickRandom<AlterQueryType>();
			var recordIndex = _faker.Random.Number(1, 100);
			var query = new AlterQuery(id, _schema.GetTable("armor") ?? _table, type, recordIndex, null, null, null);
			list1.Add(query);
		}
		list1.Sort((x, y) => (int)x.Type - (int)y.Type);
		var input = list1.ToArray();
		using var bulk = new BulkAlter(_schema, list1);

		// arrange 
		bulk.Sort();

		// assert
		Assert.Equal(input.Select(q => q.Id), bulk.Queries.ToArray().Select(q => q.Id));
	}

	[Fact]
	public void Sort_ReverseOrder_SortsByType()
	{
		// arrange 
		var list1 = new PooledList<AlterQuery>();
		for (var i = 0; i < 60; i++)
		{
			var id = _faker.Random.Number(1, 10000);
			var type = _faker.PickRandom<AlterQueryType>();
			var recordIndex = _faker.Random.Number(1, 100);
			var query = new AlterQuery(id, _schema.GetTable("armor") ?? _table, type, recordIndex, null, null, null);
			list1.Add(query);
		}
		list1.Sort((x, y) => ((int)y.Type).CompareTo((int)x.Type)); // Force reverse (descending) order initially
		using var bulk = new BulkAlter(_schema, list1);

		// act 
		bulk.Sort();

		// assert
		var types = bulk.Queries.ToArray().Select(q => (int)q.Type).ToArray();
		for (var i = 0; i < types.Length - 1; i++) Assert.True(types[i] <= types[i + 1]);
	}

	[Fact]
	public void Sort_SameTypeQueries_PreservesRelativeOrder()
	{
		// arrange 
		var list1 = new PooledList<AlterQuery>();
		var table = _schema.GetTable("skill") ?? _table;

		// Add multiple queries with the exact same Type, but distinct IDs to track order
		for (var i = 1; i <= 15; i++)
		{
			var query = new AlterQuery(id: i, table, AlterQueryType.CreateTable, -1, null, null, null);
			list1.Add(query);
		}

		using var bulk = new BulkAlter(_schema, list1);

		// act 
		bulk.Sort();

		// assert - since all have the same type, a stable sort must preserve original insertion order
		var sortedIds = bulk.Queries.ToArray().Select(q => q.Id).ToArray();
		for (var i = 0; i < sortedIds.Length; i++) Assert.Equal(i + 1, sortedIds[i]);
	}

	[Fact]
	internal void GetHashCode_BulkAlterHashEqual_False()
	{
		// arrange 
		var id = _faker.Random.Number(1, 100);
		// bulk1
		var list1 = new PooledList<AlterQuery>();
		var query1 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateTable, -1, null, null, null);
		var query2 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.AlterTableAddColumn, -1, null, null, null);
		var query3 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateDefaultConstraint, 1, null, null, null);
		list1.Add(query1);
		list1.Add(query2);
		list1.Add(query3);
		using var bulk1 = new BulkAlter(_schema, list1);

		// bulk2
		var query4 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateTable, -1, null, null, null);
		var query5 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.AlterTableAddColumn, -1, null, null, null);
		var query6 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateDefaultConstraint, 2, null, null, null);
		var list2 = new PooledList<AlterQuery>();
		list2.Add(query4);
		list2.Add(query5);
		list2.Add(query6);
		using var bulk2 = new BulkAlter(_schema, list2);

		// bulk3
		var query7 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateTable, -1, null, null, null);
		var query8 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.AlterTableAddColumn, -1, null, null, null);
		var query9 = new AlterQuery(id, _schema.GetTable("armor") ?? _table, AlterQueryType.CreateDefaultConstraint, 3, null, null, null);
		var list3 = new PooledList<AlterQuery>();
		list3.Add(query7);
		list3.Add(query8);
		list3.Add(query9);
		using var bulk3 = new BulkAlter(_schema, list3);

		// act 
		var hash1 = BulkAlterExtensions.Hash(bulk1);
		var hash2 = bulk2.GetHashCode();
		var hash3 = bulk3.GetHashCode();

		// assert
		Assert.NotEqual(hash1, hash2);
		Assert.NotEqual(hash1, hash3);
		Assert.NotEqual(hash2, hash3);
	}

}
