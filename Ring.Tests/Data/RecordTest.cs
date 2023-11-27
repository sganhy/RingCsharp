using Ring.Tests.Schema.Extensions;
using Record = Ring.Data.Record;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Runtime.InteropServices;

namespace Ring.Tests.Data;

public sealed class RecordTest : BaseExtensionsTest
{
    private readonly DbSchema _schema;

    public RecordTest()
    {
        var metaList = GetSchema1();
        var meta = new Meta("Test");
        _schema = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql) ??
                    MetaExtensions.GetEmptySchema(meta, DatabaseProvider.PostgreSql);
    }

    [Fact]
    internal void Equal_EmptyRecord_True()
    {
        // arrange 
        var rcd = new Record();
        var OtherRcd = new Record();

        // act 
        var result = rcd.Equals(OtherRcd);

        // assert
        Assert.True(result);
    }

    [Fact]
    internal void Equal_Null_False()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var result = rcd.Equals(null);

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void Equal_ClassRecord_True()
    {
        // arrange 
        var classTable = _schema.GetTable("feat");
        Assert.NotNull(classTable);
        var rcd1 = new Record(classTable);
        var rcd2 = new Record(classTable);

        // act 
        var result = rcd1.Equals(rcd2);

        // assert
        Assert.True(result);
    }

}

