using AutoFixture;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Data.Common;

namespace Ring.Tests.Schema.Builders;

public sealed class SchemaBuilderTest
{
    private readonly SchemaBuilder _sut;
    private readonly IFixture _fixture;
    public SchemaBuilderTest()
    {
        _sut = new SchemaBuilder();
        _fixture = new Fixture();
    }

    [Fact]
    internal void GetMeta_AnonymousSchemaName_MetaSchemaObject()
    {
        // arrange 
        var schemaName = "Test";
        var maxPoolSize = 20; 

        // act 
        var metaSchema = _sut.GetMeta(schemaName, DatabaseProvider.PostgreSql,20, _fixture.Create<string>(), typeof(DbConnection));
        var metaTable = metaSchema.GetTable(3);
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
        Assert.Equal(10, metaTable.RecordIndexes.Length);
        Assert.Equal(4, metaId.Fields.Length);
        Assert.Equal(4, metaId.RecordIndexes.Length);
        Assert.Equal(11, metaLog.Fields.Length);
        Assert.Equal(11, metaLog.RecordIndexes.Length);
        // test max pool size
        Assert.Equal(maxPoolSize, metaSchema.Connections.Connections.Length);
    }
}
