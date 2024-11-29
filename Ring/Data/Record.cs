using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Ring.Data;

public struct Record : IEquatable<Record>
{
	private const char HashCodeSeparator = (char)3553;// end of text character
	private const decimal MaxIntValue = int.MaxValue;
	private const decimal MinIntValue = int.MinValue;
	private const decimal MaxShortValue = short.MaxValue;
	private const decimal MinShortValue = short.MinValue;
	private const decimal MaxByteValue = sbyte.MaxValue;
	private const decimal MinByteValue = sbyte.MinValue;
#pragma warning disable RCS1187 // Use constant instead of field
	private static readonly string[] DefaultData = new string[2]; // 1 field + state info
	private static readonly Table DefaultType = GetDefaultType();
	private static readonly string NullField = "^^";
	private static readonly string NullString = "Null";
	private static readonly string DefaultPrimaryKeyValue = "0";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly NumberStyles DefaultNumberStyle = NumberStyles.Integer;
	private static readonly NumberStyles DefaultFloatStyle = NumberStyles.AllowDecimalPoint | NumberStyles.Float;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture);
#pragma warning restore RCS1187

	// should be instantiate when record type is defined
	// _data.Length should be > _type.Fields.Length
	private string?[] _data;
	private Table _type;
	private readonly int _offset;

	/// <summary>
	///	 Ctor
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

	internal string? this[int i]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get => _data[i + _offset];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => _data[i + _offset] = value;
	}

	internal readonly string?[] Data => _data;
	internal readonly int Offset => _offset;
	public readonly bool IsDirty => _data[_type.RecordSize-1+_offset] != null;
	public readonly bool IsNew
	{
		get
		{
			if (_type.Id==-1) ThrowRecordUnknownRecordType();
			if (_type.Type == TableType.Business) return _data[_type.RecordIndexes[0]]==null;
			// not manage ==> @lexicon_itm, @log, @meta, @meta_id; 
			return true; // always New if there is no keys
		}
	}
	internal readonly Table Table => _type;

	internal void ClearData()
	{
		var span = _data.AsSpan();
		var lastIndex = _type.RecordSize + _offset;
		for (var i=_offset;i<lastIndex;++i) span[i] = null;
	}

	/// <summary>
	///	 Get primary key value (Field name ID)
	/// </summary>
	internal readonly long GetField()
		=> long.Parse(_data[_type.RecordIndexes[0]+_offset] ?? DefaultPrimaryKeyValue, DefaultCulture);

	/// <summary>
	///	 GetField methods
	/// </summary>
	public readonly string? GetField(string name)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId > -1) return _data[fieldId + _offset] ?? _type.Fields[fieldId].DefaultValue;
		ThrowRecordUnknownFieldName(name);
		return null;
	}

	public readonly void GetField(string name, out bool? value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		var field = _type.Fields[fieldId];
		if (field.Type != FieldType.Boolean) ThrowImpossibleConversion(field.Type, FieldType.Boolean);
		value = null;
		//BooleanTrue: BooleanFalse
		var result = _data[fieldId + _offset] ?? _type.Fields[fieldId].DefaultValue;
		if (BooleanTrue.Equals(result, StringComparison.Ordinal)) value = true;
		else if (BooleanFalse.Equals(result, StringComparison.Ordinal)) value = false;
	}

	public readonly void GetField(string name, out byte[]? value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		value = null;
		var field = _type.Fields[fieldId];
		if (field.Type != FieldType.ByteArray) ThrowImpossibleConversion(field.Type, FieldType.Boolean);
		var result = _data[fieldId + _offset] ?? _type.Fields[fieldId].DefaultValue;
		if (result != null) value = Convert.FromBase64String(result);
	}

	public readonly void GetField(string name, out long? value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		value = null;
		var field = _type.Fields[fieldId];
		if (field.Type != FieldType.Byte && field.Type != FieldType.Short && field.Type != FieldType.Int && field.Type != FieldType.Long)
			ThrowImpossibleConversion(field.Type, FieldType.Long);
		var result = _data[fieldId + _offset] ?? _type.Fields[fieldId].DefaultValue;
		if (result != null) value = long.Parse(result, DefaultCulture);
	}

	/// <summary>
	/// Get UTC date/time
	/// </summary>
	public readonly void GetField(string name, out DateTime? value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(name);
		value = null;
		var field = _type.Fields[fieldId];
		if (field.Type != FieldType.DateTime && field.Type != FieldType.LongDateTime && field.Type != FieldType.ShortDateTime)
			ThrowImpossibleConversion(field.Type, FieldType.DateTime);
		var result = _data[fieldId + _offset] ?? _type.Fields[fieldId].DefaultValue;
		if (result == null) return;
		var year = int.Parse(result[..4], DefaultCulture);
		var month = int.Parse(result.AsSpan(5, 2), DefaultNumberStyle, DefaultCulture);
		var day = int.Parse(result.AsSpan(8, 2), DefaultNumberStyle, DefaultCulture);
		if (field.Type == FieldType.ShortDateTime)
		{
			value = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
		}
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
	///	 Set field value
	/// </summary>
	/// <param name="name">field name</param>
	/// <param name="value">field value</param>
	public void SetField(string name, string? value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var type = _type.Fields[fieldId].Type;
		if (value != null) 
			switch (type)
			{
				case FieldType.String: SetStringField(_type.Fields[fieldId].Size, fieldId, value); break;
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
	}

	internal void SetField(string name, long value, FieldType fieldType)
	{
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
	public void SetField(string name, long value) => SetField(name, value, FieldType.Long);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetField(string name, int value) => SetField(name, value, FieldType.Int);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetField(string name, short value) => SetField(name, value, FieldType.Short);
	public void SetField(string name, sbyte value) => SetField(name, value, FieldType.Byte);
	public void SetField(string name, bool value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var fieldType = _type.Fields[fieldId].Type;
		if (fieldType == FieldType.Boolean) SetData(fieldId, value ? BooleanTrue : BooleanFalse);
		else ThrowImpossibleConversion(FieldType.Boolean, fieldType);
	}
	public void SetField(string name, DateTime value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value, null);
	}
	public void SetField(string name, DateTimeOffset value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		SetDateTimeField(fieldId, _type.Fields[fieldId].Type, value.DateTime, value.Offset);
	}
	public void SetField(string name, double value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var fieldType = _type.Fields[fieldId].Type;
		if (fieldType != FieldType.Float && fieldType != FieldType.Double) ThrowImpossibleConversion(FieldType.Double, fieldType);
		SetData(fieldId, value.ToString(DefaultCulture));
	}
	public void SetField(string name, float value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var fieldType = _type.Fields[fieldId].Type;
		if (fieldType != FieldType.Float && fieldType != FieldType.Double) ThrowImpossibleConversion(FieldType.Float, fieldType);
		SetData(fieldId, value.ToString(DefaultCulture));
	}
	public void SetField<T>(string name, T value) where T : IEnumerable<byte>
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(name);
		var fieldType = _type.Fields[fieldId].Type;
		if (fieldType != FieldType.ByteArray) ThrowImpossibleConversion(FieldType.ByteArray, fieldType);
		SetData(fieldId, Convert.ToBase64String(value.ToArray()));
	}
	public static bool operator ==(Record left, Record right) => left.Equals(right);
	public static bool operator !=(Record left, Record right) => !left.Equals(right);
	public override readonly bool Equals(object? obj) => obj is Record record && Equals(record);
	public readonly bool Equals(Record other)
	{
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
	public override readonly int GetHashCode()
	{
		var result = new StringBuilder();
		result.Append(_type.PhysicalName);
		var i = _offset;
		var columnCount = _type.RecordSize - 1;
		columnCount += i;
		while (i < columnCount)
		{
			result.Append(_data[i] ?? NullField);
			result.Append(HashCodeSeparator);
			++i;
		}
		HashHelper.Djb2X(result.ToString(), out int hash);
		return hash;
	}
	internal readonly bool Equals(SaveQuery obj) => ReferenceEquals(obj.Data, _data) && obj.Offset == _offset;
	internal readonly bool IsFieldChanged(string name)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var index = _type.GetFieldIndex(name);
		var trackerIndex = _offset + _type.RecordSize - 1;
		if (index != -1) return _data[trackerIndex] != null && IsColumnChanged(index, trackerIndex);
		ThrowRecordUnknownFieldName(name);
		return false;
	}

	internal readonly bool IsFieldExist(string name) => _type.GetFieldIndex(name) != -1;

	internal readonly bool IsRelationChanged(string name)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var relation = _type.GetRelation(name);
		var trackerIndex = _offset + _type.RecordSize - 1;
		if (relation == null) ThrowRecordUnknownRelationName(name);
		var index = relation.RecordIndex;
		if (index >= 0) return _data[trackerIndex] != null && IsColumnChanged(index, trackerIndex);
		return false;
	}

	internal readonly bool IsRelationExist(string name) => _type.GetRelationIndex(name) != -1;

	/// <summary>
	///	 Return relation ID value by name
	/// </summary>
	/// <param name="name">Name of the relation</param>
	/// <returns>relation ID value;if not defined return null</returns>
	internal readonly long? GetRelation(string name)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var relation = _type.GetRelation(name);
		if (relation == null) ThrowRecordUnknownRelationName(name);
		var index = relation.RecordIndex + _offset;
		if (index >= 0 && _data[index] != null) return long.Parse(_data[index]!, CultureInfo.InvariantCulture);
		else ThrowRecordWrongRelationType(name);
		return null;
	}

	internal void SetRelation(string name, long? value)
	{
		if (_type.Id == -1) ThrowRecordUnknownRecordType();
		var relation = _type.GetRelation(name);
		if (relation == null) ThrowRecordUnknownRelationName(name);
		var index = relation.RecordIndex;
		if (index >= 0) SetData(index, value?.ToString(DefaultCulture));
		else ThrowRecordWrongRelationType(name);
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetStringField(int fieldSize, int fieldId, string value)
	{
		if (value.Length <= fieldSize) SetData(fieldId, value);
		else SetData(fieldId, value.Truncate(fieldSize)); // truncate or exception ?? // replace by Span<T>
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetIntegerField(int fieldId, FieldType numberType, string value) {
		if (!value.IsNumber()) ThrowWrongStringFormat();
		else if (long.TryParse(value, DefaultNumberStyle, DefaultCulture, out long lng) && (
				(numberType == FieldType.Int && lng <= MaxIntValue && lng >= MinIntValue) ||
				(numberType == FieldType.Short && lng <= MaxShortValue && lng >= MinShortValue) ||
				(numberType == FieldType.Byte && lng <= MaxByteValue && lng >= MinByteValue)))
			SetData(fieldId, lng.ToString(DefaultCulture));
		else ThrowValueTooLarge(numberType);
	}

	private void SetFloatField(FieldType fieldType, int fieldId, string value)
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

	private void SetByteArrayField(int fieldId, string value)
	{
		if (value.IsBase64String()) SetData(fieldId, value);
		else ThrowInvalidBase64String();
	}

	private void SetBooleanField(int fieldId, string value)
	{
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

	private void InitializeTracking(int trackerIndex) => _data[trackerIndex] = new string(new char[(_type.Fields.Length >> 4) + 1]);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetData(int fieldId, string? value)
	{
		var fieldIndex = fieldId + _offset;
		var trackerIndex = _type.RecordSize -1 +_offset;
		if (string.CompareOrdinal(_data[fieldIndex], value) == 0) return; // detect no change
		if (value == null && _type.Fields[fieldId].NotNull) MandatoryField(fieldId); // manage mandatory fields !!
		if (_data[trackerIndex] == null) InitializeTracking(trackerIndex);
		_data[trackerIndex]!.SetBitValue(fieldId); // cannot be null here !!
		_data[fieldIndex] = value;
	}

	// Dereference of a possibly null reference. - _data cannot be null here !!!
	// Possible null reference argument. 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly bool IsColumnChanged(int fieldId, int trackerIndex) => _data[trackerIndex]!.GetBitValue(fieldId); // cannot be null here 

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
			fieldTypeSource.RecordTypeDisplay() ?? NullString,
			fieldTypeDestination.RecordTypeDisplay() ?? NullString));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidBase64String() =>
		throw new FormatException(ResourceHelper.GetErrorMessage(ResourceType.InvalidBase64String));

	private static Table GetDefaultType()
	{
		var metaTable = new Meta(-1, (byte)EntityType.Table, 0, (int)TableType.Undefined, 0L, string.Empty, null, null, true);
		var metaArray = new Meta[] { new(0, (byte)EntityType.Field, 0, 0, 0L, string.Empty, null, null, true) };
		return metaTable.ToTable(new ArraySegment<Meta>(metaArray), PhysicalType.Undefined, string.Empty)!; // cannot be null here!!
	}

	#endregion

}
