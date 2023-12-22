using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema;

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
        var metaTable = _sut.GetMeta(schemaName,DatabaseProvider.PostgreSql);
        metaTable.LoadMapper();

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
        Assert.Equal("id", metaTable.Fields[metaTable.ColumnMapper[0]].Name);
        Assert.Equal("schema_id", metaTable.Fields[metaTable.ColumnMapper[1]].Name);
        Assert.Equal("object_type", metaTable.Fields[metaTable.ColumnMapper[2]].Name);
        Assert.Equal("reference_id", metaTable.Fields[metaTable.ColumnMapper[3]].Name);
        Assert.Equal("data_type", metaTable.Fields[metaTable.ColumnMapper[4]].Name);
        Assert.Equal("flags", metaTable.Fields[metaTable.ColumnMapper[5]].Name);
        Assert.Equal("name", metaTable.Fields[metaTable.ColumnMapper[6]].Name);
        Assert.Equal("description", metaTable.Fields[metaTable.ColumnMapper[7]].Name);
        Assert.Equal("value", metaTable.Fields[metaTable.ColumnMapper[8]].Name);
        Assert.Equal("active", metaTable.Fields[metaTable.ColumnMapper[9]].Name);
    }


    [Fact]
    internal void GetMetaId_AnonymousSchema_MetaIdTableObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var metaIdTable = _sut.GetMetaId(schemaName, DatabaseProvider.PostgreSql);
        metaIdTable.LoadMapper();

        // assert
        Assert.Equal("test.\"@meta_id\"", metaIdTable.PhysicalName);
        Assert.Equal(TableType.MetaId, metaIdTable.Type);
        Assert.Equal(4, metaIdTable.Fields.Length);
        Assert.True(metaIdTable.Baseline);
        Assert.NotNull(metaIdTable.GetField("id"));
        Assert.NotNull(metaIdTable.GetField("schema_id"));
        Assert.NotNull(metaIdTable.GetField("object_type"));
        Assert.NotNull(metaIdTable.GetField("value"));
        Assert.Equal("id", metaIdTable.Fields[metaIdTable.ColumnMapper[0]].Name);
        Assert.Equal("schema_id", metaIdTable.Fields[metaIdTable.ColumnMapper[1]].Name);
        Assert.Equal("object_type", metaIdTable.Fields[metaIdTable.ColumnMapper[2]].Name);
        Assert.Equal("value", metaIdTable.Fields[metaIdTable.ColumnMapper[3]].Name);
        Assert.Equal(FieldType.Int, metaIdTable.Fields[metaIdTable.ColumnMapper[0]].Type);
        Assert.Equal(FieldType.Int, metaIdTable.Fields[metaIdTable.ColumnMapper[1]].Type);
        Assert.Equal(FieldType.Byte, metaIdTable.Fields[metaIdTable.ColumnMapper[2]].Type);
        Assert.Equal(FieldType.Long, metaIdTable.Fields[metaIdTable.ColumnMapper[3]].Type);
    }
    
    [Fact]
    internal void GetLog_AnonymousSchema_LogTableObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var logTable = _sut.GetLog(schemaName, DatabaseProvider.PostgreSql);
        logTable.LoadMapper();

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
        Assert.Equal("id", logTable.Fields[logTable.ColumnMapper[0]].Name);
        Assert.Equal("entry_time", logTable.Fields[logTable.ColumnMapper[1]].Name);
        Assert.Equal("level_id", logTable.Fields[logTable.ColumnMapper[2]].Name);
        Assert.Equal("schema_id", logTable.Fields[logTable.ColumnMapper[3]].Name);
        Assert.Equal("thread_id", logTable.Fields[logTable.ColumnMapper[4]].Name);
        Assert.Equal("call_site", logTable.Fields[logTable.ColumnMapper[5]].Name);
        Assert.Equal("job_id", logTable.Fields[logTable.ColumnMapper[6]].Name);
        Assert.Equal("method", logTable.Fields[logTable.ColumnMapper[7]].Name);
        Assert.Equal("line_number", logTable.Fields[logTable.ColumnMapper[8]].Name);
        Assert.Equal("message", logTable.Fields[logTable.ColumnMapper[9]].Name);
        Assert.Equal("description", logTable.Fields[logTable.ColumnMapper[10]].Name);
    }


}
