using AutoFixture;
using Ring.Data;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Tests.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbSchema = Ring.Schema.Models.Schema;
using Record = Ring.Data.Record;

namespace Ring.Tests.Data;

public class BulkSaveTest : BaseTest
{
    private readonly IFixture _fixture;
    private readonly DbSchema _schema;
    
    public BulkSaveTest()
    {
        var metaList = base.GetSchema1();
        _fixture = new Fixture();
        var meta = new Meta("Test");
        _schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql) ??
                    Meta.GetEmptySchema(meta, DatabaseProvider.PostgreSql);
    }

    [Fact]
    public void CountByType_FeatRecords_4()
    {
        // arrange 
        var table = _schema.GetTable("feat");
        Assert.NotNull(table);
        var rcdFeat = new Record(table);
        var bs = new BulkSave(_schema);
        bs.InsertRecord(rcdFeat); // {1}
        bs.Queries.Increment();
        bs.InsertRecord(rcdFeat); // {2}

        rcdFeat.SetField("id", 7777);
        bs.UpdateRecord(rcdFeat); // {3}
        bs.DeleteRecord(rcdFeat); // {4}
        // act 
        // assert
    }


}
