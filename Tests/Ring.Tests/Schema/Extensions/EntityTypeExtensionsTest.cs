using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Linq.Expressions;

namespace Ring.Tests.Schema.Extensions;

public sealed class EntityTypeExtensionsTest : BaseTest
{
	public EntityTypeExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

	[Theory]
	[InlineData(EntityType.Schema, TableType.SchemaCatalog)]
	[InlineData(EntityType.Table, TableType.TableCatalog)]
	[InlineData(EntityType.Tablespace, TableType.TablespaceCatalog)]
	internal void ToTableType_EntityType_RightTableType(EntityType entityType, TableType expectedTableType)
	{
		// arrange 
		// act 
		var result = EntityTypeExtensions.ToTableType(entityType);

		// assert
		Assert.Equal(expectedTableType, result);
	}
}
