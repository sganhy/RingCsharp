using Bogus;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Builders;

public class TableBuilderTest
{
    private readonly TableBuilder _sut;
    public TableBuilderTest() => _sut = new TableBuilder();

    [Fact]
    internal void GetMeta_PostgreSqlSchema_MetaTableObject()
    {
        // arrange 
        var schemaName = "Test1";

        // act 
        var metaTablePostGre = _sut.GetMeta(schemaName, DatabaseProvider.PostgreSql);

		// assert
		Assert.NotNull(metaTablePostGre);
		Assert.Equal("@meta", metaTablePostGre.Name);
		Assert.Equal("test1.\"@meta\"", metaTablePostGre.PhysicalName);
		Assert.Equal("Central catalog that describes every structural element of the database.", metaTablePostGre.Description);
		Assert.Equal(10, metaTablePostGre.Fields.Length);
		Assert.Equal(0, metaTablePostGre.ObjectIndex);
		Assert.Equal(TableType.Meta, metaTablePostGre.Type);
        Assert.True(metaTablePostGre.Baseline);
        Assert.True(metaTablePostGre.Cached);
        Assert.True(metaTablePostGre.Readonly);
        Assert.True(metaTablePostGre.Active);
        Assert.False(metaTablePostGre.AllowHardDeletion);
        Assert.False(metaTablePostGre.UsePreparedStatement);
        Assert.False(metaTablePostGre.AllowAttributeExtension);
        Assert.NotNull(metaTablePostGre.GetField("id"));
        Assert.NotNull(metaTablePostGre.GetField("schema_id"));
        Assert.NotNull(metaTablePostGre.GetField("object_type"));
        Assert.NotNull(metaTablePostGre.GetField("reference_id"));
        Assert.NotNull(metaTablePostGre.GetField("data_type"));
        Assert.NotNull(metaTablePostGre.GetField("flags"));
        Assert.NotNull(metaTablePostGre.GetField("name"));
        Assert.NotNull(metaTablePostGre.GetField("description"));
        Assert.NotNull(metaTablePostGre.GetField("value"));
        Assert.NotNull(metaTablePostGre.GetField("active"));
        Assert.NotNull(metaTablePostGre.GetColumn("id"));
        Assert.NotNull(metaTablePostGre.GetColumn("schema_id"));
        Assert.NotNull(metaTablePostGre.GetColumn("object_type"));
        Assert.Equal("id", metaTablePostGre.Fields[metaTablePostGre.Columns[0].RecordIndex].Name);
		Assert.Equal(1, metaTablePostGre.Fields[metaTablePostGre.Columns[0].RecordIndex].Id);
		Assert.Equal("schema_id", metaTablePostGre.Fields[metaTablePostGre.Columns[1].RecordIndex].Name);
        Assert.Equal("object_type", metaTablePostGre.Fields[metaTablePostGre.Columns[2].RecordIndex].Name);
        Assert.Equal("reference_id", metaTablePostGre.Fields[metaTablePostGre.Columns[3].RecordIndex].Name);
        Assert.Equal("data_type", metaTablePostGre.Fields[metaTablePostGre.Columns[4].RecordIndex].Name);
        Assert.Equal("flags", metaTablePostGre.Fields[metaTablePostGre.Columns[5].RecordIndex].Name);
        Assert.Equal("name", metaTablePostGre.Fields[metaTablePostGre.Columns[6].RecordIndex].Name);
        Assert.Equal("description", metaTablePostGre.Fields[metaTablePostGre.Columns[7].RecordIndex].Name);
        Assert.Equal("value", metaTablePostGre.Fields[metaTablePostGre.Columns[8].RecordIndex].Name);
        Assert.Equal("active", metaTablePostGre.Fields[metaTablePostGre.Columns[9].RecordIndex].Name);
		Assert.Equal(0, metaTablePostGre.Indexes?.Length ?? -1);
		Assert.Equal(15, metaTablePostGre.Constraints?.Length ?? -1);
	}

	[Fact]
    internal void GetMetaId_PostgreSqlSchema_MetaIdTableObject()
    {
        // arrange 
        var schemaName = "Test2";

        // act 
        var metaIdTablePostGre = _sut.GetMetaId(schemaName, DatabaseProvider.PostgreSql);

		// assert
		Assert.NotNull(metaIdTablePostGre);
		Assert.True(metaIdTablePostGre.Baseline);
        Assert.True(metaIdTablePostGre.Cached);
        Assert.True(metaIdTablePostGre.Readonly);
        Assert.True(metaIdTablePostGre.Active);
        Assert.False(metaIdTablePostGre.AllowHardDeletion);
        Assert.True(metaIdTablePostGre.UsePreparedStatement);
        Assert.False(metaIdTablePostGre.AllowAttributeExtension);
        Assert.Equal("test2.\"@meta_id\"", metaIdTablePostGre.PhysicalName);
		Assert.Equal("Provides controlled, scoped id generation.", metaIdTablePostGre.Description);
		Assert.Equal(TableType.MetaId, metaIdTablePostGre.Type);
        Assert.Equal(4, metaIdTablePostGre.Fields.Length);
        Assert.NotNull(metaIdTablePostGre.GetField("id"));
        Assert.NotNull(metaIdTablePostGre.GetField("schema_id"));
        Assert.NotNull(metaIdTablePostGre.GetField("object_type"));
        Assert.NotNull(metaIdTablePostGre.GetField("value"));
        Assert.Equal("id", metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[0].RecordIndex].Name);
        Assert.Equal("schema_id", metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[1].RecordIndex].Name);
        Assert.Equal("object_type", metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[2].RecordIndex].Name);
        Assert.Equal("value", metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[3].RecordIndex].Name);
        Assert.Equal(FieldType.Int, metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[0].RecordIndex].Type);
        Assert.Equal(FieldType.Int, metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[1].RecordIndex].Type);
        Assert.Equal(FieldType.Byte, metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[2].RecordIndex].Type);
        Assert.Equal(FieldType.Long, metaIdTablePostGre.Fields[metaIdTablePostGre.Columns[3].RecordIndex].Type);
		Assert.Equal(0, metaIdTablePostGre?.Indexes.Length);
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
		Assert.Equal(34, catalog.Id);
		Assert.Equal("schema_name", catalog.Fields[catalog.Columns[0].RecordIndex].Name);
        Assert.Equal("name", catalog.Fields[catalog.Columns[1].RecordIndex].Name);
		Assert.Equal(2, catalog.Columns.Length);
		Assert.Equal("table_schema", catalog.Columns[0].PhysicalName);
		Assert.Equal("table_name", catalog.Columns[1].PhysicalName);
	}

    [Fact]
    internal void GetCatalog_MySqlTable_TableCatalog()
    {
        // arrange 
        // act 
        var catalog = _sut.GetCatalog(EntityType.Table, DatabaseProvider.MySql);

		// assert
		Assert.Equal("information_schema.tables", catalog.PhysicalName);
		Assert.Equal(PhysicalType.View, catalog.PhysicalType);
		Assert.Equal(2, catalog.Fields.Length);
		Assert.Equal(34, catalog.Id);
		Assert.Equal("schema_name", catalog.Fields[catalog.Columns[0].RecordIndex].Name);
		Assert.Equal("name", catalog.Fields[catalog.Columns[1].RecordIndex].Name);
		Assert.Equal(2, catalog.Columns.Length);
		Assert.Equal("table_schema", catalog.Columns[0].PhysicalName);
		Assert.Equal("table_name", catalog.Columns[1].PhysicalName);
	}

    [Fact]
    internal void GetLog_AnonymousSchema_LogTableObject()
    {
        // arrange 
        var schemaName = "Test";

        // act 
        var logTable = _sut.GetLog(schemaName, DatabaseProvider.PostgreSql);

		// assert
		Assert.NotNull(logTable);
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
        Assert.Equal("Provides the operational audit trail for all schemas managed by the @meta table.", logTable.Description);
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
		Assert.Equal(1, logTable?.Indexes.Length);
		Assert.Equal(1, logTable?.Indexes[0].Columns.Length);
		Assert.Equal("entry_time", logTable?.Indexes[0].Columns[0].PhysicalName);
	}


}
