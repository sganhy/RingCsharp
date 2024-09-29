using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using AutoFixture;
using System.Reflection;
using Ring.Schema;
using Ring.Tests.Schema.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System;

namespace Ring.Tests.Schema;

public sealed class MetaTest : BaseExtensionsTest
{
    private readonly IFixture _fixture;
    private readonly Table _anonymousTable;
    public MetaTest()
    {
        _fixture = new Fixture();
        _anonymousTable = GetAnonymousTable();
    }

    [Theory]
    [InlineData(1L, 1, true)]
    [InlineData(2L, 2, true)]
    [InlineData(4L, 3, true)]
    [InlineData(8L, 4, true)]
    [InlineData(16L, 5, true)]
    [InlineData(32L, 6, true)]
    [InlineData(64L, 7, true)]
    [InlineData(65536L, 17, true)]
    [InlineData(4294967296L, 33, true)]
    [InlineData(long.MinValue, 64, true)]
    internal void ReadFlag_Input_OnlyOneTrueFlag(long flags, byte bitPosition, bool expectedValue)
    {
        // arrange 
        var meta = new Meta(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<EntityType>(), flags, _fixture.Create<string>());

        // act 
        var result = meta.ReadFlag(bitPosition);
        var result08Position = meta.ReadFlag(8);
        var result11Position = meta.ReadFlag(11);
        var result28Position = meta.ReadFlag(28);
        var result42Position = meta.ReadFlag(42);

        // assert
        Assert.Equal(result, expectedValue);
        Assert.False(result08Position);
        Assert.False(result11Position);
        Assert.False(result28Position);
        Assert.False(result42Position);
    }

    [Theory]
    [InlineData(long.MaxValue, 4)]
    [InlineData(long.MaxValue / 2, 19)]
    [InlineData(long.MaxValue / 4, 34)]
    [InlineData(long.MaxValue / 8, 50)]
    [InlineData(long.MaxValue, 64)]
    internal void WriteFlag_Input_TrueFlag(long flags, byte bitPosition)
    {
        // arrange 
        var meta = new Meta(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<EntityType>(), flags, _fixture.Create<string>());

        // act 
        var currentFlag = meta.ReadFlag(bitPosition);
        var newflags = Meta.WriteFlag(meta.Flags, bitPosition, false);
        var result = meta.ReadFlag(bitPosition);
        var flagAfterWrite = Meta.WriteFlag(meta.Flags, bitPosition, currentFlag);

        // assert
        Assert.False(result);
        Assert.Equal(flagAfterWrite, flags); // are flags altered after writting? 
    }

    [Theory]
    [InlineData(0, FieldType.Long)]
    [InlineData(1, FieldType.Int)]
    [InlineData(2, FieldType.Short)]
    [InlineData(3, FieldType.Byte)]
    [InlineData(14, FieldType.Float)]
    [InlineData(15, FieldType.Double)]
    [InlineData(16, FieldType.String)]
    [InlineData(17, FieldType.ShortDateTime)]
    [InlineData(18, FieldType.DateTime)]
    [InlineData(19, FieldType.LongDateTime)]
    [InlineData(21, FieldType.ByteArray)]
    [InlineData(27, FieldType.LongString)]
    [InlineData(125, FieldType.Undefined)]
    [InlineData(126, FieldType.Undefined)]
    internal void GetFieldType_Input_ValidFieldType(int dataType, FieldType expectedResult)
    {
        // arrange 
        var mask = _fixture.Create<int>() << 16;
        var flags = _fixture.Create<long>();
        mask &= 0x0FFFFF00;

        var meta = new Meta(_fixture.Create<int>(), _fixture.Create<byte>(), _fixture.Create<int>(), dataType + mask, flags, _fixture.Create<string>(),
             _fixture.Create<string>(),null, true);

        // act 
        var result = meta.GetFieldType();

        // assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(meta.DataType, dataType + mask); // method is pure ?? 
        Assert.Equal(meta.Flags, flags); // method is pure ?? 
    }

    [Theory]
    [InlineData(8192 + 2, true)]
    [InlineData(8192 + 1, true)]
    [InlineData(8192, true)]
    [InlineData(1111, false)]
    [InlineData(5465, false)]
    internal void IsEntityBaseline_Input_Expected(long mask, bool expectedResult)
    {
        // arrange 
        var flags = _fixture.Create<long>() << 16;
        flags += mask;

        var meta = new Meta(_fixture.Create<int>(), (byte)EntityType.Schema, _fixture.Create<int>(), _fixture.Create<int>(), flags,
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), false);

        // act 
        var result = meta.IsEntityBaseline();

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    internal void GetFieldSize_AnonymoousSize_ReturnValue()
    {
        // arrange 
        var size = _fixture.Create<int>();
        var expectedResult1 = size;
        var expectedResult2 = int.MaxValue;
        var flags = size << 17;
        var meta1 = new Meta(_fixture.Create<int>(), (byte)EntityType.Schema, _fixture.Create<int>(), _fixture.Create<int>(), flags + 1111 + 0x100000000000000,
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), false);
        var meta2 = new Meta(_fixture.Create<int>(), (byte)EntityType.Schema, _fixture.Create<int>(), _fixture.Create<int>(), long.MaxValue,
            _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), false);

        // act 
        var result1 = meta1.GetFieldSize();
        var result2 = meta2.GetFieldSize();

        // assert
        Assert.Equal(expectedResult1, result1);
        Assert.Equal(expectedResult2, result2);
    }

    [Fact]
    internal void ToField_Meta1_FieldObject()
    {
        // arrange 
        // eg. from rpg_schema.xml => ability.name (generated by golang version of ring) 
        var meta = new Meta(2, (byte)EntityType.Field, 1011, 16, 10493964, "name",_fixture.Create<string>(), null, true);

        var exepectedFieldType = FieldType.String;
        var exepectedFieldSize = 80;

        // act 
        var field = meta.ToField();

        // assert
        Assert.NotNull(field);
        Assert.Equal(field.Id, meta.Id);
        Assert.Equal(field.Name, meta.Name);
        Assert.Equal(field.Type, exepectedFieldType);
        Assert.Equal(field.Description, meta.Description);
        Assert.Null(field.DefaultValue);
        Assert.Equal(field.Size, exepectedFieldSize);
        Assert.True(field.Active);
        Assert.False(field.CaseSensitive);
        Assert.True(field.NotNull);
        Assert.True(field.Baseline);
        Assert.True(field.Multilingual);
    }


    [Fact]
    internal void ToField_Meta2_FieldObject()
    {
        // arrange 
        // eg. from rpg_schema.xml => ability.name (generated by golang version of ring) 
        var meta = new Meta(6, (byte)EntityType.Field, 1021, 16, 1712142, "isbn", _fixture.Create<string>(), "Test2", false);
        var exepectedFieldType = FieldType.String;
        var exepectedFieldSize = 13;
        var exepectedDefaultValue = "Test2";

        // act 
        var field = meta.ToField();

        // assert
        Assert.NotNull(field);
        Assert.Equal(field.Id, meta.Id);
        Assert.Equal(field.Name, meta.Name);
        Assert.Equal(field.Type, exepectedFieldType);
        Assert.Equal(field.Description, meta.Description);
        Assert.Equal(field.DefaultValue, exepectedDefaultValue);
        Assert.Equal(field.Size, exepectedFieldSize);
        Assert.False(field.Active);
        Assert.True(field.CaseSensitive);
        Assert.True(field.NotNull);
        Assert.True(field.Baseline);
        Assert.True(field.Multilingual);
    }

    [Fact]
    internal void ToField_Meta3_FieldObject()
    {
        // arrange 
        // eg. from rpg_schema.xml => ability.name (generated by golang version of ring) 
        var meta = new Meta(4, (byte)EntityType.Field, 1032, 2, 6, "status", _fixture.Create<string>(), "Test3", true);
        var exepectedFieldType = FieldType.Short;
        var exepectedFieldSize = 0;
        var exepectedDefaultValue = "Test3";

        // act 
        var field = meta.ToField();

        // assert
        Assert.NotNull(field);
        Assert.Equal(field.Id, meta.Id);
        Assert.Equal(field.Name, meta.Name);
        Assert.Equal(field.Type, exepectedFieldType);
        Assert.Equal(field.Description, meta.Description);
        Assert.Equal(field.DefaultValue, exepectedDefaultValue);
        Assert.Equal(field.Size, exepectedFieldSize);
        Assert.True(field.Active);
        Assert.True(field.CaseSensitive);
        Assert.True(field.NotNull);
        Assert.False(field.Baseline);
        Assert.False(field.Multilingual);
    }

    [Fact]
    internal void ToField_Meta4_FieldObject()
    {
        // arrange 
        // eg. from rpg_schema.xml => ability.name (generated by golang version of ring) 
        var meta = new Meta(4, (byte)EntityType.Undefined, 1032, 2, 6, "status", _fixture.Create<string>(), "Test3", true);
    
        // act 
        var field = meta.ToField();

        // assert
        Assert.Null(field);
    }

    [Fact]
    internal void ToField_Meta5_FieldObject()
    {
        // arrange 
        // eg. from rpg_schema.xml => ability.name (generated by golang version of ring) 
        var dataType = 6;
        dataType = Meta.SetFieldType(dataType, FieldType.Int);
        var meta = new Meta(4, (byte)EntityType.Field, 1032, 2, dataType, "status", _fixture.Create<string>(), null, true);
        var exepectedFieldType = FieldType.Int;
        var exepectedFieldSize = 0;
        var exepectedDefaultValue = "0";

        // act 
        var field = meta.ToField();

        // assert
        Assert.NotNull(field);
        Assert.Equal(field.Id, meta.Id);
        Assert.Equal(field.Name, meta.Name);
        Assert.Equal(field.Type, exepectedFieldType);
        Assert.Equal(field.Description, meta.Description);
        Assert.Equal(field.DefaultValue, exepectedDefaultValue);
        Assert.Equal(field.Size, exepectedFieldSize);
        Assert.True(field.Active);
        Assert.True(field.CaseSensitive);
        Assert.True(field.NotNull);
        Assert.False(field.Baseline);
        Assert.False(field.Multilingual);
    }

    /*
[Theory]

[InlineData(0, EntityType.Table)]
[InlineData(1, EntityType.Field)]
[InlineData(2, EntityType.Relation)]
[InlineData(3, EntityType.Index)]
[InlineData(7, EntityType.Schema)]
[InlineData(15, EntityType.Sequence)]
[InlineData(17, EntityType.Language)]
[InlineData(18, EntityType.Tablespace)]
[InlineData(23, EntityType.Parameter)]
[InlineData(25, EntityType.Alias)]
[InlineData(101, EntityType.Constraint)]
[InlineData(200, EntityType.Undefined)]
internal void GetEntityType_MutlipleInput_EntityType(byte dataType, EntityType entityType)
{
// arrange
var meta = _fixture.Create<Meta>();
meta.SetEntityType(dataType);

// act 
var result = MetaExtensions.GetEntityType(meta);

// assert
Assert.Equal(result, entityType);
}

[Fact]
internal void ToIndex_Meta1_IndexObject()
{
// arrange 
// eg. from rpg_schema.xml => ability.name (generated by golang version of ring) 
var id = _fixture.Create<int>();
var name = _fixture.Create<string>();

var meta = new Meta
{
    DataType = 0,
    Flags = 8704,
    Value = "name;object2book"
};
meta.SetEntityRefId(1071);
meta.SetEntityName(name);
meta.SetEntityId(id);
meta.SetEntityType(EntityType.Index);
meta.SetEntityActive(true);
meta.SetEntityDescription(_fixture.Create<string>());

// act 
var index = meta.ToIndex();

// assert
Assert.NotNull(index);
Assert.Equal(index.Id, id);
Assert.Equal(index.Name, name);
Assert.Equal(index.Description, meta.GetEntityDescription());
Assert.True(index.Active);
Assert.True(index.Baseline);
Assert.True(index.Unique);
Assert.False(index.Bitmap);
}

[Fact]
internal void ToIndex_Meta2_IndexObject()
{
// arrange
var meta = _fixture.Create<Meta>();
meta.SetEntityType(EntityType.Alias);

// act 
var index = meta.ToIndex();

// assert
Assert.Null(index);
}


[Theory]
[InlineData(1, 0x7000D1AB, RelationType.Otop)]
[InlineData(2, 0x6000D91C, RelationType.Otm)]
[InlineData(3, 0x5000D8EE, RelationType.Mtm)]
[InlineData(11, 0x1000D111, RelationType.Mto)]
[InlineData(12, 0x4000D1FC, RelationType.Otof)]
[InlineData(125, 0x4000D1FC, RelationType.Undefined)]
internal void GetRelationType_Input_ValidFieldType(int flags, int mask, RelationType expectedResult)
{
// arrange - BitPositionFirstPositionRelType = 18
flags <<= 18;
flags += mask;
var meta = _fixture.Create<Meta>();
meta.Flags = flags;

// act 
var result = MetaExtensions.GetRelationType(meta);

// assert
Assert.Equal(expectedResult, result);
Assert.Equal(meta.Flags, flags); // method is pure ?? 
}

[Theory]
[InlineData(0x7001D1AB, RelationType.Otop)]
[InlineData(0x6002D91C, RelationType.Otm)]
[InlineData(0x5003D8EE, RelationType.Mtm)]
[InlineData(0x1001D111, RelationType.Mto)]
[InlineData(0x4002D1FC, RelationType.Otof)]
[InlineData(0x4003D1FC, RelationType.Undefined)]
internal void SetRelationType_Input_ValidFieldType(int flags, RelationType relationType)
{
// arrange - BitPositionFirstPositionRelType = 18
var meta = _fixture.Create<Meta>();
meta.Flags = flags;

// act 
MetaExtensions.SetRelationType(meta, relationType);
var result = MetaExtensions.GetRelationType(meta);

// assert
Assert.Equal(relationType, result);
Assert.Equal(meta.Flags & 0xFFFF, flags & 0xFFFF); // other flags altered ? 
//Assert.Equal(meta.Flags & 0xFE000000, flags & 0xFE000000); // other flags altered ? 
}

[Fact]
internal void ToRelation_Meta1_RelationObject()
{
// arrange 
// eg. from rpg_schema.xml => ability2book (generated by golang version of ring) 
var meta = new Meta
{
    DataType = 1021,
    Flags = 786448,
    Value = "book2ability"
};
meta.SetEntityRefId(1011);
meta.SetEntityName("ability2book");
meta.SetEntityId(2);
meta.SetEntityType(EntityType.Relation);
meta.SetEntityActive(true);
meta.SetEntityDescription(_fixture.Create<string>());
var exepectedRelType = RelationType.Mtm;

// act 
var relation = MetaExtensions.ToRelation(meta, _anonymousTable);

// assert
Assert.NotNull(relation);
Assert.Equal(relation.Id, meta.GetEntityId());
Assert.Equal(relation.Name, meta.GetEntityName());
Assert.Equal(relation.Type, exepectedRelType);
Assert.Equal(relation.Description, meta.GetEntityDescription());
Assert.True(relation.Active);
Assert.False(relation.NotNull);
Assert.False(relation.Baseline);
}


[Fact]
internal void ToParameter_Meta1_ParameterObject()
{
// arrange 
// eg. from rpg_schema.xml => ability2book (generated by golang version of ring) 
var expectedValue = "en-US";
var meta = new Meta
{
    DataType = 16,
    Flags = 286720,
    Value = expectedValue
};
meta.SetEntityRefId(0);
meta.SetEntityName("@language");
meta.SetEntityId(4);
meta.SetEntityType(EntityType.Parameter);
meta.SetEntityActive(true);
meta.SetEntityDescription(_fixture.Create<string>());

// act 
var parameter = MetaExtensions.ToParameter(meta);

// assert
Assert.NotNull(parameter);
Assert.Equal(meta.GetEntityId(), parameter.Id);
Assert.Equal(meta.GetEntityName(), parameter.Name);
Assert.Equal(meta.GetEntityDescription(), parameter.Description);
Assert.Equal(expectedValue, parameter.Value);
Assert.Equal(FieldType.String, parameter.ValueType);
Assert.True(parameter.Baseline);
Assert.True(parameter.Active);
}

[Fact]
internal void ToFieldType_AllFieldType_ReturnFielType()
{
// test if Meta.ToFieldType() manage all field type 
// arrange 
var fieldTypeList = Enum.GetValues(typeof(FieldType)) as IEnumerable<FieldType>;
Assert.NotNull(fieldTypeList);

foreach (var fieldType in fieldTypeList)
{
    // act 
    var meta = _fixture.Create<Meta>();
    meta.SetFieldType(fieldType);
    var result = meta.GetFieldType();

    // assert
    Assert.Equal(result, fieldType);
}
}

[Fact]
internal void ToRelationType_AllRelationType_ReturnRelationType()
{
// test if Meta.ToFieldType() manage all field type 
// arrange 
var relTypeList = Enum.GetValues(typeof(RelationType)) as IEnumerable<RelationType>;
Assert.NotNull(relTypeList);

foreach (var relType in relTypeList)
{
    // act 
    var meta = _fixture.Create<Meta>();
    meta.SetRelationType(relType);
    var result = meta.GetRelationType();

    // assert
    Assert.Equal(result, relType);
}
}

[Fact]
internal void ToTableSpace_Meta1_TableSpaceObject()
{
// arrange 
// eg. from rpg_schema.xml => ability2book (generated by golang version of ring) 
var meta = new Meta
{
    DataType = 0,
    Flags = 1024,
    Value = "c:\\temp\\rpg\\index"
};
meta.SetEntityRefId(0);
meta.SetEntityName("rpg_index");
meta.SetEntityId(2);
meta.SetEntityType(EntityType.Tablespace);
meta.SetEntityActive(true);
meta.SetEntityDescription(_fixture.Create<string>());

// act 
var tableSpace = MetaExtensions.ToTableSpace(meta);

// assert
Assert.NotNull(tableSpace);
Assert.Equal(tableSpace.Description, meta.GetEntityDescription());
Assert.Equal(tableSpace.Name, meta.GetEntityName());
Assert.True(tableSpace.Index);
Assert.False(tableSpace.Table);
Assert.False(tableSpace.Constraint);
Assert.True(tableSpace.Active);
Assert.Equal(tableSpace.Id, meta.GetEntityId());
}

[Fact]
internal void ToTable_Meta1_TableObject()
{
// arrange 
var metaTable = GetMeta1Table();
var metaItems = GetMeta1TableItems();
var physicalName = _fixture.Create<string>();
var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);

// act 
var table = MetaExtensions.ToTable(metaTable, segment, TableType.Fake, PhysicalType.Table, physicalName);
var field = table?.GetField("name");
var fieldPk = table?.GetField("id");

// assert
Assert.NotNull(table);
Assert.NotNull(field);
Assert.NotNull(fieldPk);
Assert.Equal(table.Id, metaTable.GetEntityId());
Assert.Equal(table.Description, metaTable.GetEntityDescription());
Assert.Equal(table.Name, metaTable.GetEntityName());
Assert.Equal(TableType.Fake, table.Type);
Assert.True(table.Baseline);
Assert.True(table.Readonly);
Assert.Single(table.Relations);
Assert.Equal(8, table.Fields.Length);
Assert.Equal(8, table.RecordIndexes.Length);
Assert.Equal(FieldType.String, field.Type);
Assert.Equal(80, field.Size);
Assert.Equal("name", field.Name);
Assert.True(field.NotNull);
Assert.True(field.Multilingual);
Assert.True(fieldPk.IsPrimaryKey());
Assert.Equal(FieldType.Short, fieldPk.Type);
}

[Fact]
internal void ToTable_Meta2_MtmTableObject()
{
// arrange 
var metaTable = GetMeta2Table();
var metaItems = GetMeta2TableItems();
var physicalName = _fixture.Create<string>();
var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);

// act 
var table = MetaExtensions.ToTable(metaTable, segment, TableType.Mtm, PhysicalType.Table, physicalName);
var fieldPk = table?.GetField("id");

// assert
Assert.NotNull(table);
Assert.Null(fieldPk);
Assert.Equal(TableType.Mtm, table.Type);
}

[Fact]
internal void ToSchema_Schema1_SchemaObject()
{
// arrange 
var metaList = GetSchema1();
var expectedPhysicalName = "rpg_sheet.t_ability";

// act 
var result = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql);
var tblAbility = result?.GetTable("ability");
var fldName = tblAbility?.GetField("name");
var relAbility2book = tblAbility?.GetRelation("ability2book");
var mtmAbility2book = relAbility2book?.ToTable;
var relAbility2book2 = mtmAbility2book?.GetRelation("ability2book");
var tblAlignment = result?.GetTable("alignment");
var tblGender = result?.GetTable("gender");
var tblRace = result?.GetTable(1051);
var tblWeapon = result?.GetTable(1071);
var tblAlignmentDesc = result?.GetTable(1013);

// assert
Assert.NotNull(result);
Assert.NotNull(fldName);
Assert.NotNull(tblAbility);
Assert.NotNull(tblAlignment);
Assert.NotNull(tblGender);
Assert.NotNull(tblRace);
Assert.NotNull(tblWeapon);
Assert.NotNull(tblAlignmentDesc);
Assert.NotNull(mtmAbility2book);
Assert.NotNull(relAbility2book);
Assert.NotNull(relAbility2book2);
Assert.Equal(expectedPhysicalName, tblAbility.PhysicalName);
Assert.Equal("alignment", tblAlignment.Name);
Assert.Equal(1, tblGender.SchemaId);
Assert.Equal(2, result.TableSpaces.Length);
Assert.Equal(TableType.Business, tblAlignmentDesc.Type);
Assert.Equal(RelationType.Mtm, relAbility2book.Type);
Assert.Equal("book", relAbility2book2.ToTable.Name);
Assert.True(fldName.Multilingual);
Assert.True(fldName.Baseline);
Assert.True(fldName.NotNull);
}

[Fact]
internal void ToSchema_Schema2_SchemaObject()
{
// arrange 
var metaList = GetSchema1();
var rel = GetAnonymousRelation("Test2");

// act 
var result = MetaExtensions.ToSchema(metaList, DatabaseProvider.PostgreSql);

// assert
Assert.NotNull(result);
// check if there is no null relationship 
foreach (var table in result.TablesByName)
{
    foreach (var relation in table.Relations)
    {
        Assert.NotNull(relation);
        Assert.NotNull(relation.InverseRelation);
        // SetInverseRelation not working !!
        relation.SetInverseRelation(rel);
        Assert.NotEqual(rel.Name, relation.InverseRelation.Name);
    }
}
}

[Fact]
internal void GetEmptyRelation_Meta_RelationObject()
{
// arrange 
var meta = new Meta(_fixture.Create<string>());

// act 
var relation = MetaExtensions.GetEmptyRelation(meta, RelationType.Otm);

// assert
Assert.Equal(relation.Name, meta.GetEntityName());
Assert.Equal(RelationType.Otm, relation.Type);
Assert.Equal(relation.ToTable.Name, meta.GetEntityName());
Assert.Equal(TableType.Fake, relation.ToTable.Type);
}
*/
}