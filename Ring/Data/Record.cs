using Microsoft.VisualBasic;
using Ring.Schema.Models;

namespace Ring.Data;

public struct Record : IEquatable<Record>
{
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
        _data = null;
    }

    /// <summary>
    ///     SetField methods
    /// </summary>
    public void SetField(string name, string value)
    {
        if (_type == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
        var fieldId = _type.GetFieldIndex(name);
        if (fieldId == Constants.FieldNameNotFound)
            throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));

        switch (Table.Fields[fieldId].Type)
        {
            case FieldType.String: SetStringField(fieldId, value); return;
            case FieldType.Byte:
            case FieldType.Short:
            case FieldType.Int:
            case FieldType.Long: SetIntegerField(fieldId, value); return;
            case FieldType.Float:
            case FieldType.Double: SetFloatField(fieldId, value); return;
            case FieldType.ShortDateTime:
            case FieldType.DateTime:
            case FieldType.LongDateTime: SetDateTimeField(name, value); return;
            case FieldType.Boolean: SetBooleanField(name, value); return;
            case FieldType.NotDefined:
                break;
            case FieldType.Array:
                break;
        }
    }


    public static bool operator==(Record left, Record right) => left.Equals(right);
    public static bool operator!=(Record left, Record right) => !(left==right);
    public readonly bool Equals(Record other)
    {
        if (ReferenceEquals(_type, other._type)) {
            if (_data==null ^ other._data==null) return false;
            if (_data!=null && other._data!=null) {
                var i = 0;
                while (i<_data.Length)
                {
                    if (!string.Equals(_data[i], other._data[i], StringComparison.Ordinal)) return false;
                    ++i;
                }
            }
            return true;
        }
        return false;
    }
    public override readonly bool Equals(object? obj) => obj != null && obj is Record record && Equals(record);
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
