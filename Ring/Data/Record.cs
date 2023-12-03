using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Data;

public struct Record : IEquatable<Record>
{
    private readonly static string NullField = @"^^";
    private readonly static string NullString = @"Null";
    private readonly static char HashFieldDelimiter = (char)3; // end of text character
    private readonly static CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private readonly static string BooleanTrue = true.ToString(DefaultCulture);
    private readonly static string BooleanFalse = false.ToString(DefaultCulture);
    private readonly static decimal MaxLongValue = long.MaxValue;
    private readonly static decimal MinLongValue = long.MinValue;
    private readonly static decimal MaxIntValue = int.MaxValue;
    private readonly static decimal MinIntValue = int.MinValue;
    private readonly static decimal MaxShortValue = short.MaxValue;
    private readonly static decimal MinShortValue = short.MinValue;
    private readonly static decimal MaxByteValue = sbyte.MaxValue;
    private readonly static decimal MinByteValue = sbyte.MinValue;
    private readonly static Dictionary<byte, char[]> DateTimeTemplates = new()
    {
      { (byte)FieldType.ShortDateTime, new char[] {'0','0','0','0','-','0','0','-','0','0' } },
      { (byte)FieldType.DateTime, new char[] {'0','0','0','0','-','0','0','-','0','0','T','0','0',':','0','0',':','0','0','.','0','0','0','Z' } },
      { (byte)FieldType.LongDateTime, new char[] {'0','0','0','0','-','0','0','-','0','0','T','0','0',':',
          '0','0',':','0','0','.','0','0','0','0','0','0','+','0','0',':','0','0' } }
    };

    // should be instanciate when record type is defined
    // _data.Lenght should be > _type.Fields.Length
    private string?[]? _data; 
    private Table? _type;

    /// <summary>
    ///     Ctor
    /// </summary>
    public Record()
    {
        _type = null;
        _data = null;
    }
    internal Record(Table type)
    {
        _type = type;
        _data = new string? [type.Fields.Length+1];
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
    internal readonly string? this[int i]  { get { return _data[i];} set { _data[i]=value; }}
#pragma warning restore CS8602
    public readonly bool IsDirty => _data != null && _data[^1] != null;
    internal readonly Table? Table => _type;
    internal readonly void ClearData() => Array.Fill(_data??Array.Empty<string?>(), null);

    /// <summary>
    ///     GetField methods
    /// </summary>
    public readonly string? GetField(string name)
    {
        if (_type==null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        if (fieldId>-1) return _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        ThrowRecordUnkownFieldName(name);
        return null;
    }

    /// <summary>
    ///     Set field value 
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="value">field value</param>
    public readonly void SetField(string name, string? value)
    {
        if (_type==null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId==-1) ThrowRecordUnkownFieldName(name);
        switch (_type.Fields[fieldId].Type)
        {
            case FieldType.String: SetStringField(fieldId, value); break;
            case FieldType.Byte:
            case FieldType.Short:
            case FieldType.Int:
            case FieldType.Long: SetIntegerField(_type.Fields[fieldId].Type, fieldId, value); break;
            case FieldType.Float:
            case FieldType.Double: SetFloatField(_type.Fields[fieldId].Type, fieldId, value); break;
            case FieldType.ShortDateTime:
            case FieldType.DateTime:
            case FieldType.LongDateTime: SetDateTimeField(fieldId, _type.Fields[fieldId].Type,value); break;
            case FieldType.Boolean: SetBooleanField(fieldId, value); break;
            case FieldType.ByteArray: throw new NotImplementedException();
        }
    }

    public readonly void SetField(string name, long value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        var type = _type.Fields[fieldId].Type;
        switch (type)
        {
            case FieldType.Long: 
                SetData(fieldId, value.ToString(DefaultCulture));
                break;
            case FieldType.Int:
                if (value<=int.MaxValue && value>=int.MinValue) SetData(fieldId, value.ToString(DefaultCulture));
                else ThrowValueTooLarge(type);
                break;
            case FieldType.Short:
                if (value<=short.MaxValue && value>=short.MinValue) SetData(fieldId, value.ToString(DefaultCulture));
                else ThrowValueTooLarge(type);
                break;
            case FieldType.Byte:
                if (value<=sbyte.MaxValue && value>=sbyte.MinValue) SetData(fieldId, value.ToString(DefaultCulture));
                else ThrowValueTooLarge(type);
                break;
            case FieldType.Float:
            case FieldType.Double:
                SetFloatField(type, fieldId, value.ToString(DefaultCulture)); 
                break;
            default:
                // throw exception !!
                break; 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetField(string name, int value) => SetField(name, (long)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetField(string name, short value) => SetField(name, (long)value);
    public readonly void SetField(string name, sbyte value) => SetField(name, (long)value);
    public readonly void SetField(string name, bool value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        if (_type.Fields[fieldId].Type == FieldType.Boolean) SetData(fieldId, value ? BooleanTrue : BooleanFalse);
        else { // throw exception
        }
    }
    public readonly void SetField(string name, DateTime value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value);
    }
    public readonly void SetField(string name, double value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        SetData(fieldId, value.ToString(DefaultCulture));
    }
    public readonly void SetField(string name, float value) => SetField(name,(double)value);

    public static bool operator==(Record left, Record right) => left.Equals(right);
    public static bool operator!=(Record left, Record right) => !(left==right);
    public readonly bool Equals(Record other)
    {
        if (ReferenceEquals(_type, other._type)) {
            if (_data!=null) {
                var i=0;
                while (i<_data.Length)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference. --> other._data cannot be null here
                    if (!string.Equals(_data[i], other._data[i], StringComparison.Ordinal)) return false;
#pragma warning restore CS8602 
                    ++i;
                }
            }
            return true;
        }
        return false;
    }
    public override readonly bool Equals(object? obj) => obj != null && obj is Record record && Equals(record);
    public override readonly int GetHashCode()
    {
        var result = new StringBuilder();
        var fieldCount = _type?.Fields.Length ?? 0;
        if (_type != null) result.Append(_type.PhysicalName);
        if (_data != null)
        {
            for (var i = 0; i < fieldCount; ++i)
            {
                result.Append(_data[i] ?? NullField);
                result.Append(HashFieldDelimiter);
            }
        } 
        return HashHelper.Djb2X(result.ToString());
    }

    internal readonly bool IsFieldChanged(string name)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var index = _type.GetFieldIndex(name);
#pragma warning restore CS8604 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference. - _type cannot be null here !!!
        if (index != -1) return _data[^1]!=null && FieldChange(index);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        ThrowRecordUnkownFieldName(name);
        return false;
    }
    internal readonly bool IsFieldExist(string name) => _type != null && _type.GetFieldIndex(name)!=-1;

    #region private methods 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetStringField(int fieldId, string? value)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference; _type cannot be null here !!
        if (value==null) SetData(fieldId, null);
        else if (value.Length <= _type.Fields[fieldId].Size) SetData(fieldId, value);
        else SetData(fieldId, value.Truncate(_type.Fields[fieldId].Size)); // truncate or exception ??
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetIntegerField(FieldType fieldType, int fieldId, string? value) {
        if (value==null) SetData(fieldId, null);
        else if (!value.IsNumber()) ThrowWrongStringFormat();
        else if (value.Length>20) ThrowValueTooLarge(fieldType);
        else if (decimal.TryParse(value, out decimal dcm))
        {
            if ((fieldType == FieldType.Long && dcm <= MaxLongValue && dcm >= MinLongValue) ||
                (fieldType == FieldType.Int && dcm <= MaxIntValue && dcm >= MinIntValue) ||
                (fieldType == FieldType.Short && dcm <= MaxShortValue && dcm >= MinShortValue) ||
                (fieldType == FieldType.Byte && dcm <= MaxByteValue && dcm >= MinByteValue))
            {
                SetData(fieldId, dcm.ToString(DefaultCulture));
                return;
            }
        }
        ThrowValueTooLarge(fieldType);
    }

    private readonly void SetFloatField(FieldType fieldType, int fieldId, string? value)
    {
        if (value==null) SetData(fieldId, null);
        else 
        { 
            value = value.Replace(',', '.');
            if (value.IsFloat())
            {
                if (fieldType==FieldType.Double &&  double.TryParse(value, out double dbl)) 
                    SetData(fieldId, dbl.ToString(DefaultCulture));
                else if (fieldType == FieldType.Float && float.TryParse(value, out float flt))
                    SetData(fieldId, flt.ToString(DefaultCulture));
                else ThrowValueTooLarge(fieldType);
                return;
            }
        }
        ThrowWrongStringFormat();
    }

    private readonly void SetBooleanField(int fieldId, string? value)
    {
        if (value==null) SetData(fieldId, null);
        else if (bool.TryParse(value, out bool result)) SetData(fieldId, result? BooleanTrue: BooleanFalse);
        else ThrowWrongBooleanValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetDateTimeField(int fieldId, FieldType fieldType, string? value)
    {
        if (DateTime.TryParse(value,out var dt)) SetDateTimeField(fieldId, fieldType, dt);
        // throw exception 
    }

    private readonly void SetDateTimeField(int fieldId, FieldType fieldType, DateTime value)
    {
        if (fieldType == FieldType.DateTime || fieldType == FieldType.LongDateTime || fieldType == FieldType.ShortDateTime)
        {
            // IS0-8601 ==> "YYYY-MM-DDTHH:MM:SS.mmmZ" eg. 2005-12-12T18:17:16.015+04:00; lenght max ==> 30
            var template=DateTimeTemplates[(byte)fieldType];
            var count=template.Length;
            var result=new char[count];
            var dateToConv = fieldType==FieldType.DateTime?value.ToUniversalTime():value;
            Array.Copy(template,result,count);
            SetDateTime(result,4,dateToConv.Year,3);
            SetDateTime(result,2,dateToConv.Month,6);
            SetDateTime(result,2,dateToConv.Day,9);
            if (fieldType!=FieldType.ShortDateTime)
            {
                SetDateTime(result,2,dateToConv.Hour,12);
                SetDateTime(result,2,dateToConv.Minute,15);
                SetDateTime(result,2,dateToConv.Second,18);
                SetDateTime(result,3,dateToConv.Millisecond,22);
                if (fieldType==FieldType.LongDateTime)
                {
                    throw new NotImplementedException();
                }
            }
            SetData(fieldId, new string(result));
            return;
        }
        //throw exception
    }

    private static void SetDateTime(char[] input, int size, int value, int lastPosition)
    {
        var decimalSys=10;
        input[lastPosition--] += (char)(value%decimalSys); value/=decimalSys;
        input[lastPosition--] += (char)(value%decimalSys); value/=decimalSys;
        if (size<3) return;
        input[lastPosition--] += (char)(value%decimalSys); value/=decimalSys;
        if (size<4) return;
        input[lastPosition--] += (char)(value%decimalSys);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetData(int fieldId, string? value)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference. - _data cannot be null here !!!
        if (string.CompareOrdinal(_data[fieldId],value)==0) return; // detect no change
#pragma warning restore CS8602
        if (_data[^1]==null)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference. - _type cannot be null here !!!
            var index=(_type.Fields.Length+_type.Relations.Length) >>4; 
#pragma warning restore CS8602
            _data[^1]=new string(new char[index+1]); 
        }
#pragma warning disable CS8604 // Possible null reference argument. - _data cannot be null here !!!
        _data[^1].SetBitValue(fieldId);
#pragma warning restore CS8604 // Possible null reference argument.
        _data[fieldId] = value;
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference. - _data cannot be null here !!!
#pragma warning disable CS8604 // Possible null reference argument. - _data[^1] cannot be null here !!!
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool FieldChange(int fieldId) => _data[^1].GetBitValue(fieldId);
#pragma warning restore CS8602 // Possible null reference argument.
#pragma warning restore CS8604 // Possible null reference argument.

    // exceptions 
    private readonly void ThrowRecordUnkownFieldName(string name) => 
        throw new ArgumentException(string.Format(DefaultCulture,
                  ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownFieldName), name, _type?.Name));

    private static void ThrowRecordUnkownRecordType() =>
        throw new ArgumentException(ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRecordType));

    private static void ThrowWrongStringFormat() =>
        throw new FormatException(ResourceHelper.GetErrorMessage(ResourceType.RecordWrongStringFormat));

    private static void ThrowValueTooLarge(FieldType fieldType) =>
        throw new OverflowException(string.Format(DefaultCulture, 
            ResourceHelper.GetErrorMessage(ResourceType.RecordValueTooLarge), fieldType.RecordTypeDisplay()));

    private static void ThrowWrongBooleanValue(string? value) =>
        throw new FormatException(string.Format(DefaultCulture,
            ResourceHelper.GetErrorMessage(ResourceType.RecordWrongBooleanValue), value?? NullString));

    #endregion

}
