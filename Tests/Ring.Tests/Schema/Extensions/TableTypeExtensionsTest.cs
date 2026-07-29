using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class TableTypeExtensionsTest
{
    [Theory]
    [InlineData(TableType.Meta, "@meta")]
    [InlineData(TableType.MetaId, "@meta_id")]
    [InlineData(TableType.Log, "@log")]
	[InlineData(TableType.TableCatalog, "@table_catalog")]
	[InlineData(TableType.TablespaceCatalog, "@tablespace_catalog")]
	[InlineData(TableType.SchemaCatalog, "@schema_catalog")]
	internal void GetLogicalName(TableType tableType, string expectedValue)
    {
        // arrange 
        // act 
        var result = TableTypeExtensions.GetLogicalName(tableType);

        // assert
        Assert.Equal(expectedValue, result);
    }

}
