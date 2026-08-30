using Ring.Data;
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
        var maxPoolSize = 2;
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 20 };

        // act 
        var metaSchema = _sut.GetMeta(DatabaseProvider.PostgreSql, config);
        var metaTable = metaSchema.GetTable(13);
        var metaLog = metaSchema.GetTable("@log");
        var metaId = metaSchema.GetTable("@meta_id");

        // assert
        Assert.NotNull(metaSchema);
        Assert.True(metaSchema.Baseline);
        Assert.NotNull(metaTable);
        Assert.NotNull(metaLog);
        Assert.NotNull(metaId);
        Assert.Equal("@meta", metaTable.Name);
        Assert.Equal("test.\"@meta\"", metaTable.PhysicalName);
        Assert.Equal("test.\"@log\"", metaLog.PhysicalName);
        Assert.Equal("test.\"@meta_id\"", metaId.PhysicalName);
        Assert.Equal(10, metaTable.Fields.Length);
        Assert.Equal(10, metaTable.Columns.Length);
        Assert.Equal(11, metaTable.RecordSize);
        Assert.Equal(4, metaId.Fields.Length);
        Assert.Equal(4, metaId.Columns.Length);
        Assert.Equal(5, metaId.RecordSize);
        Assert.Equal(11, metaLog.Fields.Length);
        Assert.Equal(11, metaLog.Columns.Length);
        Assert.Equal(12, metaLog.RecordSize);
        // test max pool size
        Assert.Equal(maxPoolSize, metaSchema.Connections.Connections.Length);
    }
}
