using Ring.Tests.Schema.Extensions;
using Record = Ring.Data.Record;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using AutoFixture;
using Ring.Schema.Builders;
using System.Globalization;

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
    public void Equal_EmptyRecord_True()
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
    public void Equal_Null_False()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var result = rcd.Equals(null);

        // assert
        Assert.False(result);
    }

    [Fact]
    public void Equal_FeatRecord_True()
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
    public void Equal_RaceRecord_False()
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
    public void Equal_DifferentRecordType_False()
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
    public void Equal_DifferentRecordType_True()
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
    public void Equal_DifferentRecordTypeWithNull_False()
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
    public void GetHashCode_DifferentRecordTypeWithNull_NotEquals()
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
    public void GetHashCode_DifferentRecordTypeWithNull_Equals()
    {
        // arrange 
        var table = _schema.GetTable("gender");
        Assert.NotNull(table);
        var data = new string?[table.Fields.Length];
        data[1] = _fixture.Create<string>();
        var rcd1 = new Record(table, data);
        var rcd2 = new Record(table, data);

        // act 
        var result1 = rcd1.GetHashCode();
        var result2 = rcd2.GetHashCode();

        // assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void SetField_AnonymousValue_RecordUnkownRecordType()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.SetField(_fixture.Create<string>(), null));

        // assert
        Assert.Equal("This Record object has an unknown RecordType.  The RecordType \nproperty must be set before performing this operation.", ex.Message);
    }

    [Fact]
    public void SetField_AnonymousValue_ThrowArgumentException()
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
    public void SetField_AnonymousString_ReturnSameString()
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
    public void SetField_AnonymousShort_ReturnShortValue()
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
    public void SetField_AnonymousInt_ReturnIntegerValue()
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
    public void SetField_AnonymousStringByte_ReturnByteValue()
    {
        // arrange 
        var table = _schema.GetTable("class");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);
        var expectedValue = _fixture.Create<sbyte>().ToString();

        // act 
        rcd.SetField("fortitude", expectedValue);

        // assert
        Assert.Equal(expectedValue, rcd.GetField("fortitude"));
    }

    [Fact]
    public void SetField_AnonymousByte_ReturnByteValue()
    {
        // arrange 
        var table = _schema.GetTable("class");
        Assert.NotNull(table);
        var rcd = new Record(table, new string?[table.Fields.Length + 1]);
        var expectedValue = _fixture.Create<sbyte>();

        // act 
        rcd.SetField("hit_die", expectedValue);

        // assert
        Assert.Equal(expectedValue.ToString(), rcd.GetField("hit_die"));
    }

    [Fact]
    public void SetField_AnonymousInt_ThrowValueTooLarge()
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
    public void SetField_AnonymousLong_ThrowValueTooLarge()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var metaIdTable = tableBuilder.GetMetaId("Test", DatabaseProvider.SqlLite);
        var rcd = new Record(metaIdTable);

        // act 
        var ex = Assert.Throws<OverflowException>(() => rcd.SetField("value", "1000000000000000000000000000000000000"));

        // assert
        Assert.Equal("Value was either too large or too small for an Int64.", ex.Message);
    }


    [Fact]
    public void SetField_AnonymousInt_ThrowWrongStringFormat()
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
    public void SetField_AnonymousShort_ReturnShort()
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
    public void SetField_AnonymousInt_ReturnInt()
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
    public void SetField_AnonymousLong_ReturnLong()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var metaIdTable = tableBuilder.GetMetaId("Test",DatabaseProvider.SqlLite);
        var rcd = new Record(metaIdTable);
        var expectedValue = long.MinValue;

        // act 
        rcd.SetField("value", expectedValue);

        // assert
        Assert.Equal(expectedValue.ToString(), rcd.GetField("value"));
    }

    [Fact]
    public void SetField_MaxLong_ThrowValueTooLarge()
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
    public void SetField_MaxInt_ThrowValueTooLarge()
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
    public void SetField_Bool_True()
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
    public void SetField_Bool_False()
    {
        // arrange 
        var featTable = _schema.GetTable("feat");
        Assert.NotNull(featTable);
        var rcd = new Record(featTable);

        // act 
        rcd.SetField("metamagic", false);

        // assert
        Assert.Equal("False", rcd.GetField("metamagic"), true);
    }

    [Fact]
    public void SetField_Bool_ThrowWrongBooleanValue()
    {
        // arrange 
        var table = _schema.GetTable("feat");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        var ex = Assert.Throws<FormatException>(() => rcd.SetField("effects_stack", "yes man"));

        // assert
        Assert.Equal("'yes man' was not recognized as a valid Boolean.", ex.Message);
    }

    [Fact]
    public void SetField_BoolString_True()
    {
        // arrange 
        var table = _schema.GetTable("feat");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.SetField("metamagic", "true");

        // assert
        Assert.Equal("True", rcd.GetField("metamagic"), true);
    }

    [Fact]
    public void SetField_NullBoolString_False()
    {
        // arrange 
        var table = _schema.GetTable("feat");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.SetField("epic", null);

        // assert
        Assert.Equal("false", rcd.GetField("epic"), true);
    }

    [Fact]
    public void SetField_ShortDateTime1_ReturnIso8601Format()
    {
        // arrange 
        var table = _schema.GetTable("book");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.SetField("publish_date", new DateTime(2001,1,1,18,18,18));

        // assert
        Assert.Equal("2001-01-01", rcd.GetField("publish_date"));
    }

    [Fact]
    public void SetField_ShortDateTime2_ReturnIso8601Format()
    {
        // arrange 
        var table = _schema.GetTable("book");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.SetField("publish_date", new DateTime(1, 10, 27, 18, 18, 18));

        // assert
        Assert.Equal("0001-10-27", rcd.GetField("publish_date"));
    }

    [Fact]
    public void SetField_DateTime_ReturnIso8601Format()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.SqlLite);
        var rcd = new Record(logTable);
        var dt = DateTime.ParseExact("2005-12-12T18:17:16.015+04:00", "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);

        // act - 2001, 1, 1, 18, 18, 18, 458
        rcd.SetField("entry_time", dt);

        // assert
        Assert.Equal("2005-12-12T14:17:16.015Z", rcd.GetField("entry_time"));
    }

    [Fact]
    public void SetField_MaxDateTime_ReturnIso8601Format()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.SqlLite);
        var rcd = new Record(logTable);
        var dt = DateTime.MaxValue;

        // act - 2001, 1, 1, 18, 18, 18, 458
        rcd.SetField("entry_time", dt);

        // assert
        Assert.Equal("9999-12-31T22:59:59.999Z", rcd.GetField("entry_time"));
    }

    [Fact]
    public void SetField_MinDateTime_ReturnIso8601Format()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.SqlLite);
        var rcd = new Record(logTable);
        var dt = DateTime.MinValue;

        // act - 2001, 1, 1, 18, 18, 18, 458
        rcd.SetField("entry_time", dt);

        // assert
        Assert.Equal("0001-01-01T00:00:00.000Z", rcd.GetField("entry_time"));
    }

    [Fact]
    public void SetField_LongDateTime_ReturnIso8601Format()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.LongDateTime);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField;
        var rcd = new Record(logTable);
        
        //var dt = DateTime.ParseExact("2005-12-12T18:17:16.015116+04:00", "yyyy-MM-ddTHH:mm:ss.ffffffzzz", CultureInfo.InvariantCulture);

        // act - 2001, 1, 1, 18, 18, 18, 458
        //rcd.SetField("entry_time", dt);
        //var test = rcd.GetField("entry_time");
        // assert
        //Assert.Equal("2005-12-12T14:17:16.015Z", rcd.GetField("entry_time"));
    }

    [Fact]
    public void SetField_Double1_ReturnDoubleValue()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.Double);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);
        
        // act 
        rcd.SetField("entry_time", "-789456123,0123");
        
        // assert
        Assert.Equal("-789456123.0123", rcd.GetField("entry_time"));
    }

    [Fact]
    public void SetField_Double2_ReturnDoubleValue()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.Double);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);
        var dbl = 0.456D;

        // act 
        rcd.SetField("entry_time", dbl);

        // assert
        Assert.Equal("0.456", rcd.GetField("entry_time"));
    }

    [Fact]
    public void GetField_AnonymousField_ThrowArgumentException()
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
    public void GetField_FieldWithDefaultValue_DefaultValue()
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
    public void GetField_PkDefaultValue_0()
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
    public void GetField_NumberFields_DefaultValue()
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
    public void IsFieldChange_SetNameField_True()
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
    public void IsFieldChange_SetNameField_False()
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
    public void IsFieldChange_AnonymousField_ThrowArgumentException()
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
    public void IsFieldChange_AnonymousField_ThrowUnknownRecordType()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.IsFieldChanged("zorro"));

        // assert
        Assert.Equal("This Record object has an unknown RecordType.  The RecordType \nproperty must be set before performing this operation.", ex.Message);
    }

    [Theory]
    [InlineData("weapon", "name", true)]
    [InlineData("rule", "try_again", false)]
    [InlineData("rule", "short_name", true)]
    [InlineData("gender", "iso_code", true)]
    public void IsFieldExist_AnonymousField_Result(string tableName, string field, bool expectedResult)
    {
        // arrange 
        var table = _schema.GetTable(tableName);
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        var result = rcd.IsFieldExist(field);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsFieldExist_NullField_False()
    {
        // arrange 
        var rcd = new Record();

        // act 
        var result = rcd.IsFieldExist("testy");

        // assert
        Assert.False(result);
    }


    [Fact]
    public void ClearData_AnonymousRecord_DataResetToNull()
    {
        // arrange 
        var table = _schema.GetTable(1071); // weapon
        Assert.NotNull(table);
        var rcd1 = new Record(table); // filled record 
        rcd1.SetField("name", _fixture.Create<string>());
        rcd1.SetField("unarmed_attack", _fixture.Create<bool>());
        rcd1.SetField("ammo", _fixture.Create<bool>());
        rcd1.SetField("light_melee", null);
        rcd1.SetField("one_handed_melee", _fixture.Create<bool>());
        rcd1.SetField("two_handed_melee", _fixture.Create<bool>());
        rcd1.SetField("ranged", _fixture.Create<bool>());
        rcd1.SetField("range_increment", _fixture.Create<short>());
        rcd1.SetField("critical_range", _fixture.Create<short>());
        rcd1.SetField("critical_multiplier_1", _fixture.Create<short>());
        rcd1.SetField("critical_multiplier_2", _fixture.Create<short>());
        var rcd2 = new Record(); // empty record 
        
        // act 
        rcd1.ClearData();
        rcd2.ClearData();

        // assert
        for (var i = 0; i <= rcd1.Table?.Fields.Length; ++i)
        {
            Assert.Null(rcd1[i]);
        }
    }

    

}

