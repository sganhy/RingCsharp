using AutoFixture;
using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Data;

public sealed class SpanListTest
{
    private readonly DbSchema _schema;
    private readonly IFixture _fixture;

    public SpanListTest()
    {
        _fixture = new Fixture();
        var builder = new SchemaBuilder();
        IConfiguration config = new Configuration
        {
            ConnectionString = _fixture.Create<string>(),
            DefaultSchema = "test",
            MinConnectionPoolSize = 1,
            MaxConnectionPoolSize = 4,
            DefaultTableStorage = "ring_data",
            DefaultIndexStorage = "ring_index"
        };
        _schema = builder.GetMeta(DatabaseProvider.MySql, config);
    }

}
