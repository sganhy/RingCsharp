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

namespace Ring.Data;

/// <summary>
/// 	The Record struct is a mutable data container that represents a single database record with built-in change tracking and type safety. 
/// </summary>
public struct Record : IEquatable<Record>
{
	private const long MaxIntValue = int.MaxValue;
	private const long MinIntValue = int.MinValue;
	private const long MaxShortValue = short.MaxValue;
	private const long MinShortValue = short.MinValue;
	private const long MinByteValue = sbyte.MinValue;
	private const long MaxByteValue = sbyte.MaxValue;
	private const char SchemaSeparator = '.';
#pragma warning disable RCS1187 // Use constant instead of field
	private static readonly string[] DefaultData = new string[2]; // 1 field + state info
	private static readonly Table DefaultType = GetDefaultType();
	private static readonly string NullField = "^^"; // skip conversion into constant
	private static readonly string NullString = "<Null>";
	private static readonly string DefaultPrimaryKeyValue = "0";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture);
#pragma warning restore RCS1187

    // should be instantiated when record type is defined - "Flyweight" Pattern
    // _data.Length should be > _type.Fields.Length - total: ~24 bytes + heap allocations for array of string?
    private string?[] _data;
	private Table _type;
	private int _offset; // cannot be readonly anymore! : Allows multiple records to share the same underlying array

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
		get => _data[i + _offset]; // Code size: 16 (0x10)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => _data[i + _offset] = value; // Code size: 17 (0x11)
	}
	
#pragma warning disable RCS1085 // Use auto-implemented property
	internal readonly string?[] Data => _data; // skip auto-property here
	internal readonly int Offset => _offset; // skip auto-property here
#pragma warning restore RCS1085
	
	public readonly bool IsDirty => _data[_type.RecordSize-1 + _offset] is not null; // Code size: 34 (0x22)
    public readonly bool IsNew
	{
		// Code size: 56 (0x38)
		get
		{
			var table = _type;
			if (table.Id==-1) ThrowRecordUnknownRecordType();
			if (table.Type == TableType.Business) return _data[table.Columns[0].RecordIndex]==null;
			// not managed ==> @lexicon_itm, @log, @meta, @meta_id; 
			return true; // always New if there are no keys
		}
	}
	internal readonly Table Table => _type;

#pragma warning disable IDE0251 // Make member 'readonly'
	internal void ClearData() => Array.Clear(_data, _offset, _type.RecordSize); // Code size: 29 (0x1d)
#pragma warning restore IDE0251

	public string? RecordType 
	{
		readonly get
		{
			// Code size: 67 (0x43)
			var table = _type;
			if (table is null) return null;
			var schema = Global.GetSchema(table.SchemaId);
			var tableName = table.Name;
			if (schema is null) ThrowRecordUnknownRecordType();
			if (Global.IsSchemaDefault(schema)) return tableName;
			return schema.Name + SchemaSeparator + tableName;
		}
		set 
		{
			// Code size: 152 (0x98)
			if (value is not null)
			{
				var separatorIndex = value.IndexOf(SchemaSeparator);
				var tableName = separatorIndex > 0 ? value[(separatorIndex+1)..] : value;
				var schemaName = separatorIndex > 0 ? value[..separatorIndex] : null;
				var table = Global.GetTable(schemaName, tableName);
				if (table is null) ThrowRecordUnknownRecordType();
				if (ReferenceEquals(table, _type)) ClearData(); // Is RecordType changed? 
				else
				{
					_data = new string?[table.RecordSize];
					_type = table;
					_offset = 0;
				}
			}
			else 
			{
				_data = DefaultData;
				_type = DefaultType;
				_offset = 0;
			}
		}
	}

	/// <summary>
	/// 	Get primary key value (Field name ID)
	/// </summary>
	internal readonly long GetField() => long.Parse(_data[_type.Columns[0].RecordIndex + _offset] ?? DefaultPrimaryKeyValue, DefaultCulture); // Code size: 52 (0x34)

	/// <summary>
	/// 	GetField methods
	/// </summary>
	public readonly string? GetField(string name)
	{
		// Code size: 77 (0x4d) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId > -1) return data[fieldId + _offset] ?? table.Fields[fieldId].DefaultValue;
		ThrowRecordUnknownFieldName(table, name);
		return null;
	}

	public readonly void GetField(string name, out bool? value)
	{
		// Code size: 159 (0x9f) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(table, name);
		var field = table.Fields[fieldId];
		var type = field.Type;
		if (type != FieldType.Boolean) ThrowImpossibleConversion(type, FieldType.Boolean);
		value = null;
		//BooleanTrue: BooleanFalse
		var result = data[fieldId + _offset] ?? field.DefaultValue;
		if (string.Equals(BooleanTrue, result, StringComparison.Ordinal)) value = true;
		else if (string.Equals(BooleanFalse, result, StringComparison.Ordinal)) value = false;
	}

	public readonly void GetField(string name, out byte[]? value)
	{
		// Code size: 113 (0x71) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(table, name);
		value = null;
		var field = table.Fields[fieldId];
		var fieldType = field.Type;
		if (fieldType != FieldType.ByteArray) ThrowImpossibleConversion(fieldType, FieldType.ByteArray);
		var result = data[fieldId + _offset] ?? field.DefaultValue;
		if (result is not  null) value = Convert.FromBase64String(result);
	}

	public readonly void GetField(string name, out long? value)
	{
		// Code size: 140 (0x8c) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(table, name);
		value = null;
		var field = table.Fields[fieldId];
		var fieldType = field.Type;
#pragma warning disable RCS1001 // Add braces (when expression spans over multiple lines)
		if (fieldType != FieldType.Byte && fieldType != FieldType.Short &&
			fieldType != FieldType.Int && fieldType != FieldType.Long)
			ThrowImpossibleConversion(fieldType, FieldType.Long);
#pragma warning restore RCS1001
		var result = data[fieldId + _offset] ?? field.DefaultValue;
		if (result is not null) value = long.Parse(result, DefaultCulture); // already validated in SetField method!!!
	}

	/// <summary>
	/// 	Get UTC date/time
	/// </summary>
	public readonly void GetField(string name, out DateTime? value)
	{
		// Code size: 138 (0x8a) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId <= -1) ThrowRecordUnknownFieldName(table, name);
		value = null;
		var field = table.Fields[fieldId];
		var fieldType = field.Type;
		if (fieldType != FieldType.DateTime && fieldType != FieldType.DateTimeOffset && fieldType != FieldType.Date) 
			ThrowImpossibleConversion(fieldType, FieldType.DateTime);
		var result = data[fieldId + _offset] ?? field.DefaultValue;
		if (result is null) return;
		value = result.ToDateTime(fieldType);
	}

#pragma warning disable IDE0251 // Make member 'readonly'

    /// <summary>
    /// 	Set field value
    /// </summary>
    public void SetField(string name, string? value)
	{
		// Code size: 258 (0x102) - no virtual calls
		var data = new Span<string?>(_data);
		var table = _type;
		var offset = _offset;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		var field = table.Fields[fieldId];
		var fieldType = field.Type;
#pragma warning disable RCS1003 // Add braces to if-else (when expression spans over multiple lines)
		if (value is not null)
			switch (fieldType)
			{
				case FieldType.String: SetStringField(data, table, offset, field.Size, fieldId, value); break;
				case FieldType.LongString: SetData(data, table, offset, fieldId, value); break; // no size check!
				case FieldType.Byte:
				case FieldType.Short:
				case FieldType.Int:
				case FieldType.Long: SetIntegerField(data, table, offset, fieldId, fieldType, value); break;
				case FieldType.Float:
				case FieldType.Double: SetFloatField(data, table, offset, fieldType, fieldId, value); break;
				case FieldType.Date:
				case FieldType.DateTime:
				case FieldType.DateTimeOffset: SetDateTimeField(data, table, offset, fieldId, fieldType, value); break;
				case FieldType.Boolean: SetBooleanField(data, table, offset, fieldId, value); break;
				case FieldType.ByteArray: SetByteArrayField(data, table, offset, fieldId, value); break;
			}
		else SetData(data, table, offset, fieldId, null);
#pragma warning restore RCS1003
	}

	public void SetField(string name, long value) => SetField(_data, _type, _offset, name, value, FieldType.Long); // Code size: 27 (0x1b)
	public void SetField(string name, int value) => SetField(_data, _type, _offset, name, value, FieldType.Int); // Code size: 27 (0x1b)
	public void SetField(string name, short value) => SetField(_data, _type, _offset, name, value, FieldType.Short); // Code size: 27 (0x1b)
	public void SetField(string name, sbyte value) => SetField(_data, _type, _offset, name, value, FieldType.Byte); // Code size: 27 (0x1b)
	public void SetField(string name, bool value)
	{
		// Code size: 105 (0x69) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		var fieldType = table.Fields[fieldId].Type;
		if (fieldType == FieldType.Boolean) SetData(data, table, _offset, fieldId, value ? BooleanTrue : BooleanFalse);
		else ThrowImpossibleConversion(FieldType.Boolean, fieldType);
	}
	public void SetField(string name, DateTime value)
	{
		// Code size: 79 (0x4f) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		SetDateTimeField(data, table, _offset, fieldId, table.Fields[fieldId].Type, value, null);
	}
	public void SetField(string name, DateTimeOffset value)
	{
		// Code size: 88 (0x58) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		SetDateTimeField(data, table, _offset, fieldId, table.Fields[fieldId].Type, value.DateTime, value.Offset);
	}
	public void SetField(string name, double value) => SetField(name, value, FieldType.Double); // Code size: 11 (0xb)
	public void SetField(string name, float value) => SetField(name, value, FieldType.Float); // Code size: 12 (0xc)

	public void SetField<T>(string name, T value) where T : IEnumerable<byte>
	{
		// Code size: 99 (0x63) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		var fieldType = table.Fields[fieldId].Type;
		if (fieldType != FieldType.ByteArray) ThrowImpossibleConversion(FieldType.ByteArray, fieldType);
		if (value is null) SetData(data, table, _offset, fieldId, null);
		else SetData(data, table, _offset, fieldId, Convert.ToBase64String(value.ToArray()));
	}

#pragma warning restore IDE0251

	public static bool operator ==(Record left, Record right) => left.Equals(right); // Code size: 9 (0x9)
    public static bool operator !=(Record left, Record right) => !left.Equals(right); // Code size: 12 (0xc)
    public readonly override bool Equals(object? obj) => obj is Record record && Equals(record); // Code size: 25 (0x19)
    public readonly bool Equals(Record other)
	{
		// Code size: 115 (0x73)
		var table = _type;
		if (!ReferenceEquals(table, other._type)) return false;
		var count = table.RecordSize - 1;
		var span1 = new ReadOnlySpan<string?>(_data, _offset, count);
		var span2 = new ReadOnlySpan<string?>(other._data, other._offset, count);
		for (var i = 0; i < count; i++)	if (!string.Equals(span1[i], span2[i], StringComparison.Ordinal)) return false;
		return true;
	}
	public readonly override int GetHashCode()
	{
		// Code size: 122 (0x7a)
		var table = _type;
		var data = new ReadOnlySpan<string?>(_data, _offset, table.RecordSize - 1);
		var hash = new HashCode();

		hash.Add(table.Id); // pair of identification for a table
		hash.Add(table.SchemaId);

		// Process each field
		foreach (var value in data) hash.Add(value?? NullField);
		return hash.ToHashCode();
	}
	internal readonly bool EqualTo(SaveQuery obj) => ReferenceEquals(obj.Data, _data) && obj.Offset == _offset; // Code size: 31 (0x1f)
	internal readonly bool IsFieldChanged(string name)
	{
		// Code size: 91 (0x5b) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var index = table.GetFieldIndex(name);
		var trackerIndex = _offset + table.RecordSize - 1;
		if (index != -1) return _data[trackerIndex] is not null && IsColumnChanged(data, index, trackerIndex);
		ThrowRecordUnknownFieldName(table, name);
		return false;
	}
#pragma warning disable IDE0251 // Make member 'readonly'
	internal void ResetTracker() => _data[_type.RecordSize - 1 + _offset] = null; // Code size: 29 (0x1d)
#pragma warning restore IDE0251
	internal readonly bool IsFieldExist(string name) => _type.GetFieldIndex(name) != -1; // Code size: 19 (0x13)

	internal readonly bool IsRelationChanged(string name)
	{
		// Code size: 116 (0x74) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var relation = table.GetRelation(name);
		var trackerIndex = _offset + table.RecordSize - 1;
		if (relation is null) ThrowRecordUnknownRelationName(name);
		var column = table.GetColumn(relation.Id, EntityType.Relation);
		if (column is null) ThrowRecordWrongRelationType(name);
		var index = column.RecordIndex;
		if (index >= 0) return data[trackerIndex] is not null && IsColumnChanged(data, index, trackerIndex);
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
		// Code size: 115 (0x73) - no virtual calls
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var relation = table.GetRelation(name);
		if (relation is null) ThrowRecordUnknownRelationName(name);
		var column = table.GetColumn(relation.Id, EntityType.Relation);
		if (column is null) ThrowRecordWrongRelationType(name);
		var index = _offset + column.RecordIndex;
		var value = _data[index];
		return value != null ? long.Parse(value, CultureInfo.InvariantCulture) : null;
	}

#pragma warning disable IDE0251 // Make member 'readonly'
	internal void SetRelation(string name, long? value)
	{
		// Code size: 137 (0x89) - no virtual calls
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var relation = table.GetRelation(name);
		if (relation is null) ThrowRecordUnknownRelationName(name);
		var column = table.GetColumn(relation.Id, EntityType.Relation);
		if (column is null) ThrowRecordUnknownRelationName(name);
		var index = column.RecordIndex;
		if (index >= 0) SetRelationData(_data, table, _offset, index, value?.ToString(DefaultCulture));
		else ThrowRecordWrongRelationType(name);
	}
#pragma warning restore IDE0251 // Make member 'readonly'
   
	#region private methods 

#pragma warning disable IDE0251 // Make member 'readonly'
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetField(string name, double value, FieldType fieldType)
	{
		// Code size: 175 (0xaf) - no virtual calls
		var data = _data;
		var table = _type;
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = _type.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		var type = table.Fields[fieldId].Type;
		if (type == FieldType.Double) SetData(data, table, _offset, fieldId, value.ToString(DefaultCulture));
		else if (type == FieldType.Float)
		{
			var flt = (float)value;                                          // truncate to float range
			if (float.IsInfinity(flt) && !double.IsInfinity(value))         // double in range, float not
				ThrowValueTooLarge(type);
			SetData(data, table, _offset, fieldId, flt.ToString(DefaultCulture));
		}
		else ThrowImpossibleConversion(fieldType, type);
	}
#pragma warning restore IDE0251 // Make member 'readonly'

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetField(Span<string?> data, Table table, int offset, string name, long value, FieldType fieldType)
	{
		// Code size: 279 (0x117) - no virtual calls; removed pattern merge!
		if (table.Id == -1) ThrowRecordUnknownRecordType();
		var fieldId = table.GetFieldIndex(name);
		if (fieldId == -1) ThrowRecordUnknownFieldName(table, name);
		var type = table.Fields[fieldId].Type;
		switch (type)
		{
			case FieldType.Long:
				SetData(data, table, offset, fieldId, value.ToString(DefaultCulture));
				break;
			case FieldType.Int:
				if (value <= MaxIntValue && value >= MinIntValue) SetData(data, table, offset, fieldId, value.ToString(DefaultCulture));
				else ThrowValueTooLarge(type);
				break;
			case FieldType.Short:
				if (value <= MaxShortValue && value >= MinShortValue) SetData(data, table, offset, fieldId, value.ToString(DefaultCulture));
				else ThrowValueTooLarge(type);
				break;
			case FieldType.Byte:
				if (value <= MaxByteValue && value >= MinByteValue) SetData(data, table, offset, fieldId, value.ToString(DefaultCulture));
				else ThrowValueTooLarge(type);
				break;
			case FieldType.Float:
			case FieldType.Double:
				SetFloatField(data, table, offset, type, fieldId, value.ToString(DefaultCulture));
				break;
			default:
				ThrowImpossibleConversion(fieldType, type);
				break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetStringField(Span<string?> data, Table table, int offset, int fieldSize, int fieldId, string value)
	{
		// Code size: 48 (0x30)
		if (value.Length <= fieldSize)
		{
			SetData(data, table, offset, fieldId, value);
		}
		else
		{
			// Use span to avoid allocation - add a condition to create exception
			SetData(data, table, offset, fieldId, new string(value.AsSpan(0, fieldSize)));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetIntegerField(Span<string?> data, Table table, int offset, int fieldId, FieldType numberType, string value) {
		// Code size: 129 (0x81) - removed merge pattern!
#pragma warning disable RCS1003 // Add braces to if-else (when expression spans over multiple lines)
		if (!value.IsNumber()) ThrowWrongStringFormat();
		else if (long.TryParse(value, NumberStyles.Integer, DefaultCulture, out var lng) &&
				(numberType == FieldType.Long ||
				(numberType == FieldType.Int && lng <= MaxIntValue && lng >= MinIntValue) ||
				(numberType == FieldType.Short && lng <= MaxShortValue && lng >= MinShortValue) ||
				(numberType == FieldType.Byte && lng <= MaxByteValue && lng >= MinByteValue)))
			SetData(data, table, offset, fieldId, lng.ToString(DefaultCulture));
		else ThrowValueTooLarge(numberType);
#pragma warning restore RCS1003
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetFloatField(Span<string?> data, Table table, int offset, FieldType fieldType, int fieldId, string value)
	{
		// see. ISO 6093:1985
		// Code size: 108 (0x6c) - no virtual calls; - smaller than a logical pattern
		if (double.TryParse(value, NumberStyles.Float, DefaultCulture, out var dbl))
		{
			if (fieldType == FieldType.Double) SetData(data, table, offset, fieldId, dbl.ToString(DefaultCulture));
			else if (fieldType == FieldType.Float)
			{
				var flt = (float)dbl;
				if (float.IsInfinity(flt) && !double.IsInfinity(dbl)) ThrowValueTooLarge(fieldType);
				SetData(data, table, offset, fieldId, flt.ToString(DefaultCulture));
			}
			return;
		}
		ThrowWrongStringFormat();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetDateTimeField(Span<string?> data, Table table, int offset, int fieldId, FieldType fieldType, string value)
	{
		// Code size: 39 (0x27)
		var dateTimeOffset = value.ParseIso8601Date();
		SetDateTimeField(data, table, offset, fieldId, fieldType, dateTimeOffset.DateTime, dateTimeOffset.Offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetByteArrayField(Span<string?> data, Table table, int offset, int fieldId, string value)
	{
		// Code size: 27 (0x1b)
		if (value.IsBase64String()) SetData(data, table, offset, fieldId, value);
		else ThrowInvalidBase64String();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetBooleanField(Span<string?> data, Table table, int offset, int fieldId, string value)
	{
		// Code size: 44 (0x2c) - no virtual calls;
		if (bool.TryParse(value, out var result)) SetData(data, table, offset, fieldId, result ? BooleanTrue : BooleanFalse);
		else ThrowWrongBooleanValue(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetDateTimeField(Span<string?> data, Table table, int rcdOffset, int fieldId, FieldType fieldType, DateTime value, TimeSpan? offset)
	{
		// Code size: 59 (0x3b) - smaller than a logical pattern - no virtual calls;
		if (fieldType == FieldType.DateTime || fieldType == FieldType.DateTimeOffset || fieldType == FieldType.Date)
			SetData(data, table, rcdOffset, fieldId, value.ToString(fieldType, offset));
		else ThrowImpossibleConversion(FieldType.DateTime, fieldType);
	}

	private static void MandatoryField(Table table, int fieldId)
	{
		if (table.Fields[fieldId].DefaultValue is null) {
			// throw exception mandatory field 
			ThrowMandatoryFieldCannotBeNull(table, table.Fields[fieldId].Name);
		}
	}

	private static void InitializeTracking(Span<string?> data, Table table, int trackerIndex) => data[trackerIndex] = new string('\0', (table.Fields.Length>>4)+1); // Code size: 28 (0x1c)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetData(Span<string?> data, Table table, int offset, int fieldId, string? value)
	{
		// Code size: 118 (0x76) - no virtual calls;
		var fieldIndex = fieldId + offset;
		var trackerIndex = table.RecordSize - 1 + offset;
		if (data.Length <= fieldIndex) fieldIndex = fieldId; // another thread changed RecordType avoiding crash; here offset is reset to 0
		if (data[fieldIndex] == value) return; // detect no change
		if (value is null && table.Fields[fieldId].NotNull) MandatoryField(table, fieldId); // manage mandatory fields !!
		if (data[trackerIndex] is null) InitializeTracking(data, table, trackerIndex);
		data[trackerIndex]!.SetBitValue(fieldId); // cannot be null here !!
		data[fieldIndex] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetRelationData(Span<string?> data, Table table, int offset, int recordIndex, string? value)
	{
		// Code size: 92 (0x5c) - no virtual calls;
		var relationIndex = recordIndex + offset;
		var trackerIndex = table.RecordSize - 1 + offset;
		if (data.Length <= relationIndex) relationIndex = recordIndex; // another thread changed RecordType avoiding crash; here offset is reset to 0
		if (data[relationIndex] == value) return;                   // detect no change
																 // relations have no NotNull constraint in Fields[] — skip the mandatory check
		if (data[trackerIndex] is null) InitializeTracking(data, table, trackerIndex);
		data[trackerIndex]!.SetBitValue(recordIndex);            // cannot be null here !!
		data[relationIndex] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsColumnChanged(string?[] data, int fieldId, int trackerIndex) => data[trackerIndex]!.GetBitValue(fieldId); // cannot be null here - Code size: 15 (0xf)

	// exceptions 
	[DoesNotReturn]
	private static void ThrowRecordUnknownFieldName(Table table, string fieldName) => 
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.RecordUnkownFieldName), fieldName, table.Name));

	[DoesNotReturn]
	private readonly void ThrowRecordWrongRelationType(string relationName) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.RecordWrongRelationType), relationName, _type.Name));

	[DoesNotReturn]
	private readonly void ThrowRecordUnknownRelationName(string relationName) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.RecordUnkownRelationName), relationName, _type.Name));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowMandatoryFieldCannotBeNull(Table table, string fieldName) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.FieldIsMandatory), table.Name, fieldName));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowRecordUnknownRecordType() => throw new ArgumentException(ResourceHelper.GetMessage(ResourceType.RecordUnkownRecordType));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowWrongStringFormat() =>	throw new FormatException(ResourceHelper.GetMessage(ResourceType.RecordWrongStringFormat));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowValueTooLarge(FieldType fieldType) => throw new OverflowException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.RecordValueTooLarge), fieldType.RecordTypeDisplay()));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowWrongBooleanValue(string? value) => throw new FormatException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.RecordWrongBooleanValue), value ?? NullString));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowImpossibleConversion(FieldType fieldTypeSource, FieldType fieldTypeDestination) =>
		throw new ArgumentException(string.Format(DefaultCulture,
			ResourceHelper.GetMessage(ResourceType.RecordCannotConvert),
			fieldTypeSource.RecordTypeDisplay(),
			fieldTypeDestination.RecordTypeDisplay()));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidBase64String() => throw new FormatException(ResourceHelper.GetMessage(ResourceType.InvalidBase64String));


#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IDdlBuilder GetDefaultDdlBuilder() => new Util.Builders.PostgreSQL.DdlBuilder(); // Code size: 6 (0x6)
#pragma warning restore CA1859

    private static Table GetDefaultType()
	{
		// Code size: 82 (0x52)
		var metaTable = new Meta(-1, (byte)EntityType.Table, 0, (int)TableType.Undefined, 0L, string.Empty, null, null, true);
		var metaArray = new Meta[] { new(0, (byte)EntityType.Field, 0, 0, 0L, string.Empty, null, null, true) };
		return metaTable.ToTable(new ReadOnlySpan<Meta>(metaArray), PhysicalType.Undefined, GetDefaultDdlBuilder(), string.Empty, 1)!; // cannot be null here!!
	}

	#endregion

}
