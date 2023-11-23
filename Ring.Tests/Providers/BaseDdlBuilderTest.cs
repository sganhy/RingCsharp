using Ring.Schema.Enums;
using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;

namespace Ring.Tests.Providers;

public sealed class BaseDdlBuilderTest : BaseBuilderTest
{

    private readonly BaseDdlBuilder _sut;

    public BaseDdlBuilderTest() {
        _sut = new DdlBuilder();
    }


    [Fact]
    public void AlterAddColumn_Field1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var field = GetAnonymousField(FieldType.String, 80);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {field.Name} varchar(80) COLLATE \"C\"";

        // act 
        var dql = _sut.AlterAddColumn(table, field);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterAddColumn_Field2_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var field = GetAnonymousField(FieldType.LongDateTime, 0);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {field.Name} timestamp with time zone";

        // act 
        var dql = _sut.AlterAddColumn(table, field);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterAddColumn_Field3_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var field = GetAnonymousField(FieldType.Byte, 0);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {field.Name} int2";

        // act 
        var dql = _sut.AlterAddColumn(table, field);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterAddColumn_Relation1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var relation = GetAnonymousRelation(RelationType.Mto);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} ADD {relation.Name} int8";

        // act 
        var dql = _sut.AlterAddColumn(table, relation);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterDropColumn_Field1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var field = GetAnonymousField(FieldType.Byte, 0); 
        var expectedSql = $"ALTER TABLE {table.PhysicalName} DROP COLUMN {field.Name}";

        // act 
        var dql = _sut.AlterDropColumn(table, field);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void AlterDropColumn_Relation1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(12, 2);
        var relation = GetAnonymousRelation(RelationType.Mto);
        var expectedSql = $"ALTER TABLE {table.PhysicalName} DROP COLUMN {relation.Name}";

        // act 
        var dql = _sut.AlterDropColumn(table, relation);

        // assert
        Assert.Equal(expectedSql, dql);
    }



}

