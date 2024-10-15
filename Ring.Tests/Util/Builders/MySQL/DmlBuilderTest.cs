using AutoFixture;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
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
        _schema = Meta.ToSchema(metaList,DatabaseProvider.MySql) ??
            Meta.GetEmptySchema(meta, DatabaseProvider.MySql);
        _sut = new DmlBuilder();
        _sut.Init(_schema, _schema.GetTableIndex());
    }

    [Fact]
    internal void Insert_Table1_InsertSql()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        var expectedResult = "INSERT INTO rpg_sheet.t_skill (id,name,skill2ability,sub_name,is_group,category,armor_penality,trained_only,try_again) VALUES (:a1,:a2,:a3,:a4,:a5,:a6,:a7,:a8,:a9)";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.Insert(table);
        var result2 = _sut.Insert(table); // using cache 

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
        var expectedResult = "INSERT INTO rpg_sheet.`@mtm_01021_01031_009` (book2class,class2book) VALUES (:a1,:a2)";

        // act 
        Assert.NotNull(mtmTable);
        var result1 = _sut.Insert(mtmTable);
        var result2 = _sut.Insert(mtmTable); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }

    [Fact]
    internal void Insert_EmptyTable_InsertSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var schemaId = _fixture.Create<int>();
        var testTable= new Meta(_fixture.Create<int>(), (byte)EntityType.Table, schemaId, (int)TableType.Business
            , 0L, "Test", null, null, true);
        var testSchema= new Meta(schemaId, (byte)EntityType.Schema, schemaId, 0, 0L, "Test", null, null, true);
        var schema = Meta.ToSchema(new Meta[] { testTable, testSchema }, DatabaseProvider.MySql);
        var expectedResult = "INSERT INTO test.t_test () VALUES ()";
        var tableTest = schema?.GetTable("Test");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(tableTest);
        sut.Init(schema, schema.GetTableIndex());
        var result = sut.Insert(tableTest);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Insert_Table2WithRel_InsertSql()
    {
        // arrange 
        var table = _schema.GetTable("deity");
        var expectedResult = "INSERT INTO rpg_sheet.t_deity (id,deity2alignment,name,deity2gender,nickname,portfolio,symbol) VALUES (:a1,:a2,:a3,:a4,:a5,:a6,:a7)";

        // act 
        Assert.NotNull(table);
        var result = _sut.Insert(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Delete_Table1_DeleteSql()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        var expectedResult = "DELETE FROM rpg_sheet.t_skill WHERE id=:a1";

        // act 
        Assert.NotNull(table);
        var result = _sut.Delete(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Delete_MtmTable_DeleteSql()
    {
        // arrange 
        var table = _schema.GetTable(1021); // get book
        var relation = table?.GetRelation("book2class");
        var mtmTable = relation?.ToTable;
        var expectedResult = "DELETE FROM rpg_sheet.`@mtm_01021_01031_009` WHERE book2class=:a1 AND class2book=:a2";

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
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var schema = schBuilder.GetMeta(schemaName, DatabaseProvider.MySql, 2, string.Empty);
        var table = schema.GetTable("@meta");
        var expectedResult = "DELETE FROM `@test`.`@meta` WHERE id=:a1 AND schema_id=:a2 AND object_type=:a3 AND reference_id=:a4";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema, schema.GetTableIndex());
        var result = sut.Delete(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Delete_TableMetaId_DeleteSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var schBuilder = new SchemaBuilder();
        var schemaName = "@test";
        var schema = schBuilder.GetMeta(schemaName, DatabaseProvider.MySql, 2, string.Empty);
        var table = schema.GetTable("@meta_id");
        var expectedResult = "DELETE FROM `@test`.`@meta_id` WHERE id=:a1 AND schema_id=:a2 AND object_type=:a3";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema, schema.GetTableIndex());
        var result = sut.Delete(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Update_Table1_UpdateSql()
    {
        // arrange 
        var table = _schema.GetTable("armor");
        var expectedResult = "UPDATE rpg_sheet.t_armor SET {0} WHERE id=:a1";

        // act 
        Assert.NotNull(table);
        var result = _sut.Update(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Update_TableMeta_UpdateSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var schema = schBuilder.GetMeta(schemaName, DatabaseProvider.MySql, 8, string.Empty);
        var table = schema.GetTable("@meta");
        var expectedResult = "UPDATE `@test`.`@meta` SET {0} WHERE id=:a1 AND schema_id=:a2 AND object_type=:a3 AND reference_id=:a4";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema, schema.GetTableIndex());
        var result = sut.Update(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void Update_TableMetaId_UpdateSql()
    {
        // arrange 
        var sut = new DmlBuilder();
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var schema = schBuilder.GetMeta(schemaName, DatabaseProvider.MySql, 8, string.Empty);
        var table = schema.GetTable("@meta_id");
        var expectedResult = "UPDATE `@test`.`@meta_id` SET {0} WHERE id=:a1 AND schema_id=:a2 AND object_type=:a3";

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        sut.Init(schema, schema.GetTableIndex());
        var result = sut.Update(table);

        // assert
        Assert.Equal(expectedResult, result);
    }

}
