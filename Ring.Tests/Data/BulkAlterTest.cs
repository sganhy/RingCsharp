using AutoFixture;
using Ring.Data;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;

namespace Ring.Tests.Data;
public sealed class BulkAlterTest
{
    private readonly IFixture _fixture;

    public BulkAlterTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void CreateTable_TestValue_BulkAlterInvalidTableName()
    {
        // arrange 
        var builder = new SchemaBuilder();
        var schema = builder.GetMeta(_fixture.Create<string>(), DatabaseProvider.SqlServer, 20, _fixture.Create<string>());
        var bulk = new BulkAlter(schema);

        // act 
        var ex = Assert.Throws<ArgumentException>(() => bulk.CreateTable("Test"));

        // assert
        Assert.Equal("Object type 'Test' does not exist.", ex.Message);
    }
}
