using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
namespace Ring.Tests.Schema.Extensions;

public sealed class ConstraintExtensionsTest : BaseTest
{
	public ConstraintExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Fact]
	internal void ToMeta_PrimaryKeyConstraint_MetaStruct()
	{
		// arrange 
		var id = _faker.Random.Number(int.MinValue, int.MaxValue);
		var builder = new TableBuilder();
		var meta = builder.GetMeta("@test",DatabaseProvider.PostgreSql);
		var name = "name";
		var description = "description";
		var defaultCol = new Column(EntityType.Field, FieldType.Long, "???", SearchableType.None, 2, 7, 20, true);
		var col1 = meta.GetColumn("id") ?? defaultCol;
		var col2 = meta.GetColumn("reference_id") ?? defaultCol;
		// int id, string name,string? description, bool baseline, bool enabled, ConstraintType type,  Column[] columns, long? minValue = null, long? maxValue = null) : base(id, name, description, baseline, enabled
		var constraint = new Constraint(id, name, description, true, true, ConstraintType.PrimaryKey, new Column[] { col1, col2 }, 0, 100);
		var expectedValue = "0;100;id;reference_id";
		var expectedCsv = $"{id},19,13,1,8192,\"name\",\"description\",\"0;100;id;reference_id\",True";

		// act 
		var subresult = constraint.ToMeta(meta);
		var result = subresult.ToCsv();

		// assert
		Assert.Equal(expectedValue, subresult.Value);
		Assert.Equal(expectedCsv, result);
	}


	[Fact]
	internal void GetHashCode_ConstraintEqual_False()
	{
		// arrange 
		var id = _faker.Random.Number(int.MinValue, int.MaxValue);
		var builder = new TableBuilder();
		var meta = builder.GetMeta("@test", DatabaseProvider.PostgreSql);
		var name = "name";
		var description = "description";
		var defaultCol = new Column(EntityType.Field, FieldType.Long, "???", SearchableType.None, 2, 7, 20, true);
		var col1 = meta.GetColumn("id") ?? defaultCol;
		var col2 = meta.GetColumn("reference_id") ?? defaultCol;
		// int id, string name,string? description, bool baseline, bool enabled, ConstraintType type,  Column[] columns, long? minValue = null, long? maxValue = null) : base(id, name, description, baseline, enabled
		var constraint1 = new Constraint(id, name, description, true, true, ConstraintType.PrimaryKey, new Column[] { col1, col2 }, 0, 100);
		var constraint2 = new Constraint(id, name, description, true, true, ConstraintType.ForeignKey, new Column[] { col1, col2 }, 0, 100);
		var constraint3 = new Constraint(id, name, description, true, false, ConstraintType.PrimaryKey, new Column[] { col1, col2 }, 0, 100);

		// act 
		var hash1 = ConstraintExtensions.Hash(constraint1);
		var hash2 = constraint2.GetHashCode();
		var hash3 = constraint3.GetHashCode();

		// assert
		Assert.NotEqual(hash1, hash2);
		Assert.NotEqual(hash1, hash3);
		Assert.NotEqual(hash2, hash3);
	}

	[Fact]
	internal void Equals_2AnonymousConstraints_False()
	{
		// arrange 
		var id = _faker.Random.Number(int.MinValue, int.MaxValue);
		var builder = new TableBuilder();
		var meta = builder.GetMeta("@test", DatabaseProvider.PostgreSql);
		var name = "name";
		var description = "description";
		var defaultCol = new Column(EntityType.Field, FieldType.Long, "???", SearchableType.None, 2, 7, 20, true);
		var col1 = meta.GetColumn("id") ?? defaultCol;
		var col2 = meta.GetColumn("reference_id") ?? defaultCol;
		var physicalName = _faker.Random.String();
		var constraint1 = new Constraint(id, name, description, true, true, ConstraintType.PrimaryKey, new Column[] { col1, col2 }, 0, 100);
		var constraint2 = new Constraint(id, name, description, true, true, ConstraintType.ForeignKey, new Column[] { col1, col2 }, 0, 100);
		var constraint3 = new Constraint(id, name, description, true, false, ConstraintType.PrimaryKey, new Column[] { col1, col2 }, 0, 100);
		var constraint4 = new Constraint(id, name, description, true, true, ConstraintType.ForeignKey, new Column[] { col1, col2 }, 0, 100);
		var constraint5 = new Constraint(id, name, description, true, true, ConstraintType.UniqueKey, new Column[] { col1, col2 }, 0, 100);

		// act 
		var result1 = constraint1 == constraint2;
		var result2 = constraint1.Equals(constraint3);
		var result3 = constraint2.Equals((object)constraint3);
		var result4 = constraint2 != constraint4;
		var result5 = ConstraintExtensions.IsEquivalentTo(constraint4, constraint5);

		// assert
		Assert.False(result1);
		Assert.False(result2);
		Assert.False(result3);
		Assert.False(result4);
		Assert.False(result5);
	}

}
