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
    private readonly static string BooleanTrue = true.ToString(CultureInfo.InvariantCulture);
    private readonly static string BooleanFalse = false.ToString(CultureInfo.InvariantCulture);
    private string?[]? _data; // should be instanciate when record type is defined
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
    /// <summary>
    /// data lenght should be type.Length+1
    /// </summary>
    internal Record(Table type, string?[] data)
    {
        _type = type;
        _data = data;
    }

    
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
            case FieldType.String: SetStringField(fieldId, value); return;
            case FieldType.Byte:
            case FieldType.Short:
            case FieldType.Int:
            case FieldType.Long: SetIntegerField(_type.Fields[fieldId].Type, fieldId, value); return;
            case FieldType.Float:
            case FieldType.Double: SetFloatField(_type.Fields[fieldId].Type, fieldId, value); return;
            case FieldType.ShortDateTime:
            case FieldType.DateTime:
            case FieldType.LongDateTime: 
                //SetDateTimeField(name, value);
                return;
            case FieldType.Boolean: SetBooleanField(fieldId, value); return;
        }
    }

    public readonly void SetField(string name, long value)
    {
        if (_type == null) ThrowRecordUnkownRecordType();
#pragma warning disable CS8604 // Dereference of a possibly null reference. _type cannot be null here 
        var fieldId = _type.GetFieldIndex(name);
#pragma warning restore CS8604
        if (fieldId == -1) ThrowRecordUnkownFieldName(name);
        switch (_type.Fields[fieldId].Type)
        {
            case FieldType.String: 
                SetStringField(fieldId, value.ToString(CultureInfo.InvariantCulture)); 
                return;
            case FieldType.Long: 
                SetData(fieldId, value.ToString(CultureInfo.InvariantCulture));  
                return;
            case FieldType.Int:
                if (value<=int.MaxValue && value>=int.MinValue) SetData(fieldId, value.ToString(CultureInfo.InvariantCulture));
                else ThrowValueTooLarge(_type.Fields[fieldId].Type);
                return;
            case FieldType.Short:
                if (value <= short.MaxValue && value >= short.MinValue) SetData(fieldId, value.ToString(CultureInfo.InvariantCulture));
                else ThrowValueTooLarge(_type.Fields[fieldId].Type);
                return;
            case FieldType.Byte:
                if (value <= sbyte.MaxValue && value >= sbyte.MinValue) SetData(fieldId, value.ToString(CultureInfo.InvariantCulture));
                else ThrowValueTooLarge(_type.Fields[fieldId].Type);
                return;
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
        switch (_type.Fields[fieldId].Type)
        {
            case FieldType.String:
                SetStringField(fieldId, value.ToString(CultureInfo.InvariantCulture));
                break;
            case FieldType.Boolean:
                SetData(fieldId, value ? BooleanTrue : BooleanFalse);
                break;
        }
    }

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
        if (_type != null) result.Append(_type.PhysicalName);
        if (_data != null) for (var i = 0; i < _data.Length; ++i) result.Append(_data[i] ?? NullField);
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
        else if (long.TryParse(value, out long lng))
        {
            if (fieldType == FieldType.Long ||
               (fieldType == FieldType.Int && lng <= int.MaxValue && lng >= int.MinValue) ||
               (fieldType == FieldType.Short && lng <= short.MaxValue && lng >= short.MinValue))
            {
                SetData(fieldId, lng.ToString(CultureInfo.InvariantCulture));
                return;
            }
            ThrowValueTooLarge(fieldType);
        }
        ThrowWrongStringFormat();
    }

    private readonly void SetFloatField(FieldType fieldType, int fieldId, string? value)
    {
        if (value == null) SetData(fieldId, null);
        else if (double.TryParse(value, out double lng))
        {
            //TODO
            if (fieldType == FieldType.Long ||
               (fieldType == FieldType.Int && lng <= int.MaxValue && lng >= int.MinValue) ||
               (fieldType == FieldType.Short && lng <= short.MaxValue && lng >= short.MinValue))
            {
                SetData(fieldId, lng.ToString(CultureInfo.InvariantCulture));
                return;
            }
            ThrowValueTooLarge(fieldType);
        }
        ThrowWrongStringFormat();
    }

    private readonly void SetBooleanField(int fieldId, string? value)
    {
        if (value == null) SetData(fieldId, null);
        if (bool.TryParse(value, out bool result))
        {
            SetData(fieldId, result? BooleanTrue: BooleanFalse);
        }
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
            var index=(_type.Fields.Length+ _type.Relations.Length) >>4; 
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
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                  ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownFieldName), name, _type?.Name));

    private static void ThrowRecordUnkownRecordType() =>
        throw new ArgumentException(ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRecordType));

    private static void ThrowWrongStringFormat() =>
        throw new FormatException(ResourceHelper.GetErrorMessage(ResourceType.RecordWrongStringFormat));

    private static void ThrowValueTooLarge(FieldType fieldType) =>
        throw new OverflowException(string.Format(CultureInfo.InvariantCulture, 
            ResourceHelper.GetErrorMessage(ResourceType.RecordValueTooLarge), fieldType.RecordTypeDisplay()));

    #endregion

}
