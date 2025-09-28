using Bogus;
using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Tests.Data;

public sealed class SpanListTest
{
    private readonly DbSchema _schema;
    private readonly Faker _faker = new();

    public SpanListTest()
    {
        var builder = new SchemaBuilder();
        IConfiguration config = new Configuration
        {
            ConnectionString = _faker.Random.String(),
            DefaultSchema = "test",
            MinConnectionPoolSize = 1,
            MaxConnectionPoolSize = 4,
            DefaultTableStorage = "ring_data",
            DefaultIndexStorage = "ring_index"
        };
        _schema = builder.GetMeta(DatabaseProvider.MySql, config);
    }

}
