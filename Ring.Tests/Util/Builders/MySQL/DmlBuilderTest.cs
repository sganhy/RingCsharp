using AutoFixture;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Builders.MySQL;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Util.Builders.MySQL;

public class DmlBuilderTest : BaseBuilderTest
{
    private readonly IDmlBuilder _sut;
    private readonly IFixture _fixture;
    private readonly DbSchema _schema;

    public DmlBuilderTest()
    {
        _fixture = new Fixture();
        var metaList = GetSchema1();
        var meta = new Meta(_fixture.Create<string>());
        _schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.MySql) ?? 
            MetaExtensions.GetEmptySchema(meta, DatabaseProvider.MySql);
        _sut = new DmlBuilder();
        _sut.Init(_schema);
    }

    [Fact]
    internal void Insert_Table1_InsertSql()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        var expectedResult = "INSERT INTO rpg_sheet.t_skill (id,name,sub_name,is_group,category,armor_penality,trained_only,try_again) VALUES (?,?,?,?,?,?,?,?)";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.Insert(table, false);
        var result2 = _sut.Insert(table, false); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Insert_MtmTable_InsertSql()
    {
        // arrange 
        var table = _schema.GetTable(1021); // get book
        var relation = table?.GetRelation("book2class");
        var mtmTable = relation?.ToTable;
        var expectedResult = "INSERT INTO rpg_sheet.`@mtm_01021_01031_009` (book2class,class2book) VALUES (?,?)";

        // act 
        Assert.NotNull(mtmTable);
        var result1 = _sut.Insert(mtmTable, true);
        var result2 = _sut.Insert(mtmTable, true); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Insert_EmptyTable_InsertSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var meta = new Meta("Test");
        meta.SetEntityType(EntityType.Table);
        var metaSch = new Meta("Test");
        metaSch.SetEntityType(EntityType.Schema);
        var schema = MetaExtensions.ToSchema(new Meta[] { meta, metaSch }, DatabaseProvider.MySql);
        var expectedResult = "INSERT INTO test.t_test () VALUES ()";
        var tableTest = schema?.GetTable("Test");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(tableTest);
        sut.Init(schema);
        var result = sut.Insert(tableTest, true);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Insert_Table2WithRel_InsertSql()
    {
        // arrange 
        var table = _schema.GetTable("deity");
        var expectedResult = "INSERT INTO rpg_sheet.t_deity (id,name,nickname,portfolio,symbol,deity2alignment,deity2gender) VALUES (?,?,?,?,?,?,?)";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.Insert(table, true);
        var result2 = _sut.Insert(table, true); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Delete_Table1_DeleteSql()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        var expectedResult = "DELETE FROM rpg_sheet.t_skill WHERE id=?";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.Delete(table);
        var result2 = _sut.Delete(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Delete_MtmTable_DeleteSql()
    {
        // arrange 
        var table = _schema.GetTable(1021); // get book
        var relation = table?.GetRelation("book2class");
        var mtmTable = relation?.ToTable;
        var expectedResult = "DELETE FROM rpg_sheet.`@mtm_01021_01031_009` WHERE book2class=? AND class2book=?";

        // act 
        Assert.NotNull(mtmTable);
        var result1 = _sut.Delete(mtmTable);
        var result2 = _sut.Delete(mtmTable); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Delete_TableMeta_DeleteSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var tblBuilder = new TableBuilder();
        var schemaName = "@Test";
        var table = tblBuilder.GetMeta(schemaName, DatabaseProvider.MySql);
        var metaTbl = table.ToMeta(0);
        var metaSch = new Meta(schemaName);
        metaSch.SetEntityType(EntityType.Schema);
        var metaList = new List<Meta>() { metaSch };
        metaList.AddRange(metaTbl);
        var schema = MetaExtensions.ToSchema(metaList.ToArray(), DatabaseProvider.MySql);
        var expectedResult = "DELETE FROM `@test`.`@meta` WHERE id=? AND schema_id=? AND object_type=? AND reference_id=?";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema);
        var result1 = sut.Delete(table);
        var result2 = sut.Delete(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Delete_TableMetaId_DeleteSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var tblBuilder = new TableBuilder();
        var schemaName = "@Test";
        var table = tblBuilder.GetMetaId(schemaName, DatabaseProvider.MySql);
        var metaTbl = table.ToMeta(0);
        var metaSch = new Meta(schemaName);
        metaSch.SetEntityType(EntityType.Schema);
        var metaList = new List<Meta>() { metaSch };
        metaList.AddRange(metaTbl);
        var schema = MetaExtensions.ToSchema(metaList.ToArray(), DatabaseProvider.MySql);
        var expectedResult = "DELETE FROM `@test`.`@meta_id` WHERE id=? AND schema_id=? AND object_type=?";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema);
        var result1 = sut.Delete(table);
        var result2 = sut.Delete(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

}
