using Ring.Tests.Schema.Extensions;
using Record = Ring.Data.Record;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using AutoFixture;
using Xunit;
using System.Runtime.InteropServices;

namespace Ring.Tests.Data;

public sealed class RecordTest : BaseExtensionsTest
{
    private readonly DbSchema _schema;
    private readonly IFixture _fixture;

    public RecordTest()
    {
        var metaList = GetSchema1();
        _fixture = new Fixture();
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
    internal void Equal_FeatRecord_True()
    {
        // arrange 
        var featTable = _schema.GetTable("feat");
        Assert.NotNull(featTable);
        var data = new string?[featTable.Fields.Length];
        data[0] = null;
        for (var i= 1;i<featTable.Fields.Length; ++i) data[i] = _fixture.Create<string?>();
        var rcd1 = new Record(featTable, data);
        var rcd2 = new Record(featTable, data);

        // act 
        var result = rcd1.Equals(rcd2);

        // assert
        Assert.True(result);
    }


    [Fact]
    internal void Equal_RaceRecord_False()
    {
        // arrange 
        var raceTable = _schema.GetTable("race");
        Assert.NotNull(raceTable);
        var data1 = new string?[raceTable.Fields.Length];
        var data2 = new string?[raceTable.Fields.Length];
        data1[0] = null;
        for (var i = 1; i < raceTable.Fields.Length; ++i) data1[i] = _fixture.Create<string?>();
        var rcd1 = new Record(raceTable, data1);
        var rcd2 = new Record(raceTable, data2);

        // act 
        var result = rcd1 == rcd2;

        // assert
        Assert.False(result);
    }


    [Fact]
    internal void Equal_DifferentRecordType_False()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        var ruleTable = _schema.GetTable("rule");
        Assert.NotNull(genderTable);
        Assert.NotNull(ruleTable);
        var rcd1 = new Record(genderTable);
        var rcd2 = new Record(ruleTable);

        // act 
        var result = rcd1 == rcd2;

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void Equal_DifferentRecordType_True()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        var ruleTable = _schema.GetTable("rule");
        Assert.NotNull(genderTable);
        Assert.NotNull(ruleTable);
        var rcd1 = new Record(genderTable);
        var rcd2 = new Record(ruleTable);

        // act 
        var result1 = rcd1 != rcd2;
        var result2 = rcd2 != rcd1;

        // assert
        Assert.True(result1);
        Assert.True(result2);
    }


    [Fact]
    internal void Equal_DifferentRecordTypeWithNull_False()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        Assert.NotNull(genderTable);
        var rcd1 = new Record(genderTable,new string?[genderTable.Fields.Length+1]); //!!! important +1 to lenght
        var rcd2 = new Record(genderTable);

        // act 
        var result1 = rcd2.Equals((object)rcd1);
        var result2 = rcd1 != rcd2;

        // assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    internal void GetHashCode_DifferentRecordTypeWithNull_NotEquals()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        Assert.NotNull(genderTable);
        var rcd1 = new Record(genderTable, new string?[genderTable.Fields.Length]);
        var rcd2 = new Record(genderTable);

        // act 
        var result1 = rcd1.GetHashCode();
        var result2 = rcd2.GetHashCode();

        // assert
        Assert.NotEqual(result1, result2);
    }


    [Fact]
    internal void GetHashCode_DifferentRecordTypeWithNull_Equals()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        Assert.NotNull(genderTable);
        var data = new string?[genderTable.Fields.Length];
        data[1] = _fixture.Create<string>();
        var rcd1 = new Record(genderTable, data);
        var rcd2 = new Record(genderTable, data);

        // act 
        var result1 = rcd1.GetHashCode();
        var result2 = rcd2.GetHashCode();

        // assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    internal void SetField_AnonymousValue_RecordUnkownRecordType()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.SetField(_fixture.Create<string>(), null));

        // assert
        Assert.Equal("This Record object has an unknown RecordType.  The RecordType \nproperty must be set before performing this operation.", ex.Message);
    }

    [Fact]
    internal void SetField_AnonymousValue_ThrowArgumentException()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        Assert.NotNull(genderTable);
        var rcd = new Record(genderTable);

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.SetField("zorro", _fixture.Create<string>()));

        // assert
        Assert.Equal("Field name 'zorro' does not exist for object type 'gender'.", ex.Message);
    }

    [Fact]
    internal void SetField_AnonymousString_ReturnSameString()
    {
        // arrange 
        var table = _schema.GetTable("campaign_setting");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length+1]);

        // act 
        rcd.SetField("name", "test1");
        rcd.SetField("world", "012345678912");

        // assert
        Assert.Equal("test1",rcd.GetField("name"));
        Assert.Equal("0123456789", rcd.GetField("world")); // with truncate 
    }

    [Fact]
    internal void SetField_AnonymousShort_ReturnShortValue()
    {
        // arrange 
        var table = _schema.GetTable("alignment");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);
        var expectedValue = _fixture.Create<short>().ToString();

        // act 
        rcd.SetField("id", expectedValue);

        // assert
        Assert.Equal(expectedValue, rcd.GetField("id"));
    }

    [Fact]
    internal void SetField_AnonymousInt_ReturnIntegerValue()
    {
        // arrange 
        var table = _schema.GetTable("book");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);
        var expectedValue = _fixture.Create<int>().ToString();

        // act 
        rcd.SetField("id", expectedValue);

        // assert
        Assert.Equal(expectedValue, rcd.GetField("id"));
    }

    [Fact]
    internal void SetField_AnonymousInt_ThrowValueTooLarge()
    {
        // arrange 
        var genderTable = _schema.GetTable("gender");
        Assert.NotNull(genderTable);
        var rcd = new Record(genderTable);

        // act 
        var ex = Assert.Throws<OverflowException>(() => rcd.SetField("id", "1000000"));

        // assert
        Assert.Equal("Value was either too large or too small for an Int16.", ex.Message);
    }


    [Fact]
    internal void SetField_AnonymousInt_ThrowWrongStringFormat()
    {
        // arrange 
        var featType = _schema.GetTable("feat_type");
        Assert.NotNull(featType);
        var rcd = new Record(featType);

        // act 
        var ex = Assert.Throws<FormatException>(() => rcd.SetField("id", "&&&qqd"));

        // assert
        Assert.Equal("Input string was not in a correct format.", ex.Message);
    }

    [Fact]
    internal void SetField_AnonymousShort_ReturnShort()
    {
        // arrange 
        var campaignSettingTable = _schema.GetTable("campaign_setting");
        Assert.NotNull(campaignSettingTable);
        var rcd = new Record(campaignSettingTable);
        var expectedValue = _fixture.Create<short>();

        // act 
        rcd.SetField("status", expectedValue);

        // assert
        Assert.Equal(expectedValue.ToString(), rcd.GetField("status"));
    }

    [Fact]
    internal void SetField_AnonymousInt_ReturnInt()
    {
        // arrange 
        var armorTable = _schema.GetTable("armor");
        Assert.NotNull(armorTable);
        var rcd = new Record(armorTable);
        var expectedValue = _fixture.Create<int>();

        // act 
        rcd.SetField("cost", expectedValue);

        // assert
        Assert.Equal(expectedValue.ToString(), rcd.GetField("cost"));
    }

    [Fact]
    internal void SetField_MaxLong_ThrowValueTooLarge()
    {
        // arrange 
        var armorTable = _schema.GetTable("armor");
        Assert.NotNull(armorTable);
        var rcd = new Record(armorTable);
        var expectedValue = long.MaxValue;

        // act 
        var ex = Assert.Throws<OverflowException>(() => rcd.SetField("cost", expectedValue));

        // assert
        Assert.Equal("Value was either too large or too small for an Int32.", ex.Message);
    }

    [Fact]
    internal void SetField_MaxInt_ThrowValueTooLarge()
    {
        // arrange 
        var armorTable = _schema.GetTable("armor");
        Assert.NotNull(armorTable);
        var rcd = new Record(armorTable);
        var expectedValue = int.MaxValue;

        // act 
        var ex = Assert.Throws<OverflowException>(() => rcd.SetField("max_dex_bonus", expectedValue));

        // assert
        Assert.Equal("Value was either too large or too small for an Int16.", ex.Message);
    }

    [Fact]
    internal void SetField_Bool_True()
    {
        // arrange 
        var armorTable = _schema.GetTable("armor");
        Assert.NotNull(armorTable);
        var rcd = new Record(armorTable);

        // act 
        rcd.SetField("heavy",true);

        // assert
        Assert.Equal("True",rcd.GetField("heavy"), true);
    }

    [Fact]
    internal void GetField_AnonymousField_ThrowArgumentException()
    {
        // arrange 
        var table = _schema.GetTable("campaign_setting");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.GetField("zorro"));

        // assert
        Assert.Equal("Field name 'zorro' does not exist for object type 'campaign_setting'.", ex.Message);
    }

    [Fact]
    internal void GetField_FieldWithDefaultValue_DefaultValue()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);

        // act 
        var result = rcd.GetField("is_group");

        // assert
        Assert.Equal("True", result, true);
    }

    [Fact]
    internal void GetField_PkDefaultValue_0()
    {
        // arrange 
        var table = _schema.GetTable("weapon");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);

        // act 
        var result = rcd.GetField("id");

        // assert
        Assert.Equal("0", result);
    }


    [Fact]
    internal void GetField_NumberFields_DefaultValue()
    {
        // arrange 
        var table = _schema.GetTable(1071);
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);

        // act 
        var result1 = rcd.GetField("id");
        var result2 = rcd.GetField("critical_multiplier_1");
        var result3 = rcd.GetField("martial");
        

        // assert
        Assert.Equal("0", result1);
        Assert.Equal("0", result2);
        Assert.Equal(false.ToString(), result3);
    }

    [Fact]
    internal void IsFieldChange_SetNameField_True()
    {
        // arrange 
        var table = _schema.GetTable("armor");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);
        rcd.SetField("name", _fixture.Create<string>());

        // act 
        var result = rcd.IsFieldChanged("name");

        // assert
        Assert.True(result);
    }


    [Fact]
    internal void IsFieldChange_SetNameField_False()
    {
        // arrange 
        var table = _schema.GetTable("armor");
        Assert.NotNull(table);
        var rcd1 = new Record(table, new string?[table.Fields.Length + 1]);
        rcd1.SetField("name", _fixture.Create<string>());
        var rcd2 = new Record(table, new string?[table.Fields.Length + 1]);

        // act 
        var result1 = rcd1.IsFieldChanged("id");
        var result2 = rcd1.IsFieldChanged("heavy");
        var result3 = rcd2.IsFieldChanged("heavy");

        // assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    internal void IsFieldChange_AnonymousField_ThrowArgumentException()
    {
        // arrange 
        var table = _schema.GetTable("armor");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.IsFieldChanged("zorro"));

        // assert
        Assert.Equal("Field name 'zorro' does not exist for object type 'armor'.", ex.Message);
    }

    [Fact]
    internal void IsFieldChange_AnonymousField_ThrowUnknownRecordType()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.IsFieldChanged("zorro"));

        // assert
        Assert.Equal("This Record object has an unknown RecordType.  The RecordType \nproperty must be set before performing this operation.", ex.Message);
    }

}

