using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema;
using Xunit.Abstractions;
using Ring.Schema.Enums;
using Ring.Data;

namespace Ring.Tests;

public sealed class GlobalTest : BaseTest
{
    private readonly DbSchema _schema;

    public GlobalTest(ITestOutputHelper output) : base(output)
    {
        var metaList = GetSchema1();
        var meta = new Meta("Test");
        _schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql, SchemaType.Static, SchemaLoadType.Full) ?? Meta.GetEmptySchema(meta, DatabaseProvider.PostgreSql);
        var config = new Configuration
        {
            MaxNumberOfSchema = 4096
        };
        Global.Init(config);
    }

    [Fact]
    public void GetSchema_AnonymousName_SchemaObject()
    {
        // arrange 
        var lstOfName = new List<string>();
        for (var i = 0; i < 255; ++i)
        {
            //var schemaName = _faker.Random.String();
            var schemaName = _faker.Random.String()+"-" + i;
            lstOfName.Add(schemaName);
            var schema = new Meta(i+1, (byte)EntityType.Schema, 0, 0, 0L, schemaName, _faker.Random.String(), null, true);
            var schemaObj = Meta.GetEmptySchema(schema, DatabaseProvider.SqlServer);
            if (i == 98)
            {
                Console.WriteLine("44411");
            }
            Global.LoadSchema(schemaObj);
        }
        var j = 1;

        foreach (var name in lstOfName)
        {
            // act 
            var sch = Global.GetSchema(name);

            // assert
            Assert.NotNull(sch);
            Assert.Equal(j, sch.Id);
            j++;
        }
        Global.Clear();
    }

}
