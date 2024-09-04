using Ring.Tests.Schema.Extensions;
using Record = Ring.Data.Record;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using AutoFixture;
using Ring.Schema.Builders;
using System.Globalization;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;

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
        var table = _schema.GetTable("feat");
        Assert.NotNull(table);
        var rcd1 = new Record(table);
        for (var i = 1; i < table.Fields.Length; ++i) rcd1[i] = _fixture.Create<string?>();
        var rcd2 = new Record(table);
        for (var i = 1; i < table.Fields.Length; ++i) rcd2[i] = rcd1[i];

        // act 
        var result = rcd1.Equals(rcd2);

        // assert
        Assert.True(result);
    }

    [Fact]
    public void Equal_RaceRecord_False()
    {
        // arrange 
        var table = _schema.GetTable("race");
        Assert.NotNull(table);
        var rcd1 = new Record(table);
        for (var i = 1; i < table.Fields.Length; ++i) rcd1[i] = _fixture.Create<string?>();
        var rcd2 = new Record(table);

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
        var tableGender = _schema.GetTable("gender");
        var tableRace = _schema.GetTable("race");
        Assert.NotNull(tableGender);
        Assert.NotNull(tableRace);
        var rcd1 = new Record(tableGender);
        var rcd2 = new Record(tableRace);

        // act 
        var result1 = rcd2.Equals((object)rcd1);
        var result2 = rcd1 != rcd2;

        // assert
        Assert.False(result1);
        Assert.True(result2);
    }

    [Fact]
    public void GetHashCode_DifferentRecordEmpty_Equals()
    {
        // arrange 
        var rcd1 = new Record();
        var rcd2 = new Record();

        // act 
        var result1 = rcd1.GetHashCode();
        var result2 = rcd2.GetHashCode();

        // assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetHashCode_DifferentRecordTypeWithNull_NotEquals()
    {
        // arrange 
        var table = _schema.GetTable("gender");
        Assert.NotNull(table);
        var rcd1 = new Record(table);
        var rcd2 = new Record(table);

        // act 
        rcd1.SetField("iso_code", 45);
        var result1 = rcd1.GetHashCode();
        var result2 = rcd2.GetHashCode();

        // assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void GetHashCode_DifferentWayToUpdateData_Equals()
    {
        // arrange 
        var tableGender = _schema.GetTable("gender");
        Assert.NotNull(tableGender);
        var rcd1 = new Record(tableGender); ;
        rcd1[1] = "45";
        var rcd2 = new Record(tableGender);
        rcd2.SetField("iso_code", 45);

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
        var rcd = new Record(table);

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
        var rcd = new Record(table);
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
        var rcd = new Record(table);
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
        var rcd = new Record(table);
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
        var rcd = new Record(table);
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
    public void SetField_MaxLong_ThrowImpossibleConversion()
    {
        // arrange 
        var builder = new TableBuilder();
        var logTable = builder.GetLog("Test", DatabaseProvider.MySql);
        var rcd = new Record(logTable);
        var longValue = long.MaxValue;

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.SetField("entry_time", longValue));

        // assert
        Assert.Equal("Cannot implicitly convert type 'Int64' to 'DateTime'.", ex.Message);
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
    public void SetField_MaxShort_ThrowValueTooLarge()
    {
        // arrange 
        var armorTable = _schema.GetTable("armor");
        Assert.NotNull(armorTable);
        var rcd = new Record(armorTable);
        var expectedValue = short.MaxValue;

        // act 
        var ex = Assert.Throws<OverflowException>(() => rcd.SetField("arcane_spell_failure", expectedValue));

        // assert
        Assert.Equal("Value was either too large or too small for an Int8.", ex.Message);
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
    public void SetField_StringDate1_ExactDate()
    {
        // arrange 
        var table = _schema.GetTable("book");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.SetField("publish_date", "1955-12-27");

        // assert
        Assert.Equal("1955-12-27", rcd.GetField("publish_date"));
    }

    [Fact]
    public void SetField_StringDate2_ExactDate()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.DateTime);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);

        // act 
        rcd.SetField("entry_time", "1914-10-30T18:09:18.123Z");

        // assert
        //Assert.Equal("3.01416", rcd.GetField("entry_time"));
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
    public void SetField_DateTime2_ReturnIso8601Format()
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
    public void SetField_DateTimeOffset_ReturnIso8601Format()
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
    public void SetField_DateTimeNow_ReturnIso8601Format()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.SqlLite);
        var rcd = new Record(logTable);
        var dt = DateTime.UtcNow;

        // act - 2001, 1, 1, 18, 18, 18, 458
        rcd.SetField("entry_time", dt);

        // assert
        Assert.Equal(dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), rcd.GetField("entry_time"));
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
    public void SetField_DateTime1_ReturnIso8601Format()
    {
        // arrange - set dateTime to LongDateTime field
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.LongDateTime);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? logTable.Fields[0];
        var rcd = new Record(logTable);
        var dt = DateTime.ParseExact("1992-09-28T01:02:03.099099", "yyyy-MM-ddTHH:mm:ss.ffffff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        var expectedResult = dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff+00:00");

        // act
        rcd.SetField("entry_time", dt);
        var result1 = rcd.GetField("entry_time");
        rcd.GetField("entry_time", out DateTime? dtResult);

        // assert
        Assert.NotNull(result1);
        Assert.Equal(expectedResult, result1);
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
        logTable.Fields[index] = newField ?? logTable.Fields[0];
        var rcd = new Record(logTable);
        var dt = DateTimeOffset.ParseExact("2005-12-12T18:17:16.015116-07:00", "yyyy-MM-ddTHH:mm:ss.ffffffzzz", CultureInfo.InvariantCulture);
        var expectedResult=@"2005-12-12T18:17:16.0151160-07:00"; 

        // act
        rcd.SetField("entry_time", dt);
        var result = rcd.GetField("entry_time");

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void SetField_Float1_ReturnFloatValue()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.Float);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);

        // act 
        rcd.SetField("entry_time", "3,01416");

        // assert
        Assert.Equal("3.01416", rcd.GetField("entry_time"));
    }

    [Fact]
    public void SetField_Float2_ThrowWrongStringFormat()
    {
        // arrange 
        var table = _schema.GetTable("book");
        Assert.NotNull(table);
        var rcd = new Record(table);
        var ff = 1.54f;

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.SetField("id", ff));

        // assert
        Assert.Equal("Cannot implicitly convert type 'Float' to 'Int32'.", ex.Message);
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
    public void SetField_NullDouble_DefaultDoubleValue()
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
        rcd.SetField("entry_time", null);
        var result= rcd.GetField("entry_time");

        // assert
        Assert.NotNull(result);
        Assert.Equal("0", result);
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
    public void SetField_ByteArray1_ReturnArray()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.ByteArray);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);
        var byteArray = new byte[] { 1,2,3,4,5,6,7,8,9, byte.MinValue, byte.MaxValue };

        // act 
        rcd.SetField("entry_time", byteArray);
        rcd.GetField("entry_time", out byte[]? result);

        // assert
        Assert.NotNull(result);
        Assert.Equal(11,result.Length);
        Assert.Equal(1, result[0]);
        Assert.Equal(2, result[1]);
        Assert.Equal(3, result[2]);
        Assert.Equal(4, result[3]);
        Assert.Equal(5, result[4]);
        Assert.Equal(6, result[5]);
        Assert.Equal(7, result[6]);
        Assert.Equal(8, result[7]);
        Assert.Equal(9, result[8]);
        Assert.Equal(byte.MinValue, result[9]);
        Assert.Equal(byte.MaxValue, result[10]);
    }

    [Fact]
    public void SetField_ByteArray2_Null()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.ByteArray);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);
        var byteArray = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, byte.MinValue, byte.MaxValue };

        // act 
        rcd.SetField("entry_time", null);
        rcd.GetField("entry_time", out byte[]? result);

        // assert
        Assert.Null(result);
    }


    [Fact]
    public void SetField_ByteArray3_ThrowInvalidBase64String()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.ByteArray);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);

        // act 
        var ex = Assert.Throws<FormatException>(() => rcd.SetField("entry_time", "aze"));

        // assert
        Assert.Equal("The input is not a valid Base-64 string.", ex.Message);
    }


    [Fact]
    public void SetField_ByteArray4_ThrowInvalidBase64String()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.MySql);
        var field = logTable.GetField("entry_time");
        var index = logTable.GetFieldIndex("entry_time");
        var meta = field?.ToMeta(99);
        meta?.SetFieldType(FieldType.ByteArray);
        var newField = meta?.ToField();
        logTable.Fields[index] = newField ?? GetAnonymousField();
        var rcd = new Record(logTable);
        var byteArray = new byte[] { _fixture.Create<byte>(), _fixture.Create<byte>(), _fixture.Create<byte>() };
        var base64 = Convert.ToBase64String(byteArray);

        // act 
        rcd.SetField("entry_time", base64);
        rcd.GetField("entry_time", out byte[]? result);

        // assert
        Assert.NotNull(result);
        Assert.Equal(3,result.Length);
        Assert.Equal(byteArray[0], result[0]);
        Assert.Equal(byteArray[1], result[1]);
        Assert.Equal(byteArray[2], result[2]);
    }
            
    [Fact]
    public void GetField_AnonymousField_ThrowArgumentException()
    {
        // arrange 
        var table = _schema.GetTable("campaign_setting");
        Assert.NotNull(table);
        var rcd = new Record(table);

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
        var rcd = new Record(table);

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
        var rcd = new Record(table);

        // act 
        var result = rcd.GetField("id");

        // assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetField_DefaultFieldPk_0()
    {
        // arrange 
        var table = _schema.GetTable("weapon");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        var result = rcd.GetField();

        // assert
        Assert.Equal(0L, result);
    }

    [Fact]
    public void GetField_DefaultFieldPk_77()
    {
        // arrange 
        var table = _schema.GetTable("skill");
        Assert.NotNull(table);
        var rcd = new Record(table);
        rcd.SetField("id", 77L);

        // act 
        var result = rcd.GetField();

        // assert
        Assert.Equal(77L, result);
    }

    [Fact]
    public void GetField_NumberFields_DefaultValue()
    {
        // arrange 
        var table = _schema.GetTable(1071);
        Assert.NotNull(table);
        var rcd = new Record(table);

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
    public void GetField_OutBoolean_DefaultValue()
    {
        // arrange 
        var table = _schema.GetTable("alignment");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.GetField("chaos", out bool? result);

        // assert
        Assert.Equal(false, result);
    }

    [Fact]
    public void GetField_OutBoolean_True()
    {
        // arrange 
        var table = _schema.GetTable("alignment");
        Assert.NotNull(table);
        var rcd = new Record(table);
        rcd.SetField("law", true);

        // act 
        rcd.GetField("law", out bool? result);

        // assert
        Assert.Equal(true, result);
    }

    [Fact]
    public void GetField_OutLong_DefaultValue()
    {
        // arrange 
        var table = _schema.GetTable("alignment");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        rcd.GetField("id", out long? result);

        // assert
        Assert.Equal(0L, result);
    }

    [Fact]
    public void GetField_OutLong_ThrowRecordUnkownFieldName()
    {
        // arrange 
        var table = _schema.GetTable("campaign_setting");
        Assert.NotNull(table);
        var rcd = new Record(table);

        // act 
        var ex = Assert.Throws<ArgumentException>(() => rcd.GetField("789", out long? lngOut));

        // assert
        Assert.Equal("Field name '789' does not exist for object type 'campaign_setting'.", ex.Message);
    }

    [Fact]
    public void GetField_OutDateTime_ShortDateTime()
    {
        // arrange 
        var table = _schema.GetTable("book");
        Assert.NotNull(table);
        var rcd = new Record(table);
        var expectedValue = DateTime.UtcNow;
        rcd.SetField("creation_time", expectedValue);

        // act 
        rcd.GetField("creation_time", out DateTime? result);

        // assert
        Assert.Equal(expectedValue.ToString("yyyyMMdd"), result?.ToString("yyyyMMdd"));
    }

    [Fact]
    public void GetField_OutDateTime_DateTime()
    {
        // arrange 
        var tableBuilder = new TableBuilder();
        var logTable = tableBuilder.GetLog("Test", DatabaseProvider.SqlLite);
        var rcd = new Record(logTable);
        var dt = new DateTime(2005, 12,12,18,17,16,15, DateTimeKind.Utc);
        rcd.SetField("entry_time", dt);

        // act - 2001, 1, 1, 18, 18, 18, 458
        rcd.GetField("entry_time", out DateTime? result);

        // assert
        Assert.NotNull(result);
        Assert.Equal(dt.Year, result.Value.Year);
        Assert.Equal(dt.Month, result.Value.Month);
        Assert.Equal(dt.Day, result.Value.Day);
        Assert.Equal(dt.Hour, result.Value.Hour);
        Assert.Equal(dt.Minute, result.Value.Minute);
        Assert.Equal(dt.Second, result.Value.Second);
        Assert.Equal(dt.Millisecond, result.Value.Millisecond);
    }

    [Fact]
    public void IsFieldChange_SetNameField_True()
    {
        // arrange 
        var table = _schema.GetTable("armor");
        Assert.NotNull(table);
        var rcd = new Record(table);
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
        var rcd1 = new Record(table);
        rcd1.SetField("name", _fixture.Create<string>());
        var rcd2 = new Record(table);

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
        var rcd = new Record(table);

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

    [Fact]
    public void IsDirty_NoChanges_False()
    {
        // arrange 
        var table = _schema.GetTable(1071); // weapon
        Assert.NotNull(table);
        var rcd = new Record(table);
        for (var i = 0; i < table.Fields.Length; i++) rcd[i] = _fixture.Create<string>();

        // act 
        var result = rcd.IsDirty;

        // assert
        Assert.False(result);
    }

    [Fact]
    public void IsDirty_ChangeAmmo_True()
    {
        // arrange 
        var table = _schema.GetTable(1071); // weapon
        Assert.NotNull(table);
        var rcd = new Record(table);
        for (var i = 0; i < table.Fields.Length; i++) rcd[i] = _fixture.Create<string>();

        // act 
        rcd.SetField("ammo", true);
        var result = rcd.IsDirty;

        // assert
        Assert.True(result);
    }

    [Fact]
    public void IsDirty_EmptyRecord_False()
    {
        // arrange 
        var rcd = new Record();
        // act 
        var result = rcd.IsDirty;

        // assert
        Assert.False(result);
    }


    [Fact]
    public void IsDirty_ChangeWithSameValue_False()
    {
        // arrange 
        var table = _schema.GetTable(1071); // weapon
        Assert.NotNull(table);
        var rcd = new Record(table);
        for (var i = 0; i < table.Fields.Length; i++) rcd[i] = _fixture.Create<string>();

        // act 
        rcd.SetField("name", rcd.GetField("name"));
        var result = rcd.IsDirty;

        // assert
        Assert.False(result);
    }

    [Fact]
    public void GetRelation_Deity2gender_8989()
    {
        // arrange 
        var tableDeity = _schema.GetTable("deity");
        Assert.NotNull(tableDeity);
        var rcd = new Record(tableDeity);
        rcd.SetField("symbol", "tests");
        var expectedValue = "8989";
        rcd[6] = expectedValue;

        // act 
        var result = rcd.GetRelation("deity2gender");

        // assert
        Assert.Equal(expectedValue, result.ToString());
    }

    [Fact]
    public void GetRelation_Deity2alignment_9797()
    {
        // arrange 
        var tableDeity = _schema.GetTable("deity");
        Assert.NotNull(tableDeity);
        var rcd = new Record(tableDeity);
        rcd.SetField("symbol", "tests");
        var expectedValue = "9797";
        rcd[5] = expectedValue;

        // act 
        var result = rcd.GetRelation("deity2alignment");

        // assert
        Assert.Equal(expectedValue, result.ToString());
    }

}

