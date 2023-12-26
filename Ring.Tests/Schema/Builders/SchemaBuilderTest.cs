using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Builders;

public sealed class SchemaBuilderTest
{
    private readonly SchemaBuilder _sut;
    public SchemaBuilderTest()
    {
        _sut = new SchemaBuilder();
    }

    [Fact]
    internal void GetMeta_AnonymousSchemaName_MetaSchemaObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var metaSchema = _sut.GetMeta(schemaName, DatabaseProvider.PostgreSql);
        var metaTable = metaSchema.GetTable(3);
        var metaLog = metaSchema.GetTable("@log");
        var metaId = metaSchema.GetTable("@meta_id");

        // assert
        Assert.NotNull(metaSchema);
        Assert.NotNull(metaTable);
        Assert.NotNull(metaLog);
        Assert.NotNull(metaId);
        Assert.Equal("@meta", metaTable.Name);
        Assert.Equal("test.\"@meta\"", metaTable.PhysicalName);
        Assert.Equal("test.\"@log\"", metaLog.PhysicalName);
        Assert.Equal("test.\"@meta_id\"", metaId.PhysicalName);
        Assert.Equal(10, metaTable.Fields.Length);
        Assert.Equal(10, metaTable.ColumnMapper.Length);
        Assert.Equal(4, metaId.Fields.Length);
        Assert.Equal(4, metaId.ColumnMapper.Length);
        Assert.Equal(11, metaLog.Fields.Length);
        Assert.Equal(11, metaLog.ColumnMapper.Length);
    }
}
