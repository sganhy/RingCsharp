using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;

namespace Ring.Tests.Util.Builders.PostgreSQL;

public class DdlBuilderTest : BaseBuilderTest
{
    private readonly IDdlBuilder _sut;
    private readonly IFixture _fixture;
    public DdlBuilderTest()
    {
        _sut = new DdlBuilder();
        _fixture = new Fixture();
    }

    [Fact]
    public void Drop_Table1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var expectedSql = $"DROP TABLE {table.PhysicalName}";

        // act 
        var dql = _sut.Drop(table);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void Create_Table1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var expectedSql = $"CREATE TABLE {table.PhysicalName} (";

        // act 
        var dql = _sut.Create(table);

        // assert
        Assert.True(dql?.StartsWith(expectedSql));
        Assert.True(dql?.EndsWith(')'));
    }

    [Fact]
    public void Create_Table2_DdlQuery()
    {
        // arrange 
        var metaTable = GetMeta2Table();
        var metaItems = GetMeta2TableItems(true);
        var physicalName = _fixture.Create<string>();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var table2 = metaTable.ToTable(segment, TableType.Business, PhysicalType.Table, physicalName);
        Assert.NotNull(table2);
        table2.Relations[1] = GetAnonymousRelation(RelationType.Mto, 1, @"skill2book");
        table2.Relations[0] = GetAnonymousRelation(RelationType.Mtm, 8, @"ability2book");
        table2.LoadColumnInformation();
        table2.LoadRelationRecordIndex();
        var expectedSql = $"CREATE TABLE {physicalName} (\n" + "    id int2 NOT NULL,\n    skill2book int8,\n" +
                "    name varchar(80) COLLATE \"C\",\n" + "    sub_name varchar(30) COLLATE \"C\",\n" + "    is_group bool,\n" +
                "    category varchar(8) COLLATE \"C\",\n" + "    armor_penality int2,\n" + "    trained_only bool,\n" +
                "    try_again bool)";

        // act 
        var ddl = _sut.Create(table2);


        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_Table3_DdlQuery()
    {
        // arrange 
        var metaTable = GetMeta2Table();
        var metaItems = GetMeta2TableItems(false);
        var physicalName = _fixture.Create<string>();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var table3 = metaTable.ToTable(segment, TableType.Meta, PhysicalType.Table, physicalName);
#pragma warning disable CS8602
        table3.Relations[0] = GetAnonymousRelation(RelationType.Mto, 11, @"skill2book", true);
        table3.LoadColumnInformation();
        table3.LoadRelationRecordIndex();
        var expectedSql = $"CREATE TABLE {physicalName} (\n" + "    id int2 NOT NULL,\n" +
                "    name varchar(80) COLLATE \"C\" NOT NULL,\n" + "    sub_name varchar(30) COLLATE \"C\",\n" + "    is_group bool NOT NULL,\n" +
                "    category varchar(8) COLLATE \"C\",\n" + "    armor_penality int2 NOT NULL,\n" + "    trained_only bool NOT NULL,\n" +
                "    try_again bool NOT NULL,\n" + "    skill2book int8 NOT NULL)";

        // act 
        var ddl = _sut.Create(table3);

#pragma warning restore  CS8602

        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_Table4_DdlQuery()
    {
        // arrange 
        var metaTable = GetMeta2Table();
        var tablespaceName = _fixture.Create<string>();
        var tablespace = GetAnonymousTableSpace(tablespaceName);
        var metaItems = GetMeta2TableItems(false);
        var physicalName = _fixture.Create<string>();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var table4 = metaTable.ToTable(segment, TableType.Fake, PhysicalType.Table, physicalName);

#pragma warning disable CS8602
        table4.Relations[0] = GetAnonymousRelation(RelationType.Mto, 11, @"skill2book", false);
        table4.LoadColumnInformation();
        table4.LoadRelationRecordIndex();
        var expectedSql = $"CREATE TABLE {physicalName} (\n" + "    id int2 NOT NULL,\n" +
                "    name varchar(80) COLLATE \"C\" NOT NULL,\n" + "    sub_name varchar(30) COLLATE \"C\",\n" + "    is_group bool NOT NULL,\n" +
                "    category varchar(8) COLLATE \"C\",\n" + "    armor_penality int2 NOT NULL,\n" + "    trained_only bool NOT NULL,\n" +
                "    try_again bool NOT NULL,\n" + $"    skill2book int8) TABLESPACE {tablespaceName}";

        // act 
        var ddl = _sut.Create(table4, tablespace);

#pragma warning restore  CS8602

        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_TableSpace1_DdlQuery()
    {
        var fileName = _fixture.Create<string>();
        var tablespaceName = _fixture.Create<string>();
        // arrange 
        var tablespace = new TableSpace(_fixture.Create<int>(), tablespaceName, _fixture.Create<string?>(), true, true, true, string.Empty,
            fileName, true, true);
        var expectedSql = $"CREATE TABLESPACE {tablespaceName} LOCATION '{fileName}'";

        // act 
        var dql = _sut.Create(tablespace);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void Truncate_Table1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var expectedSql = $"TRUNCATE TABLE {table.PhysicalName}";

        // act 
        var dql = _sut.Truncate(table);

        // assert
        Assert.Equal(expectedSql, dql);
    }


    [Fact]
    public void GetPhysicalName_Field1_FieldName()
    {
        // arrange 
        var field = GetAnonymousField(FieldType.DateTime, 12, 1, "liKe");
        var expectedSql = $"\"{field.Name}\"";

        // act 
        var result = _sut.GetPhysicalName(field);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Field2_FieldName()
    {
        // arrange 
        var field = GetAnonymousField(FieldType.String, 12, 1, "Zorba_Le_Grec");
        var expectedSql = field.Name;

        // act 
        var result = _sut.GetPhysicalName(field);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Field3_FieldName()
    {
        // arrange 
        var field = GetAnonymousField(FieldType.Undefined, 12, 1, "@user");
        var expectedSql = $"\"{field.Name}\"";

        // act 
        var result = _sut.GetPhysicalName(field);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Relation1_RelationName()
    {
        // arrange 
        var relation = GetAnonymousRelation(RelationType.Otop, 5, "rETURnINg", false);
        var expectedSql = $"\"{relation.Name}\"";

        // act 
        var result = _sut.GetPhysicalName(relation);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Table1_TableName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = metaList.ToSchema(DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("campaign_setting");
        var expectedResult = "rpg_sheet.t_campaign_setting";

        // act 
        Assert.NotNull(table);
        Assert.NotNull(schema);
        var result = _sut.GetPhysicalName(table, schema);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetPhysicalName_Schema1_SchemaName()
    {
        // arrange 
        var meta = new Meta("rpg_sheet");
        var schema = MetaExtensions.GetEmptySchema(meta, DatabaseProvider.PostgreSql);
        var expectedResult = "rpg_sheet";

        // act 
        var result = _sut.GetPhysicalName(schema);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetPhysicalName_Schema2_SchemaName()
    {
        // arrange 
        var meta = new Meta("@Test");
        var schema = MetaExtensions.GetEmptySchema(meta, DatabaseProvider.PostgreSql);
        var expectedResult = "\"@test\"";

        // act 
        var result = _sut.GetPhysicalName(schema);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetPhysicalName_MtmTable1_TableName()
    {
        // arrange 
        var emptyTable = MetaExtensions.GetEmptyTable(new Meta("Test"), TableType.Mtm);
        var emptySchema = MetaExtensions.GetEmptySchema(new Meta("Where"), DatabaseProvider.MySql);
        var ddlBuilder = DatabaseProvider.PostgreSql.GetDdlBuilder();
        var expectedValue = "\"where\".\"@mtm_test\"";

        // act 
        var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);

        // assert
        Assert.Equal(expectedValue, physicalName);
    }

}