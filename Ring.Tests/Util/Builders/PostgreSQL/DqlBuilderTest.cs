using AutoFixture;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Util.Builders.PostgreSQL;

public sealed class DqlBuilderTest : BaseBuilderTest
{
    private readonly IDqlBuilder _sut;
    private readonly IFixture _fixture;
    private readonly DbSchema _schema;

    public DqlBuilderTest()
    {
        _fixture = new Fixture();
        var metaList = GetSchema1();
        var meta = new Meta(_fixture.Create<string>());
        _schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql) ??
            Meta.GetEmptySchema(meta, DatabaseProvider.PostgreSql);
        _sut = new DqlBuilder();
        _sut.Init(_schema);
    }

    [Fact]
    internal void Select_Table1_SqlQuery()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        var expectedResult = "SELECT id,name,skill2ability,sub_name,is_group,category,armor_penality,trained_only,try_again FROM rpg_sheet.t_skill";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.SelectFrom(table);
        var result2 = _sut.SelectFrom(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Select_MtmTable_SqlQuery()
    {
        // arrange 
        var table = _schema.GetTable(1021); // get book
        var relation = table?.GetRelation("book2class");
        var mtmTable = relation?.ToTable;
        var expectedResult = "SELECT book2class,class2book FROM rpg_sheet.\"@mtm_01021_01031_009\"";

        // act 
        Assert.NotNull(mtmTable);
        var result1 = _sut.SelectFrom(mtmTable);
        var result2 = _sut.SelectFrom(mtmTable); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Select_EmptyTable_SqlQuery()
    {
        // arrange 
        var sut = new DqlBuilder();
        var meta = new Meta(_fixture.Create<int>(), "Test", EntityType.Table);
        var metaSch = new Meta(_fixture.Create<int>(), "Test", EntityType.Schema);
        var schema = Meta.ToSchema((new Meta[] { meta, metaSch }), DatabaseProvider.PostgreSql);
        var expectedResult = "SELECT FROM test.t_test";
        var tableTest = schema?.GetTable("Test");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(tableTest);
        sut.Init(schema);
        var result = sut.SelectFrom(tableTest);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Select_Table2WithRel_SqlQuery()
    {
        // arrange 
        var table = _schema.GetTable("deity");
        var expectedResult = "SELECT id,deity2alignment,name,deity2gender,nickname,portfolio,symbol FROM rpg_sheet.t_deity";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.SelectFrom(table);
        var result2 = _sut.SelectFrom(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Select_TableMeta_SqlQuery()
    {
        // arrange 
        var sut = new DqlBuilder();
        var tblBuilder = new TableBuilder();
        var schemaName = "@Test";
        var table = tblBuilder.GetMeta(schemaName, DatabaseProvider.PostgreSql);
        var metaTbl = table.ToMeta(0);
        var metaSch = new Meta(_fixture.Create<int>(), schemaName, EntityType.Schema);
        var metaList = new List<Meta>() { metaSch };
        metaList.AddRange(metaTbl);
        var schema = Meta.ToSchema(metaList.ToArray(), DatabaseProvider.PostgreSql);
        var expectedResult = "SELECT id,schema_id,object_type,reference_id,data_type,flags,name,description,value,active FROM \"@test\".\"@meta\"";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema);
        var result1 = sut.SelectFrom(table);
        var result2 = sut.SelectFrom(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Select_TableMetaId_SqlQuery()
    {
        // arrange 
        var sut = new DqlBuilder();
        var tblBuilder = new TableBuilder();
        var schemaName = "@Test";
        var table = tblBuilder.GetMetaId(schemaName, DatabaseProvider.PostgreSql);
        var metaTbl = table.ToMeta(0);
        var metaSch = new Meta(_fixture.Create<int>(), schemaName, EntityType.Schema);
        var metaList = new List<Meta>() { metaSch };
        metaList.AddRange(metaTbl);
        var schema = Meta.ToSchema(metaList.ToArray(), DatabaseProvider.PostgreSql);
        var expectedResult = "SELECT id,schema_id,object_type,value FROM \"@test\".\"@meta_id\"";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema);
        var result1 = sut.SelectFrom(table);
        var result2 = sut.SelectFrom(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Select_TableWithSpecFields_SqlQuery()
    {
        // field with reserved word in PostGreSQl eg. CURRENT_TIMESTAMP, ANALYZE, and @User
        // arrange 
        var sut = new DqlBuilder();
        var meta = new Meta(_fixture.Create<int>(), "Lateral", EntityType.Table);
        var metaSch = new Meta(_fixture.Create<int>(), "Test", EntityType.Schema);
        //int id, byte objectType, int referenceId, int dataType, long flags, string name, string? description, string? value, bool active
        var metaField1 = new Meta(_fixture.Create<int>(), (byte)EntityType.Field, meta.Id, 0,0, "CURRENT_TIMESTAMP", null,null,true);
        var metaField2 = new Meta(_fixture.Create<int>(), (byte)EntityType.Field, meta.Id, 0, 0, "ANALYZE", null, null, true);
        var metaField3 = new Meta(_fixture.Create<int>(), (byte)EntityType.Field, meta.Id, 0, 0, "@User", null, null, true);
        var schema = Meta.ToSchema((new Meta[] { meta, metaSch, metaField1, metaField2, metaField3 }), DatabaseProvider.PostgreSql);
        var expectedResult = "SELECT \"@User\",\"ANALYZE\",\"CURRENT_TIMESTAMP\" FROM test.t_lateral";
        var tableTest = schema?.GetTable("Lateral");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(tableTest);
        sut.Init(schema);
        var result = sut.SelectFrom(tableTest);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Exists_PostgreSqlTable_SqlCatalogQuery()
    {
        // arrange 
        var sut = new DqlBuilder();
        var meta = new Meta(_fixture.Create<int>(), "Lateral", EntityType.Table);
        var table = Meta.GetEmptyTable(meta, TableType.Business);

        // act 
        var result = sut.Exists(table);

        // assert
        Assert.NotNull(result);
    }


}
