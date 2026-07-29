using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;

namespace Ring.Tests.Util.Builders;

public sealed class BaseDdlBuilderTest : BaseBuilderTest
{

    private readonly BaseDdlBuilder _sut;

    public BaseDdlBuilderTest()
    {
        _sut = new DdlBuilder();
    }


    [Fact]
    public void AlterAddColumn_Field1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 22, 2);
        var field = table.Fields[10];
        field = field.SetSize(80);
        field = field.SetType(FieldType.String);
        table.Fields[10] = field;
        var col = table.GetColumn(field.Id, EntityType.Field);
        Assert.NotNull(col);
        col = col.Value.SetFieldType(FieldType.String);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {col.Value.PhysicalName} varchar(80) COLLATE \"C\"";

        // act 
        var dql = _sut.AlterAddColumn(table, col.Value);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterAddColumn_Field2_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 17, 2);
        var field = table.Fields[10];
        field = field.SetType(FieldType.String);
        table.Fields[11] = field;
        var col = table.GetColumn(field.Id, EntityType.Field);
        Assert.NotNull(col);
        col = col.Value.SetFieldType(FieldType.DateTimeOffset);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {col.Value.PhysicalName} timestamp without time zone";

        // act 
        var dql = _sut.AlterAddColumn(table, col.Value);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterAddColumn_Field3_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 12, 2);
        var field = table.Fields[10];
        field = field.SetType(FieldType.String);
        table.Fields[9] = field;
        var col = table.GetColumn(field.Id, EntityType.Field);
        Assert.NotNull(col);
        col = col.Value.SetFieldType(FieldType.Byte);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {col.Value.PhysicalName} int2";

        // act 
        var dql = _sut.AlterAddColumn(table, col.Value);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterAddColumn_Relation1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 12, 25);
        var relation = table.Relations[10];
        var col = table.GetColumn(relation.Id, EntityType.Relation);
        Assert.NotNull(col);
        col = col.Value.SetFieldType(FieldType.Int);
        table.Columns[table.GetColumnIndex(relation.Id, EntityType.Relation)] = col.Value;
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {col.Value.PhysicalName} int4";

        // act 
        var dql = _sut.AlterAddColumn(table, col.Value);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterDropColumn_Field1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 18, 2);
        var field = table.Fields[16];
        var col = table.GetColumn(field.Id, EntityType.Field);
        Assert.NotNull(col);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} DROP COLUMN {col.Value.PhysicalName}";

        // act 
        var dql = _sut.AlterDropColumn(table, col.Value);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterDropColumn_Relation1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 12, 25);
        var relation = table.Relations[20];
        var col = table.GetColumn(relation.Id, EntityType.Relation);
        Assert.NotNull(col);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} DROP COLUMN {col.Value.PhysicalName}";

        // act 
        var dql = _sut.AlterDropColumn(table, col.Value);

        // assert
        Assert.Equal(expectedSql, dql);
    }


}

