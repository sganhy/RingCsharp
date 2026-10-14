using Ring.Data;
using Ring.Tests.Models;
using System.Linq.Expressions;

namespace Ring.Tests.Data;

public sealed class PooledListTest : BaseTest
{
	public PooledListTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	public void Ctor_PositiveCapacity_RentsAtLeastRequestedCapacity()
	{
		// arrange 
		// act 
		var list = new PooledList<int>(17);

		// assert
		Assert.True(list.Capacity >= 17);
		Assert.Equal(0, list.Count);
		list.Dispose();
	}

	[Fact]
	public void RemoveAt_StructWithReferenceField_None()
	{
		// arrange 
		var list = new PooledList<PoolItem>(4);
		list.Add(new PoolItem { Id = 1, Name = "a" });
		list.Add(new PoolItem { Id = 2, Name = "b" });

		// act 
		list.RemoveAt(0);

		// assert
		Assert.Equal(1, list.Count);
		Assert.Equal("b", list[0].Name);
		list.Dispose();
	}

	[Fact]
	public void RemoveAt_MiddleIndex_ShiftsFollowingItems()
	{
		// arrange 
		var list = new PooledList<int>(8);
		for (var i = 0; i < 5; i++) list.Add(i);
		int[] expected = new[] { 0, 1, 3, 4 };

		// act 
		list.RemoveAt(2);

		// assert
		Assert.Equal(expected, list.ToArray());
		list.Dispose();
	}

	[Fact]
	public void RemoveAt_LastThenFirstIndex_RemovesBothItems()
	{
		// arrange 
		var list = new PooledList<int>(8);
		for (var i = 0; i < 3; i++) list.Add(i);
		int[] expected = new [] { 1 };

		// act 
		list.RemoveAt(2);
		list.RemoveAt(0);

		// assert
		
		Assert.Equal(expected, list.ToArray());
		list.Dispose();
	}

	[Fact]
	public void GetEnumerator_ForeachWithRef_EnumeratesInOrderAndAllowsModification()
	{
		// arrange 
		var list = new PooledList<PoolItem>(4);
		int[] expected = new int[] { 10, 11, 12 };
		for (var i = 0; i < 3; i++) list.Add(new PoolItem { Id = i });

		// act 
		foreach (ref var item in list) item.Id += 10;
		var ids = new List<int>();
		foreach (var item in list) ids.Add(item.Id);

		// assert
		Assert.Equal(expected, ids);
		list.Dispose();
	}

	[Fact]
	public void GetEnumerator_EmptyList_DoesNotIterate()
	{
		// arrange 
		var list = default(PooledList<PoolItem>);
		var iterations = 0;

		// act 
		foreach (var _ in list) iterations++;

		// assert
		Assert.Equal(0, iterations);
	}

	[Fact]
	public void ToArray_ListWithItems_ReturnsIndependentCopy()
	{
		// arrange 
		var list = new PooledList<int>(4);
		var int1 = _faker.Random.Int();
		var int2 = _faker.Random.Int();
		list.Add(int1);
		list.Add(int2);

		// act 
		var array = list.ToArray();
		list[0] = 99;

		// assert
		Assert.Equal(new[] { int1, int2 }, array);
		list.Dispose();
	}

	[Fact]
	public void Clear_ListWithItems_ResetsCountAndKeepsCapacity()
	{
		// arrange
		var list = new PooledList<int>(8);
		var int1 = _faker.Random.Int();
		var int2 = _faker.Random.Int();
		var int3 = _faker.Random.Int();
		list.Add(int1);
		list.Add(int2);
		var capacity = list.Capacity;

		// act
		list.Clear();

		// assert
		Assert.Equal(0, list.Count);
		Assert.Equal(capacity, list.Capacity);
		list.Add(int3);
		Assert.Equal(int3, list[0]);
		list.Dispose();
	}

	[Fact]
	public void Clear_DefaultInstance_DoesNotThrow()
	{
		// arrange
		var list = default(PooledList<PoolItem>);

		// act
		list.Clear();

		// assert
		Assert.Equal(0, list.Count);
		list.Dispose();
	}


	[Fact]
	public void AsSpan_ListWithItems_ReturnsWritableSpanOfCountLength()
	{
		// arrange
		var list = new PooledList<int>(16);
		list.Add(1);
		list.Add(2);

		// act
		var span = list.AsSpan();

		// assert
		Assert.Equal(2, span.Length);
		span[0] = 10;
		Assert.Equal(10, list[0]);
		Assert.Equal(2, list.AsReadOnlySpan().Length);
		list.Dispose();
	}

	[Fact]
	public void EnsureCapacity_LargerThanCapacity_IncreasesCapacityAndKeepsItems()
	{
		// arrange
		var list = new PooledList<int>(2);
		list.Add(5);

		// act
		list.EnsureCapacity(500);

		// assert
		Assert.True(list.Capacity >= 500);
		Assert.Equal(1, list.Count);
		Assert.Equal(5, list[0]);
		list.Dispose();
	}


	[Fact]
	public void AddRange_ItemsExceedingCapacity_AppendsAndGrows()
	{
		// arrange
		var list = new PooledList<int>(2);
		var int1 = _faker.Random.Int();
		list.Add(int1);
		var items = Enumerable.Range(2, 100).ToArray();

		// act
		list.AddRange(items);

		// assert
		Assert.Equal(101, list.Count);
		Assert.Equal(int1, list[0]);
		Assert.Equal(101, list[100]);
		list.Dispose();
	}

	[Fact]
	public void AddRange_EmptySpan_DoesNothing()
	{
		// arrange
		var list = default(PooledList<int>);
		
		// act
		list.AddRange(ReadOnlySpan<int>.Empty);

		// assert
		Assert.Equal(0, list.Count);
		Assert.Equal(0, list.Capacity);
		list.Dispose();
	}

	[Fact]
	public void Add_DefaultInstance_GrowsFromNullBuffer()
	{
		// arrange
		var list = default(PooledList<int>);
		var int1 = _faker.Random.Int();

		Assert.Equal(0, list.Count);
		Assert.True(list.IsEmpty);
		Assert.Equal(0, list.Capacity);
		Assert.True(list.AsSpan().IsEmpty);
		
		// act
		list.Add(int1); // must grow from the null buffer

		// assert
		Assert.Equal(1, list.Count);
		Assert.Equal(int1, list[0]);
		list.Dispose();
	}

	[Fact]
	public void Add_ThousandItems_GrowsAndKeepsValues()
	{
		// arrange
		var list = new PooledList<int>(2);

		// act
		for (var i = 0; i < 255; i++) list.Add(i);
		
		// assert
		Assert.Equal(255, list.Count);
		Assert.True(list.Capacity >= 255);
		for (var i = 0; i < 255; i++) Assert.Equal(i, list[i]);
		list.Dispose();
	}

	[Fact]
	public void Add_ItemFromOwnBufferWhileGrowing_KeepsValue()
	{
		// arrange
		var list = new PooledList<int>(1);
		var int1 = _faker.Random.Int();
		list.Add(int1);

		// act
		while (list.Count < list.Capacity) list.Add(list.Count);
		var countBefore = list.Count;
		list.Add(list[0]); // 'in' aliasing the buffer that Grow returns to the pool

		// assert
		Assert.Equal(countBefore + 1, list.Count);
		Assert.Equal(int1, list[countBefore]);
		list.Dispose();
	}

	[Fact]
	public void Indexer_ModifyThroughRef_UpdatesItemInPlace()
	{
		// arrange
		var list = new PooledList<PoolItem>(4);
		var int1 = _faker.Random.Int();
		list.Add(new PoolItem { Id = 1, Name = "a" });
		
		// act
		list[0].Id = int1;

		// assert
		Assert.Equal(int1, list[0].Id);
		Assert.Equal("a", list[0].Name);
		list.Dispose();
	}

	[Fact]
	public void Sort_AscendingThenDescendingComparison_OrdersItems()
	{
		// arrange
		var list = new PooledList<int>(8);
		foreach (var v in new[] { 5, 1, 4, 2, 3 }) list.Add(v);
		list.Sort(static (a, b) => a.CompareTo(b));
		int[] expectedAsc = new[] { 1, 2, 3, 4, 5 };
		int[] expectedDesc = new[] { 5, 4, 3, 2, 1 };

		// act
		Assert.Equal(expectedAsc, list.ToArray());
		list.Sort(static (a, b) => b.CompareTo(a));

		// act
		Assert.Equal(expectedDesc, list.ToArray());
		list.Dispose();
	}

}
