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
    private readonly static string DefaultPrimaryKeyValue = @"0";
    private readonly static char HashFieldDelimiter = (char)3; // end of text character
    private readonly static CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private readonly static NumberStyles DefaultNumberStyle = NumberStyles.Integer;
    private readonly static NumberStyles DefaultFloatStyle = NumberStyles.AllowDecimalPoint | NumberStyles.Float;
    private readonly static string BooleanTrue = true.ToString(DefaultCulture);
    private readonly static string BooleanFalse = false.ToString(DefaultCulture);
    private readonly static decimal MaxIntValue = int.MaxValue;
    private readonly static decimal MinIntValue = int.MinValue;
    private readonly static decimal MaxShortValue = short.MaxValue;
    private readonly static decimal MinShortValue = short.MinValue;
    private readonly static decimal MaxByteValue = sbyte.MaxValue;
    private readonly static decimal MinByteValue = sbyte.MinValue;

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
        _data = new string?[type.Fields.Length + 1];
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
    internal readonly string? this[int i] { get { return _data[i]; } set { _data[i] = value; } }
#pragma warning restore CS8602
    public readonly bool IsDirty => _data != null && _data[^1] != null;
    internal readonly Table? Table => _type;
    internal readonly void ClearData() => Array.Fill(_data ?? Array.Empty<string?>(), null);

    /// <summary>
    ///     Get primary key value (Field name ID)
    /// </summary>
    internal readonly long GetField()
#pragma warning disable CS8602 // Dereference of a possibly null reference. _type cannot be null here 
        => long.Parse(_data[_type.Mapper[0]] ?? DefaultPrimaryKeyValue, DefaultCulture);
#pragma warning restore CS8602

    /// <summary>
    ///     GetField methods
    /// </summary>
    public readonly string? GetField(string name)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        if (fieldId > -1) return _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        ThrowRecordUnkownFieldName(name);
        return null;
    }

    public readonly void GetField(string name, out bool? value)
    {
        value = null;
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId <= -1) ThrowRecordUnkownFieldName(name);
        var field = _type.Fields[fieldId];
        if (field.Type != FieldType.Boolean) ThrowImpossibleConversion(field.Type, FieldType.Boolean);
        //BooleanTrue: BooleanFalse
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        var result = _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        if (BooleanTrue.Equals(result, StringComparison.Ordinal)) value = true;
        else if (BooleanFalse.Equals(result, StringComparison.Ordinal)) value = false;
    }

    public readonly void GetField(string name, out byte[]? value)
    {
        value = null;
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId <= -1) ThrowRecordUnkownFieldName(name);
        var field = _type.Fields[fieldId];
        if (field.Type != FieldType.ByteArray) ThrowImpossibleConversion(field.Type, FieldType.Boolean);
        //BooleanTrue: BooleanFalse
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        var result = _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        if (result != null) value = Convert.FromBase64String(result);
    }

    public readonly void GetField(string name, out long? value)
    {
        value = null;
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId <= -1) ThrowRecordUnkownFieldName(name);
        var field = _type.Fields[fieldId];
        if (field.Type != FieldType.Byte && field.Type != FieldType.Short && field.Type != FieldType.Int && field.Type != FieldType.Long)
            ThrowImpossibleConversion(field.Type, FieldType.Long);
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        var result = _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        if (result != null) value = long.Parse(result, DefaultCulture);
    }

    /// <summary>
    /// Get UTC date/time
    /// </summary>
    public readonly void GetField(string name, out DateTime? value)
    {
        value = null;
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId <= -1) ThrowRecordUnkownFieldName(name);
        var field = _type.Fields[fieldId];
        if (field.Type != FieldType.DateTime && field.Type != FieldType.LongDateTime && field.Type != FieldType.ShortDateTime)
            ThrowImpossibleConversion(field.Type, FieldType.DateTime);
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        var result = _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        if (result == null) return;
        var year = int.Parse(result[..4], DefaultCulture);
        var month = int.Parse(result.AsSpan(5, 2), DefaultNumberStyle, DefaultCulture);
        var day = int.Parse(result.AsSpan(8, 2), DefaultNumberStyle, DefaultCulture);
        if (field.Type == FieldType.ShortDateTime) value = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        else
        {
            var hour = int.Parse(result.AsSpan(11, 2), DefaultNumberStyle, DefaultCulture);
            var minute = int.Parse(result.AsSpan(14, 2), DefaultNumberStyle, DefaultCulture);
            var second = int.Parse(result.AsSpan(17, 2), DefaultNumberStyle, DefaultCulture);
            var milliSecond = int.Parse(result.AsSpan(20, 3), DefaultNumberStyle, DefaultCulture);
            if (field.Type == FieldType.DateTime) value = new DateTime(year, month, day, hour, minute, second, milliSecond, DateTimeKind.Utc);
        }
    }

    /// <summary>
    ///     Set field value 
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="value">field value</param>
    public readonly void SetField(string name, string? value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        var type = _type.Fields[fieldId].Type;
        switch (type)
        {
            case FieldType.String: SetStringField(fieldId, value); break;
            case FieldType.Byte:
            case FieldType.Short:
            case FieldType.Int:
            case FieldType.Long: SetIntegerField(fieldId, type, value); break;
            case FieldType.Float:
            case FieldType.Double: SetFloatField(type, fieldId, value); break;
            case FieldType.ShortDateTime:
            case FieldType.DateTime:
            case FieldType.LongDateTime: SetDateTimeField(fieldId, type, value); break;
            case FieldType.Boolean: SetBooleanField(fieldId, value); break;
            case FieldType.ByteArray: SetByteArrayField(fieldId, value); break;
        }
    }

    internal readonly void SetField(string name, long value, FieldType fieldType)
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
                if (value <= int.MaxValue && value >= int.MinValue) SetData(fieldId, value.ToString(DefaultCulture));
                else ThrowValueTooLarge(type);
                break;
            case FieldType.Short:
                if (value <= short.MaxValue && value >= short.MinValue) SetData(fieldId, value.ToString(DefaultCulture));
                else ThrowValueTooLarge(type);
                break;
            case FieldType.Byte:
                if (value <= sbyte.MaxValue && value >= sbyte.MinValue) SetData(fieldId, value.ToString(DefaultCulture));
                else ThrowValueTooLarge(type);
                break;
            case FieldType.Float:
            case FieldType.Double:
                SetFloatField(type, fieldId, value.ToString(DefaultCulture));
                break;
            default:
                ThrowImpossibleConversion(fieldType, type);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetField(string name, long value) => SetField(name, value, FieldType.Long);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetField(string name, int value) => SetField(name, value, FieldType.Int);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetField(string name, short value) => SetField(name, value, FieldType.Short);
    public readonly void SetField(string name, sbyte value) => SetField(name, value, FieldType.Byte);
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
        SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value, null);
    }
    public readonly void SetField(string name, DateTimeOffset value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value.DateTime, value.Offset);
    }
    public readonly void SetField(string name, double value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        var fieldType = _type.Fields[fieldId].Type;
        if (fieldType != FieldType.Float && fieldType != FieldType.Double) ThrowImpossibleConversion(FieldType.Double, fieldType);
        SetData(fieldId, value.ToString(DefaultCulture));
    }
    public readonly void SetField(string name, float value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        var fieldType = _type.Fields[fieldId].Type;
        if (fieldType != FieldType.Float && fieldType != FieldType.Double) ThrowImpossibleConversion(FieldType.Float, fieldType);
        SetData(fieldId, value.ToString(DefaultCulture));
    }
    public readonly void SetField<T>(string name, T value) where T : IEnumerable<byte>
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        var fieldType = _type.Fields[fieldId].Type;
        if (fieldType != FieldType.ByteArray) ThrowImpossibleConversion(FieldType.ByteArray, fieldType);
        SetData(fieldId, Convert.ToBase64String(value.ToArray()));
    }

    public static bool operator ==(Record left, Record right) => left.Equals(right);
    public static bool operator !=(Record left, Record right) => !(left == right);
    public readonly bool Equals(Record other)
    {
        if (ReferenceEquals(_type, other._type)) {
            if (_data != null) {
                var i = 0;
                while (i < _data.Length)
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
            var i=0;
            while (i<fieldCount)
            {
                result.Append(_data[i] ?? NullField);
                result.Append(HashFieldDelimiter);
                ++i;
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
        if (index != -1) return _data[^1] != null && FieldChange(index);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        ThrowRecordUnkownFieldName(name);
        return false;
    }
    internal readonly bool IsFieldExist(string name) => _type != null && _type.GetFieldIndex(name) != -1;

    #region private methods 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetStringField(int fieldId, string? value)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference; _type cannot be null here !!
        if (value==null) SetData(fieldId, null);
        else if (value.Length <= _type.Fields[fieldId].Size) SetData(fieldId, value);
        else SetData(fieldId, value.Truncate(_type.Fields[fieldId].Size)); // truncate or exception ??
#pragma warning restore CS8602
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetIntegerField(int fieldId, FieldType numberType, string? value) {
        if (value==null) SetData(fieldId, null);
        else if (!value.IsNumber()) ThrowWrongStringFormat();
        else if (long.TryParse(value, DefaultNumberStyle , DefaultCulture, out long lng))
        {
            if ((numberType == FieldType.Int && lng <= MaxIntValue && lng >= MinIntValue) ||
                (numberType == FieldType.Short && lng <= MaxShortValue && lng >= MinShortValue) ||
                (numberType == FieldType.Byte && lng <= MaxByteValue && lng >= MinByteValue))
            {
                SetData(fieldId, lng.ToString(DefaultCulture));
                return;
            }
        } 
        ThrowValueTooLarge(numberType);
    }

    private readonly void SetFloatField(FieldType fieldType, int fieldId, string? value)
    {
        if (value==null) SetData(fieldId, null);
        else
        {
            if (value.Contains(',')) value = value.Replace(',', '.');
            if (value.IsFloat())
            {
                if (fieldType == FieldType.Double && double.TryParse(value, DefaultFloatStyle, DefaultCulture, out double dbl))
                    SetData(fieldId, dbl.ToString(DefaultCulture));
                else if (fieldType == FieldType.Float && float.TryParse(value, DefaultFloatStyle, DefaultCulture, out float flt))
                    SetData(fieldId, flt.ToString(DefaultCulture));
                else ThrowValueTooLarge(fieldType);
                return;
            }
            ThrowWrongStringFormat();
        }
    }

    private readonly void SetByteArrayField(int fieldId, string? value)
    {
        if (value==null) SetData(fieldId, null);
        else if (value.IsBase64String()) SetData(fieldId, value);
        else ThrowInvalidBase64String();
    }

    private readonly void SetBooleanField(int fieldId, string? value)
    {
        if (value==null) SetData(fieldId, null);
        else if (bool.TryParse(value, out bool result)) SetData(fieldId, result ? BooleanTrue : BooleanFalse);
        else ThrowWrongBooleanValue(value);
    }

    private readonly void SetDateTimeField(int fieldId, FieldType fieldType, string? value)
    {
        if (value==null) SetData(fieldId, null);
        else {
            var dateTimeOffset = value.ParseIso8601Date();
            SetDateTimeField(fieldId, fieldType, dateTimeOffset.DateTime, dateTimeOffset.Offset);
        }
    }

    private readonly void SetDateTimeField(int fieldId, FieldType fieldType, DateTime value, TimeSpan? offset)
    {
        if (fieldType == FieldType.DateTime || fieldType == FieldType.LongDateTime || fieldType == FieldType.ShortDateTime)
            SetData(fieldId, new string(value.ToString(fieldType, offset)));
        else ThrowImpossibleConversion(FieldType.DateTime, fieldType);
    }

    private readonly void MandatoryField(int fieldId)
    {
        if (_type!=null && _type.Fields[fieldId].DefaultValue==null) {
            // throw exception mandatory field 
            ThrowMandatoryFieldCannotBeNull(_type.Fields[fieldId].Name);
        }
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference. - _type cannot be null here !!!
    private readonly void InitializeTracking() => _data[^1] = new string(new char[(_type.Fields.Length>>4)+1]);
#pragma warning restore CS8602

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetData(int fieldId, string? value)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference. - _data cannot be null here !!!
        if (string.CompareOrdinal(_data[fieldId],value)==0) return; // detect no change
        if (value==null && _type.Fields[fieldId].NotNull) MandatoryField(fieldId); // manage mandatory fields !!
#pragma warning restore CS8602
        if (_data[^1]==null) InitializeTracking();
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
    private readonly void ThrowRecordUnkownFieldName(string fieldName) => 
        throw new ArgumentException(string.Format(DefaultCulture,
                  ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownFieldName), fieldName, _type?.Name));

    private readonly void ThrowMandatoryFieldCannotBeNull(string fieldName) =>
        throw new ArgumentException(string.Format(DefaultCulture,
            ResourceHelper.GetErrorMessage(ResourceType.FieldIsMandatory), _type?.Name, fieldName));

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

    private static void ThrowImpossibleConversion(FieldType fieldTypeSource, FieldType fieldTypeDestination) =>
        throw new ArgumentException(string.Format(DefaultCulture,
            ResourceHelper.GetErrorMessage(ResourceType.RecordCannotConvert), 
            fieldTypeSource.RecordTypeDisplay() ?? NullString,
            fieldTypeDestination.RecordTypeDisplay() ?? NullString));

    private static void ThrowInvalidBase64String() =>
        throw new FormatException(ResourceHelper.GetErrorMessage(ResourceType.InvalidBase64String));

    

    #endregion

}
