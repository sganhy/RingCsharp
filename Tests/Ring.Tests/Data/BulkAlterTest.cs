using Bogus;
using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Data;
public sealed class BulkAlterTest
{
    private readonly Faker _faker=new();

    [Fact]
    public void CreateTable_TestValue_BulkAlterInvalidTableName()
    {
        // arrange 
        var builder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = _faker.Random.String(), MaxConnectionPoolSize = 20 };
        var schema = builder.GetMeta(DatabaseProvider.SqlServer, config);
        var table = schema.GetTable("Test");

		// act 
		Assert.NotNull(table);

		var ex = Assert.Throws<ArgumentException>(() => {
                var bulk = new BulkAlter(schema);
                bulk.CreateTable(table);
			}
        );

        // assert
        Assert.Equal("Object type 'Test' does not exist.", ex.Message);
    }


    [Fact]
    public void AlterTableAdd_TestValue_ThrowInvalidFieldName()
    {
        // arrange 
        var builder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = _faker.Random.String(), MaxConnectionPoolSize = 20 };
        var schema = builder.GetMeta(DatabaseProvider.SqlServer, config);

        // act 
        var ex = Assert.Throws<ArgumentException>(() => {
                var bulk = new BulkAlter(schema);
                bulk.AlterTableAdd("@meta", "Test");
            });

        // assert
        Assert.Equal("Column name 'Test' does not exist for object type '@meta'.", ex.Message);
    }
}
