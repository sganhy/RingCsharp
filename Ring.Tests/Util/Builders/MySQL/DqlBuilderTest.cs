using AutoFixture;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Util.Builders.MySQL;

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
        _schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.MySql) ??
            MetaExtensions.GetEmptySchema(meta, DatabaseProvider.MySql);
        _sut = new DqlBuilder();
        _sut.Init(_schema);
    }

    [Fact]
    internal void Select_Table1_SqlQuery()
    {
        // arrange 
        var table = _schema.GetTable("gender");
        var expectedResult = "SELECT id,name,short_name,iso_code,status FROM rpg_sheet.t_gender";

        // act 
        Assert.NotNull(table);
        var result1 = _sut.SelectFrom(table);
        var result2 = _sut.SelectFrom(table); // using cache 

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }


}
