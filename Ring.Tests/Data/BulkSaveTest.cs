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
        var tableDeity = _schema.GetTable("deity");
        Assert.NotNull(table);
        Assert.NotNull(tableDeity);
        var rcdFeat = new Record(table);
        var rcdDeity = new Record(tableDeity);
        var bs = new BulkSave(_schema);
        bs.InsertRecord(rcdFeat); // {1}
        bs.InsertRecord(rcdFeat); // {2}
        bs.UpdateRecord(rcdFeat); // {3}
        bs.DeleteRecord(rcdFeat); // {4}
        bs.DeleteRecord(rcdFeat); // {5}

        rcdFeat.SetField("id", 7777);
        bs.UpdateRecord(rcdFeat); // {6}
        bs.DeleteRecord(rcdFeat); // {7}
  
        // act 
        var result = bs.CountByType("feat");

        // assert
        Assert.Equal(4, result);
    }


}
