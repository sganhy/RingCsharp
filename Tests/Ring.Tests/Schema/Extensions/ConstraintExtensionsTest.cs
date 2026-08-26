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
		var defaultCol = new Column(EntityType.Field, FieldType.Long, "???", SearchableType.None, 2, 7); ;
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

}
