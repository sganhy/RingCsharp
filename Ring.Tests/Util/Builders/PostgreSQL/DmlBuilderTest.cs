using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Util.Builders.PostgreSQL;

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
        _schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql) ??
            MetaExtensions.GetEmptySchema(meta);
        _sut = new DmlBuilder();
        _sut.Init(_schema);
    }

    [Fact]
    internal void Insert_Schema1_InsertSql()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        var expectedResult = "INSERT INTO rpg_sheet.t_skill (id,name,sub_name,is_group,category,armor_penality,trained_only,try_again) VALUES ($0,$1,$2,$3,$4,$5,$6,$7)";

        // act 
#pragma warning disable CS8604 // Possible null reference argument.
        var result1 = _sut.Insert(table, false);
        var result2 = _sut.Insert(table, false); // using cache 
#pragma warning restore CS8604 // Possible null reference argument.

        // assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
    }
}
