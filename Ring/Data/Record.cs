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
        if (_type==null) throw new ArgumentException(ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRecordType));
        var fieldId = _type.GetFieldIndex(name);
#pragma warning disable CS8602 // Dereference of a possibly null reference. _data cannot be null here 
        if (fieldId>-1) return _data[fieldId] ?? _type.Fields[fieldId].DefaultValue;
#pragma warning restore CS8602
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownFieldName), name, _type.Name));
    }

    /// <summary>
    ///     Set field value 
    /// </summary>
    /// <param name="name">field name</param>
    /// <param name="value">field value</param>
    public readonly void SetField(string name, string? value)
    {
        if (_type==null) throw new ArgumentException(ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRecordType));
        var fieldId = _type.GetFieldIndex(name);
        if (fieldId==-1) throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
            ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownFieldName), name, _type.Name));
        switch (_type.Fields[fieldId].Type)
        {
            case FieldType.String: SetStringField(fieldId, value); return;
            case FieldType.Byte:
            case FieldType.Short:
            case FieldType.Int:
            case FieldType.Long: SetIntegerField(_type.Fields[fieldId].Type, fieldId, value); return;
            case FieldType.Float:
            case FieldType.Double: 
                //SetFloatField(fieldId, value); 
                return;
            case FieldType.ShortDateTime:
            case FieldType.DateTime:
            case FieldType.LongDateTime: 
                //SetDateTimeField(name, value);
                return;
            case FieldType.Boolean: 
                //SetBooleanField(name, value); 
                return;
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

    // no need agressive inlining here
    private readonly void SetIntegerField(FieldType fieldType, int fieldId, string? value) {
        if (long.TryParse(value, out long lng))
        {
            SetData(fieldId, lng.ToString(CultureInfo.InvariantCulture));
            return;
        } 
        // else throw an exception, invalid integer
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SetData(int fieldId, string? value)
    {
        if (_data==null || _data[fieldId] == value) return; // detect no change
        //if (_data[^1] == null) _data[^1] = new string(new byte[], StandardCharsets.UT);
            /*
            if (Table.PrimaryKeyIdIndex >= 0 && _data[Table.PrimaryKeyIdIndex] != null)
            {
                var key = fieldId;
                // long = 64
                key >>= Constants.RcdDefaultShiftLeft;
                key = -key; // --> set to negatif
                --key;
                if (_extraInfo == null) _extraInfo = new SortedDictionary<int, long>();
                if (!_extraInfo.ContainsKey(key)) _extraInfo.Add(key, 0L);
                // bit number =>  fieldId & MASK_64BITS
                _extraInfo[key] |= 1L << (fieldId & Constants.Mask64Bits);
            }
            // is there a change ?
            */
            _data[fieldId] = value;
    }

    #endregion 

}
