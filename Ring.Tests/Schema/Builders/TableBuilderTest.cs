using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Builders;

public class TableBuilderTest
{
    private readonly TableBuilder _sut;
    public TableBuilderTest()
    {
        _sut = new TableBuilder();
    }

    [Fact]
    internal void GetMeta_AnonymousSchema_MetaTableObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var metaTable = _sut.GetMeta(schemaName, DatabaseProvider.PostgreSql);
        metaTable.LoadRelationRecordIndex();

        // assert
        Assert.Equal("test.\"@meta\"", metaTable.PhysicalName);
        Assert.Equal(10, metaTable.Fields.Length);
        Assert.Equal(TableType.Meta, metaTable.Type);
        Assert.True(metaTable.Baseline);
        Assert.NotNull(metaTable.GetField("id"));
        Assert.NotNull(metaTable.GetField("schema_id"));
        Assert.NotNull(metaTable.GetField("object_type"));
        Assert.NotNull(metaTable.GetField("reference_id"));
        Assert.NotNull(metaTable.GetField("data_type"));
        Assert.NotNull(metaTable.GetField("flags"));
        Assert.NotNull(metaTable.GetField("name"));
        Assert.NotNull(metaTable.GetField("description"));
        Assert.NotNull(metaTable.GetField("value"));
        Assert.NotNull(metaTable.GetField("active"));

        Assert.NotNull(metaTable.GetColumn("id"));
        Assert.NotNull(metaTable.GetColumn("schema_id"));
        Assert.NotNull(metaTable.GetColumn("object_type"));

        Assert.Equal("id", metaTable.Fields[metaTable.Columns[0].RecordIndex].Name);
        Assert.Equal("schema_id", metaTable.Fields[metaTable.Columns[1].RecordIndex].Name);
        Assert.Equal("object_type", metaTable.Fields[metaTable.Columns[2].RecordIndex].Name);
        Assert.Equal("reference_id", metaTable.Fields[metaTable.Columns[3].RecordIndex].Name);
        Assert.Equal("data_type", metaTable.Fields[metaTable.Columns[4].RecordIndex].Name);
        Assert.Equal("flags", metaTable.Fields[metaTable.Columns[5].RecordIndex].Name);
        Assert.Equal("name", metaTable.Fields[metaTable.Columns[6].RecordIndex].Name);
        Assert.Equal("description", metaTable.Fields[metaTable.Columns[7].RecordIndex].Name);
        Assert.Equal("value", metaTable.Fields[metaTable.Columns[8].RecordIndex].Name);
        Assert.Equal("active", metaTable.Fields[metaTable.Columns[9].RecordIndex].Name);
    }


    [Fact]
    internal void GetMetaId_AnonymousSchema_MetaIdTableObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var metaIdTable = _sut.GetMetaId(schemaName, DatabaseProvider.PostgreSql);
        metaIdTable.LoadRelationRecordIndex();

        // assert
        Assert.Equal("test.\"@meta_id\"", metaIdTable.PhysicalName);
        Assert.Equal(TableType.MetaId, metaIdTable.Type);
        Assert.Equal(4, metaIdTable.Fields.Length);
        Assert.True(metaIdTable.Baseline);
        Assert.NotNull(metaIdTable.GetField("id"));
        Assert.NotNull(metaIdTable.GetField("schema_id"));
        Assert.NotNull(metaIdTable.GetField("object_type"));
        Assert.NotNull(metaIdTable.GetField("value"));
        Assert.Equal("id", metaIdTable.Fields[metaIdTable.Columns[0].RecordIndex].Name);
        Assert.Equal("schema_id", metaIdTable.Fields[metaIdTable.Columns[1].RecordIndex].Name);
        Assert.Equal("object_type", metaIdTable.Fields[metaIdTable.Columns[2].RecordIndex].Name);
        Assert.Equal("value", metaIdTable.Fields[metaIdTable.Columns[3].RecordIndex].Name);
        Assert.Equal(FieldType.Int, metaIdTable.Fields[metaIdTable.Columns[0].RecordIndex].Type);
        Assert.Equal(FieldType.Int, metaIdTable.Fields[metaIdTable.Columns[1].RecordIndex].Type);
        Assert.Equal(FieldType.Byte, metaIdTable.Fields[metaIdTable.Columns[2].RecordIndex].Type);
        Assert.Equal(FieldType.Long, metaIdTable.Fields[metaIdTable.Columns[3].RecordIndex].Type);
    }

    [Fact]
    internal void GetCatalog_PostgreSqlTable_TableCatalog()
    {
        // arrange 
        // act 
        var catalog = _sut.GetCatalog(EntityType.Table, DatabaseProvider.PostgreSql);

        // assert
        Assert.Equal("information_schema.tables", catalog.PhysicalName);
        Assert.Equal(PhysicalType.View, catalog.PhysicalType);
        Assert.Equal(2, catalog.Fields.Length);
        Assert.Equal("table_schema", catalog.Fields[catalog.Columns[0].RecordIndex].Name);
        Assert.Equal("table_name", catalog.Fields[catalog.Columns[1].RecordIndex].Name);
    }

    [Fact]
    internal void GetCatalog_MySqlTable_TableCatalog()
    {
        // arrange 
        // act 
        var catalog = _sut.GetCatalog(EntityType.Table, DatabaseProvider.MySql);
        catalog.LoadRelationRecordIndex();

        // assert
        Assert.Equal("information_schema.tables", catalog.PhysicalName);
        Assert.Equal(PhysicalType.View, catalog.PhysicalType);
        Assert.Equal(2, catalog.Fields.Length);
        Assert.Equal("table_schema", catalog.Fields[catalog.Columns[0].RecordIndex].Name);
        Assert.Equal("table_name", catalog.Fields[catalog.Columns[1].RecordIndex].Name);
    }

    [Fact]
    internal void GetLog_AnonymousSchema_LogTableObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var logTable = _sut.GetLog(schemaName, DatabaseProvider.PostgreSql);

        // assert
        Assert.Equal("test.\"@log\"", logTable.PhysicalName);
        Assert.Equal(TableType.Log, logTable.Type);
        Assert.Equal(11, logTable.Fields.Length);
        Assert.True(logTable.Baseline);
        Assert.NotNull(logTable.GetField("id"));
        Assert.NotNull(logTable.GetField("entry_time"));
        Assert.NotNull(logTable.GetField("level_id"));
        Assert.NotNull(logTable.GetField("schema_id"));
        Assert.NotNull(logTable.GetField("thread_id"));
        Assert.NotNull(logTable.GetField("call_site"));
        Assert.NotNull(logTable.GetField("job_id"));
        Assert.NotNull(logTable.GetField("method"));
        Assert.NotNull(logTable.GetField("line_number"));
        Assert.NotNull(logTable.GetField("message"));
        Assert.NotNull(logTable.GetField("description"));
        Assert.Equal("id", logTable.Fields[logTable.Columns[0].RecordIndex].Name);
        Assert.Equal("entry_time", logTable.Fields[logTable.Columns[1].RecordIndex].Name);
        Assert.Equal("level_id", logTable.Fields[logTable.Columns[2].RecordIndex].Name);
        Assert.Equal("schema_id", logTable.Fields[logTable.Columns[3].RecordIndex].Name);
        Assert.Equal("thread_id", logTable.Fields[logTable.Columns[4].RecordIndex].Name);
        Assert.Equal("call_site", logTable.Fields[logTable.Columns[5].RecordIndex].Name);
        Assert.Equal("job_id", logTable.Fields[logTable.Columns[6].RecordIndex].Name);
        Assert.Equal("method", logTable.Fields[logTable.Columns[7].RecordIndex].Name);
        Assert.Equal("line_number", logTable.Fields[logTable.Columns[8].RecordIndex].Name);
        Assert.Equal("message", logTable.Fields[logTable.Columns[9].RecordIndex].Name);
        Assert.Equal("description", logTable.Fields[logTable.Columns[10].RecordIndex].Name);
    }


}
