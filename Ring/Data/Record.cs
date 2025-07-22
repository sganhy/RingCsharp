using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Data;

public struct Record : IEquatable<Record>
{
	private const char HashCodeSeparator = (char)3753;// end of text character
	private const long MaxIntValue = int.MaxValue;
	private const long MinIntValue = int.MinValue;
	private const long MaxShortValue = short.MaxValue;
	private const long MinShortValue = short.MinValue;
	private const long MaxByteValue = sbyte.MaxValue;
	private const long MinByteValue = sbyte.MinValue;
#pragma warning disable RCS1187 // Use constant instead of field
	private static readonly string[] DefaultData = new string[2]; // 1 field + state info
	private static readonly Table DefaultType = GetDefaultType();
	private static readonly string NullField = "^^";
	private static readonly string NullString = "<Null>";
	private static readonly string DefaultPrimaryKeyValue = "0";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture);
#pragma warning restore RCS1187

	// should be instantiate when record type is defined
	// _data.Length should be > _type.Fields.Length
	private string?[] _data;
	private Table _type;
	private readonly int _offset;

	/// <summary>
	/// 	Ctor
	/// </summary>
	public Record()
	{
		_type = DefaultType;
		_data = DefaultData;
		_offset = 0;
	}
	internal Record(Table type)
	{
		_type = type;
		_data = new string?[type.RecordSize];
		_offset = 0;
	}
	internal Record(Table type, string?[] data, int offset)
	{
		_type = type;
		_data = data;
		_offset = offset;
	}

	internal readonly string? this[int i]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _data[i + _offset];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => _data[i + _offset] = value;
	}

	internal readonly string?[] Data => _data;
#pragma warning disable RCS1085 // Use auto-implemented property
	internal readonly int Offset => _offset;
#pragma warning restore RCS1085
	public readonly bool IsDirty => _data[_type.RecordSize-1+_offset] != null;
	public readonly bool IsNew
	{
		// Code size: 64 (0x40)
		get
		{
			if (_type.Id==-1) ThrowRecordUnknownRecordType();
			if (_type.Type == TableType.Business) return _data[_type.Columns[0].RecordIndex]==null;
			// not manage ==> @lexicon_itm, @log, @meta, @meta_id; 
			return true; // always New if there are no keys
		}
	}
	internal readonly Table Table => _type;

#pragma warning disable IDE0251 // Make member 'readonly'
	internal void ClearData()
	{
		// Code size: 59 (0x3b)
		var span = _data.AsSpan();
		var lastIndex = _type.RecordSize + _offset;
		for (var i=_offset;i<lastIndex;++i) span[i] = null;
	}
#pragma warning restore IDE0251

	/// <summary>
	/// 	Get primary key value (Field name ID)
	/// </summary>
	internal readonly long GetField() // Code size: 52 (0x34)
		=> long.Parse(_data[_type.Columns[0].RecordIndex + _offset] ?? DefaultPrimaryKeyValue, DefaultCulture);

	/// <summary>
	/// 	GetField methods
	/// </summary>
	public readonly string? GetField(string name)
	{
		// Code size: 83 (0x53)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId > -1) return _data[fieldId + _offset] ?? _type.Fields[fieldId].DefaultValue;
		ThrowRecordUnknownFieldName(name);
		return null;
	}

	public readonly void GetField(string name, out bool? value)
	{
		// Code size: 164 (0xa4) - no callvirt
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		var field = _type.Fields[fieldId];
		var type = field.Type;
		if (type != FieldType.Boolean) ThrowImpossibleConversion(type, FieldType.Boolean);
		value = null;
		//BooleanTrue: BooleanFalse
		var result = _data[fieldId + _offset] ?? field.DefaultValue;
		if (string.Equals(BooleanTrue, result, StringComparison.Ordinal)) value = true;
		else if (string.Equals(BooleanFalse, result, StringComparison.Ordinal)) value = false;
	}

	public readonly void GetField(string name, out byte[]? value)
	{
		// Code size: 118 (0x76) - no callvirt
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		value = null;
		var field = _type.Fields[fieldId];
		var fieldType = field.Type;
		if (fieldType != FieldType.ByteArray) ThrowImpossibleConversion(fieldType, FieldType.ByteArray);
		var result = _data[fieldId + _offset] ?? field.DefaultValue;
		if (result != null) value = Convert.FromBase64String(result);
	}

	public readonly void GetField(string name, out long? value)
	{
		// Code size: 145 (0x91) - no callvirt
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		value = null;
		var field = _type.Fields[fieldId];
		var fieldType = field.Type;
#pragma warning disable RCS1001 // Add braces (when expression spans over multiple lines)
		if (fieldType != FieldType.Byte && fieldType != FieldType.Short &&
			fieldType != FieldType.Int && fieldType != FieldType.Long)
			ThrowImpossibleConversion(fieldType, FieldType.Long);
#pragma warning restore RCS1001
		var result = _data[fieldId + _offset] ?? field.DefaultValue;
		if (result != null) value = long.Parse(result, DefaultCulture);
	}

	/// <summary>
	/// 	Get UTC date/time
	/// </summary>
	public readonly void GetField(string name, out DateTime? value)
	{
		// Code size: 143 (0x8f) - no callvirt
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		value = null;
		var field = _type.Fields[fieldId];
		var fieldType = field.Type;
		if (fieldType != FieldType.DateTime && fieldType != FieldType.LongDateTime && fieldType != FieldType.ShortDateTime)
			ThrowImpossibleConversion(fieldType, FieldType.DateTime);
		var result = _data[fieldId + _offset] ?? field.DefaultValue;
		if (result == null) return;
		value = result.ToDateTime(fieldType);
	}

	/// <summary>
	/// 	Set field value
	/// </summary>
	public void SetField(string name, string? value)
	{
		// Code size: 221 (0xdd)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var field = _type.Fields[fieldId];
		var type = field.Type;
#pragma warning disable RCS1003 // Add braces to if-else (when expression spans over multiple lines)
		if (value != null)
			switch (type)
			{
				case FieldType.String: SetStringField(field.Size, fieldId, value); break;
				case FieldType.LongString: SetData(fieldId, value); break; // no size check!
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
		else SetData(fieldId, null);
#pragma warning restore RCS1003
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetField(string name, long value, FieldType fieldType)
	{
		// Code size: 277 (0x115) - no callvirt
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var type = _type.Fields[fieldId].Type;
		switch (type)
		{
			case FieldType.Long:
				SetData(fieldId, value.ToString(DefaultCulture));
				break;
			case FieldType.Int:
				if (value <= MaxIntValue && value >= MinIntValue) SetData(fieldId, value.ToString(DefaultCulture));
				else ThrowValueTooLarge(type);
				break;
			case FieldType.Short:
				if (value <= MaxShortValue && value >= MinShortValue) SetData(fieldId, value.ToString(DefaultCulture));
				else ThrowValueTooLarge(type);
				break;
			case FieldType.Byte:
				if (value <= MaxByteValue && value >= MinByteValue) SetData(fieldId, value.ToString(DefaultCulture));
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

	public void SetField(string name, long value) => SetField(name, value, FieldType.Long); // Code size: 10 (0xa)
	public void SetField(string name, int value) => SetField(name, value, FieldType.Int); // Code size: 11 (0xb)
	public void SetField(string name, short value) => SetField(name, value, FieldType.Short); // Code size: 11 (0xb)
	public void SetField(string name, sbyte value) => SetField(name, value, FieldType.Byte); // Code size: 11 (0xb)
	public void SetField(string name, bool value)
	{
		// Code size: 99 (0x63)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var fieldType = _type.Fields[fieldId].Type;
		if (fieldType == FieldType.Boolean) SetData(fieldId, value ? BooleanTrue : BooleanFalse);
		else ThrowImpossibleConversion(FieldType.Boolean, fieldType);
	}
	public void SetField(string name, DateTime value)
	{
		// Code size: 79 (0x4f) 
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value, null);
	}
	public void SetField(string name, DateTimeOffset value)
	{
		// Code size: 88 (0x58)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value.DateTime, value.Offset);
	}
	public void SetField(string name, double value) => SetField(name, value, FieldType.Double); // Code size: 11 (0xb)
	public void SetField(string name, float value) => SetField(name, value, FieldType.Float); // Code size: 12 (0xc)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetField(string name, double value, FieldType fieldType)
	{
		// Code size: 99 (0x63)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var type = _type.Fields[fieldId].Type;
		if (type != FieldType.Float && type != FieldType.Double) ThrowImpossibleConversion(fieldType, type);
		SetData(fieldId, value.ToString(DefaultCulture));
	}

	public void SetField<T>(string name, T value) where T : IEnumerable<byte>
	{
		// Code size: 99 (0x63)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var fieldType = _type.Fields[fieldId].Type;
		if (fieldType != FieldType.ByteArray) ThrowImpossibleConversion(FieldType.ByteArray, fieldType);
		SetData(fieldId, Convert.ToBase64String(value.ToArray()));
	}
	public static bool operator ==(Record left, Record right) => left.Equals(right);
	public static bool operator !=(Record left, Record right) => !left.Equals(right);
	public readonly override bool Equals(object? obj) => obj is Record record && Equals(record);
	public readonly bool Equals(Record other)
	{
		// Code size: 88 (0x58)
		if (ReferenceEquals(_type, other._type))
		{
			var i = 0;
			var offset1 = _offset;
			var offset2 = other._offset;
			var count = _type.RecordSize-1;
			while (i < count)
			{
				if (!string.Equals(_data[i+offset1], other._data[i+offset2], StringComparison.Ordinal)) return false;
				++i;
			}
			return true;
		}
		return false;
	}
	public readonly override int GetHashCode()
	{
		// Code size: 108 (0x6c)
		var result = new StringBuilder();
		result.Append(_type.PhysicalName);
		var i = _offset;
		var columnCount = _type.RecordSize - 1;
		columnCount += i;
		while (i < columnCount)
		{
			result.Append(_data[i] ?? NullField).Append(HashCodeSeparator);
			++i;
		}
		HashHelper.Djb2X(result.ToString(), out int hash);
		return hash;
	}
	internal readonly bool EqualTo(SaveQuery obj) => ReferenceEquals(obj.Data, _data) && obj.Offset == _offset; // Code size: 31 (0x1f)
	internal readonly bool IsFieldChanged(string name)
	{
		// Code size: 87 (0x57)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var index = _type.GetFieldIndex(name);
		var trackerIndex = _offset + _type.RecordSize - 1;
		if (index != -1) return _data[trackerIndex] != null && IsColumnChanged(index, trackerIndex);
		ThrowRecordUnknownFieldName(name);
		return false;
	}

	internal readonly bool IsFieldExist(string name) => _type.GetFieldIndex(name) != -1; // Code size: 19 (0x13)

	internal readonly bool IsRelationChanged(string name)
	{
		// Code size: 124 (0x7c)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var relation = _type.GetRelation(name);
		var trackerIndex = _offset + _type.RecordSize - 1;
		if (relation == null) ThrowRecordUnknownRelationName(name);
		var column = _type.GetColumn(relation.Id, EntityType.Relation);
		if (column == null) ThrowRecordWrongRelationType(name);
		var index = column.RecordIndex;
		if (index >= 0) return _data[trackerIndex] != null && IsColumnChanged(index, trackerIndex);
		return false;
	}

	internal readonly bool IsRelationExist(string name) => _type.GetRelationIndex(name) != -1; // Code size: 19 (0x13)

	/// <summary>
	/// 	Return relation ID value by name
	/// </summary>
	/// <param name="name">Name of the relation</param>
	/// <returns>relation ID value; if not defined, return null</returns>
	internal readonly long? GetRelation(string name)
	{
		// Code size: 109 (0x6d)
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var relation = _type.GetRelation(name);
		if (relation == null) ThrowRecordUnknownRelationName(name);
		var column = _type.GetColumn(relation.Id, EntityType.Relation);
		if (column == null) ThrowRecordWrongRelationType(name);
		var index = _offset + column.RecordIndex;
		return long.Parse(_data[index]!, CultureInfo.InvariantCulture);
	}

	internal void SetRelation(string name, long? value)
	{
		// Code size: 99 (0x63) - no callvirt
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var relation = _type.GetRelation(name);
		if (relation == null) ThrowRecordUnknownRelationName(name);
		var column = _type.GetColumn(relation.Id, EntityType.Relation);
		if (column == null) ThrowRecordUnknownRelationName(name);
		var index = column.RecordIndex;
		if (index >= 0) SetData(column.RecordIndex, value?.ToString(DefaultCulture));
		else ThrowRecordWrongRelationType(name);
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetStringField(int fieldSize, int fieldId, string value)
	{
		// Code size: 27 (0x1b)
		SetData(fieldId, value.Length <= fieldSize ? value : value.Truncate(fieldSize)); // truncate or exception ?? // replace by Span<T>
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetIntegerField(int fieldId, FieldType numberType, string value) {
		// Code size: 120 (0x78)
#pragma warning disable RCS1003 // Add braces to if-else (when expression spans over multiple lines)
		if (!value.IsNumber()) ThrowWrongStringFormat();
		else if (long.TryParse(value, NumberStyles.Integer, DefaultCulture, out long lng) &&
				(numberType == FieldType.Long ||
				(numberType == FieldType.Int && lng <= MaxIntValue && lng >= MinIntValue) ||
				(numberType == FieldType.Short && lng <= MaxShortValue && lng >= MinShortValue) ||
				(numberType == FieldType.Byte && lng <= MaxByteValue && lng >= MinByteValue)))
			SetData(fieldId, lng.ToString(DefaultCulture));
		else ThrowValueTooLarge(numberType);
#pragma warning restore RCS1003
	}

	private void SetFloatField(FieldType fieldType, int fieldId, string value)
	{
		// see. ISO 6093:1985
		// Code size: 103 (0x67) - no callvirt
		if (double.TryParse(value, NumberStyles.Float, DefaultCulture, out double dbl))
		{
			if (fieldType == FieldType.Double) SetData(fieldId, dbl.ToString(DefaultCulture));
			else if (fieldType == FieldType.Float && float.TryParse(value, NumberStyles.Float, DefaultCulture, out float flt))
				SetData(fieldId, flt.ToString(DefaultCulture));
			else ThrowValueTooLarge(fieldType);
			return;
		}
		ThrowWrongStringFormat();
	}

	private void SetByteArrayField(int fieldId, string value)
	{
		if (value.IsBase64String()) SetData(fieldId, value);
		else ThrowInvalidBase64String();
	}

	private void SetBooleanField(int fieldId, string value)
	{
		// Code size: 40 (0x28)
		if (bool.TryParse(value, out bool result)) SetData(fieldId, result ? BooleanTrue : BooleanFalse);
		else ThrowWrongBooleanValue(value);
	}

	private void SetDateTimeField(int fieldId, FieldType fieldType, string value)
	{
		var dateTimeOffset = value.ParseIso8601Date();
		SetDateTimeField(fieldId, fieldType, dateTimeOffset.DateTime, dateTimeOffset.Offset);
	}

	private void SetDateTimeField(int fieldId, FieldType fieldType, DateTime value, TimeSpan? offset)
	{
		// Code size: 51 (0x33)
		if (fieldType == FieldType.DateTime || fieldType == FieldType.LongDateTime || fieldType == FieldType.ShortDateTime)
			SetData(fieldId, new string(value.ToString(fieldType, offset)));
		else ThrowImpossibleConversion(FieldType.DateTime, fieldType);
	}

	private readonly void MandatoryField(int fieldId)
	{
		if (_type.Fields[fieldId].DefaultValue == null) {
			// throw exception mandatory field 
			ThrowMandatoryFieldCannotBeNull(_type.Fields[fieldId].Name);
		}
	}

#pragma warning disable IDE0251 // Make member 'readonly'
	private void InitializeTracking(int trackerIndex) => _data[trackerIndex] = new string(new char[(_type.Fields.Length >> 4) + 1]);
#pragma warning restore IDE0251

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetData(int fieldId, string? value)
	{
		// Code size: 118 (0x76)
		var fieldIndex = fieldId + _offset;
		var trackerIndex = _type.RecordSize -1 +_offset;
		if (string.CompareOrdinal(_data[fieldIndex], value) == 0) return; // detect no change
		if (value == null && _type.Fields[fieldId].NotNull) MandatoryField(fieldId); // manage mandatory fields !!
		if (_data[trackerIndex] == null) InitializeTracking(trackerIndex);
		_data[trackerIndex]!.SetBitValue(fieldId); // cannot be null here !!
		_data[fieldIndex] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly bool IsColumnChanged(int fieldId, int trackerIndex) => _data[trackerIndex]!.GetBitValue(fieldId); // cannot be null here - Code size: 15 (0xf)

	// exceptions 
	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private readonly void ThrowRecordUnknownFieldName(string fieldName) => 
		throw new ArgumentException(string.Format(DefaultCulture,
			ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownFieldName), fieldName, _type.Name));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private readonly void ThrowRecordWrongRelationType(string relationName) =>
		throw new ArgumentException(string.Format(DefaultCulture,
			ResourceHelper.GetErrorMessage(ResourceType.RecordWrongRelationType), relationName, _type.Name));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private readonly void ThrowRecordUnknownRelationName(string relationName) =>
		throw new ArgumentException(string.Format(DefaultCulture,
			ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRelationName), relationName, _type.Name));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private readonly void ThrowMandatoryFieldCannotBeNull(string fieldName) =>
		throw new ArgumentException(string.Format(DefaultCulture,
			ResourceHelper.GetErrorMessage(ResourceType.FieldIsMandatory), _type.Name, fieldName));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowRecordUnknownRecordType() =>
		throw new ArgumentException(ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRecordType));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowWrongStringFormat() =>
		throw new FormatException(ResourceHelper.GetErrorMessage(ResourceType.RecordWrongStringFormat));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowValueTooLarge(FieldType fieldType) =>
		throw new OverflowException(string.Format(DefaultCulture, 
			ResourceHelper.GetErrorMessage(ResourceType.RecordValueTooLarge), fieldType.RecordTypeDisplay()));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowWrongBooleanValue(string? value) =>
		throw new FormatException(string.Format(DefaultCulture,
			ResourceHelper.GetErrorMessage(ResourceType.RecordWrongBooleanValue), value ?? NullString));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowImpossibleConversion(FieldType fieldTypeSource, FieldType fieldTypeDestination) =>
		throw new ArgumentException(string.Format(DefaultCulture,
			ResourceHelper.GetErrorMessage(ResourceType.RecordCannotConvert),
			fieldTypeSource.RecordTypeDisplay(),
			fieldTypeDestination.RecordTypeDisplay()));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidBase64String() =>
		throw new FormatException(ResourceHelper.GetErrorMessage(ResourceType.InvalidBase64String));

	private static IDdlBuilder GetDefaultDdlBuilder() => new Util.Builders.PostgreSQL.DdlBuilder(); // Code size: 6 (0x6)

	private static Table GetDefaultType()
	{
		// Code size: 77 (0x4d)
		var metaTable = new Meta(-1, (byte)EntityType.Table, 0, (int)TableType.Undefined, 0L, string.Empty, null, null, true);
		var metaArray = new Meta[] { new(0, (byte)EntityType.Field, 0, 0, 0L, string.Empty, null, null, true) };
		return metaTable.ToTable(new ArraySegment<Meta>(metaArray), PhysicalType.Undefined, GetDefaultDdlBuilder(), string.Empty, 1)!; // cannot be null here!!
	}

	#endregion

}
