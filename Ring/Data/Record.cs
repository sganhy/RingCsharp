using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ring.Data.Core;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;

namespace Ring.Data
{
	public sealed class Record
	{
		private string[] _data; // should be instanciate when record type is defined

		// <relationId, Objid>
		// or <negatifId, bit list> --> detect if a field changed
		private SortedDictionary<int, long> _extraInfo
			; // checked if record field is dirty (only dirty when value changed) + store relation Id

		/// <summary>
		///     Ctor
		/// </summary>
		public Record()
		{
			Table = null;
			_data = null;
			_extraInfo = null;
		}

		internal Record(Table type, string[] data)
		{
			_data = data;
			Table = type;
			_extraInfo = null;
		}

		internal Record(Table type)
		{
			Table = type;
			_extraInfo = null;
			_data = type.Fields.Length > 0 ? new string[type.Fields.Length] : null;
		}

		internal string this[int i] => _data[i];

		public string RecordType
		{
			get { return Table?.Name; }
			set
			{
				var table = Global.Databases[value];
				if (table != null)
				{
					Table = table;
					// !!! field empty is not allow !!!
					// allow fields data
					_data = new string[Table.Fields.Length];
				}
				else
				{
					throw new ArgumentException(string.Format(Constants.ErrInvalidObject, value));
				}
			}
		}

		public bool IsNew
		{
			get
			{
				if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
				if (Table.PrimaryKeyIdIndex != Constants.FieldNameNotFound)
					return _data[Table.PrimaryKeyIdIndex] == null;
				// not manage ==> @lexicon_itm, @log, @meta, @meta_id; 
				return true; // always New if there is no keys
			}
		}

		public bool IsDirty => _extraInfo != null;
		internal Table Table { get; private set; }

		internal void ClearData()
		{
			if (_data != null) for (var i = 0; i < _data.Length; ++i) _data[i] = null;
			_extraInfo?.Clear();
		}

		/// <summary>
		///     GetField methods
		/// </summary>
		public string GetField(string name)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId != Constants.FieldNameNotFound)
				return _data[fieldId] == null ? Table.Fields[fieldId].DefaultValue : _data[fieldId];
			throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
		}

		/// <summary>
		///     Get primary key value (Field name ID)
		/// </summary>
		/// <returns></returns>
		internal string GetField()
		{
			// cannot return null
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.PrimaryKeyIdIndex;
			if (Table.PrimaryKeyIdIndex != Constants.FieldNameNotFound)
				return _data[fieldId] == null ? Constants.RcdDefaultId : _data[fieldId];
			if (Table.PrimaryKey != null) return GetField(Table.PrimaryKey.Name);
			throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, Table?.PrimaryKey?.Name, Table.Name));
		}

		internal void GetField(string name, out sbyte value)
		{
			value = 0;
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var field = Table.Fields[fieldId];
			if (field.Type == FieldType.Byte)
			{
				if (_data[fieldId] != null) value = sbyte.Parse(_data[fieldId]);
				else if (field.DefaultValue != null) value = sbyte.Parse(field.DefaultValue);
			}
			else
			{
				throw new ArgumentException(Constants.ErrInvalidArgumentField);
			}
		}

		public void GetField(string name, out int value)
		{
			value = 0;
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var field = Table.Fields[fieldId];
			if (field.Type == FieldType.Byte || field.Type == FieldType.Int || field.Type == FieldType.Short)
			{
				if (_data[fieldId] != null) value = int.Parse(_data[fieldId]);
				else if (field.DefaultValue != null) value = int.Parse(field.DefaultValue);
			}
			else
			{
				throw new ArgumentException(Constants.ErrInvalidArgumentField);
			}
		}

		public void GetField(string name, out int? value)
		{
			value = null;
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var field = Table.Fields[fieldId];
			if (field.Type == FieldType.Byte || field.Type == FieldType.Int || field.Type == FieldType.Short)
			{
				int temp;
				if (_data[fieldId] != null && int.TryParse(_data[fieldId], out temp)) value = int.Parse(_data[fieldId]);
			}
			else
			{
				throw new ArgumentException(Constants.ErrInvalidArgumentField);
			}
		}

		public void GetField(string name, out long value)
		{
			value = 0;
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var field = Table.Fields[fieldId];
			if (field.Type == FieldType.Byte || field.Type == FieldType.Int || field.Type == FieldType.Short ||
			    field.Type == FieldType.Long)
			{
				if (_data[fieldId] != null) value = long.Parse(_data[fieldId]);
				else if (field.DefaultValue != null) value = long.Parse(field.DefaultValue);
			}
			else
			{
				throw new ArgumentException(Constants.ErrInvalidArgumentField);
			}
		}

		public void GetField(string name, out DateTime value)
		{
			value = DateTime.MinValue;
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var field = Table.Fields[fieldId];
			if (field.Type != FieldType.DateTime || field.Type != FieldType.ShortDateTime ||
			    field.Type != FieldType.LongDateTime)
				throw new ArgumentException(Constants.ErrInvalidArgumentField);
			if (_data[fieldId] != null) value = GetDateFromFieldId(fieldId);
			else if (field.DefaultValue != null) value = DateTime.MinValue;
		}

		public void GetField(string name, out bool value)
		{
			value = false;
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var field = Table.Fields[fieldId];
			if (field.Type != FieldType.Boolean) throw new ArgumentException(Constants.ErrInvalidArgumentField);
			if (bool.TrueString.Equals(_data[fieldId], StringComparison.OrdinalIgnoreCase)) value = true;
		}

		internal bool IsEmpty(string name)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			return string.IsNullOrEmpty(_data[fieldId]);
		}

		internal FieldType GetFieldType(string name)
		{
			if (Table == null)
				throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			return Table.Fields[fieldId].Type;
		}

		/// <summary>
		///     Return relation value by name
		/// </summary>
		/// <param name="name">Name of the relation</param>
		/// <returns>relation value; if not defined return 0L</returns>
		internal long GetRelation(string name)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var relationId = Table.GetRelationIndex(name);
			if (relationId == Constants.RelationNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, name, Table.Name));
			if (_extraInfo != null && _extraInfo.ContainsKey(relationId)) return _extraInfo[relationId];
			return 0L;
		}

		internal bool IsFieldChanged(string name)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId != Constants.FieldNameNotFound) return _extraInfo != null && FieldChange(fieldId);
			throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
		}

		internal Field[] GetFieldChanges()
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			if (_extraInfo == null) return new Field[0];
			var result = new List<Field>();
			var count = Table.Fields.Length;
			for (var i = 0; i < count; ++i)
			{
				if (i == Table.PrimaryKeyIdIndex) continue;
				if (FieldChange(i)) result.Add(Table.Fields[i]);
			}
			return result.ToArray();
		}

		internal Relation[] GetRelationChanges()
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			if (_extraInfo == null) return new Relation[0];
			var result = new List<Relation>();
			var dicoArr = _extraInfo.ToArray();
			var count = dicoArr.Length;
			for (var i = 0; i < count; ++i)
				if (dicoArr[i].Key >= 0)
					result.Add(Table.Relations[dicoArr[i].Key]);
			return result.ToArray();
		}

		internal bool IsFieldExist(string name)
		{
			return Table != null && Table.GetFieldIndex(name) != Constants.FieldNameNotFound;
		}

		/// <summary>
		///     SetField methods
		/// </summary>
		public void SetField(string name, string value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
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

		internal void SetDirty(bool isDirty)
		{
			if (!isDirty) _extraInfo = null;
		}

		/// <summary>
		///     Set primary key ID field (during insertion)
		/// </summary>
		internal void SetField(long value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			if (Table.PrimaryKeyIdIndex != Constants.FieldNameNotFound)
				_data[Table.PrimaryKeyIdIndex] = value.ToString();
		}

		internal void SetField(string value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			_data[Table.PrimaryKeyIdIndex] = value;
		}

		public void SetField(string name, bool value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			if (Table.Fields[fieldId].Type == FieldType.Boolean)
				SetData(fieldId, value ? bool.TrueString : bool.FalseString);
		}

		public void SetField(string name, DateTime value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, name, Table.Name));
			var type = Table.Fields[fieldId].Type;
			if (Table.Fields[fieldId].Type != FieldType.DateTime &&
			    Table.Fields[fieldId].Type != FieldType.LongDateTime &&
			    Table.Fields[fieldId].Type != FieldType.ShortDateTime)
				throw new ArgumentException(string.Format(Constants.ErrInvalidDateField, name));
			if (value == DateTime.MinValue)
			{
				SetData(fieldId, null);
				return;
			}
			// TODO recude size of of date Format
			// IS0-8601 ==> "YYYY-MM-DDTHH:MM:SSZ"
			var sb = new StringBuilder();
			var utcDt = value.ToUniversalTime();
			sb.Append(utcDt.Year.ToString(Constants.RcdFormatYear));
			sb.Append(Constants.RcdDateSeperator);
			sb.Append(utcDt.Month.ToString(Constants.RcdFormatMonth));
			sb.Append(Constants.RcdDateSeperator);
			sb.Append(utcDt.Day.ToString(Constants.RcdFormatDay));
			if (Table.Fields[fieldId].Type != FieldType.ShortDateTime)
			{
				sb.Append(Constants.RcdTimeSeperator);
				sb.Append(utcDt.Hour.ToString(Constants.RcdFormatHour));
				sb.Append(Constants.RcdHourSeperator);
				sb.Append(utcDt.Minute.ToString(Constants.RcdFormatMinute));
				sb.Append(Constants.RcdHourSeperator);
				sb.Append(utcDt.Second.ToString(Constants.RcdFormatSecond));
				if (type == FieldType.LongDateTime)
				{
					sb.Append(Constants.RcdMilliSecondSeperator);
					sb.Append(utcDt.Millisecond.ToString(Constants.RcdFormatMilliSecond));
				}
				sb.Append(Constants.RcdTimeZoneInfo);
			}
			SetData(fieldId, sb.ToString());
		}

		public void SetField(string name, long value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrInvalidArgumentField));
			if (Table.Fields[fieldId].Type != FieldType.Long
			    && Table.Fields[fieldId].Type != FieldType.String
			    && Table.Fields[fieldId].Type != FieldType.Float
			    && Table.Fields[fieldId].Type != FieldType.Double)
				throw new ArgumentException(string.Format(Constants.ErrInvalidIntField, name));
			SetData(fieldId, value.ToString());
		}

		public void SetField(string name, int value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrInvalidArgumentField));
			if (Table.Fields[fieldId].Type != FieldType.Long &&
			    Table.Fields[fieldId].Type != FieldType.Double &&
			    Table.Fields[fieldId].Type != FieldType.Float &&
			    Table.Fields[fieldId].Type != FieldType.Int)
				throw new ArgumentException(string.Format(Constants.ErrInvalidIntField, name));
			SetData(fieldId, value.ToString());
		}

		public void SetField(string name, short value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrInvalidArgumentField));
			if (Table.Fields[fieldId].Type != FieldType.Long &&
			    Table.Fields[fieldId].Type != FieldType.Double &&
			    Table.Fields[fieldId].Type != FieldType.Float &&
			    Table.Fields[fieldId].Type != FieldType.Short &&
			    Table.Fields[fieldId].Type != FieldType.Int)
				throw new ArgumentException(string.Format(Constants.ErrInvalidDateField, name));
			SetData(fieldId, value.ToString());
		}

		public void SetField(string name, double value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrInvalidArgumentField));
			if (Table.Fields[fieldId].Type != FieldType.String
			    && Table.Fields[fieldId].Type != FieldType.Float
			    && Table.Fields[fieldId].Type != FieldType.Double)
				throw new ArgumentException(string.Format(Constants.ErrInvalidIntField, name));
			SetData(fieldId, value.ToString(CultureInfo.InvariantCulture));
		}

		internal void SetField(string name, sbyte value)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(name);
			if (fieldId == Constants.FieldNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrInvalidArgumentField));
			if (Table.Fields[fieldId].Type != FieldType.Long &&
			    Table.Fields[fieldId].Type != FieldType.Double &&
			    Table.Fields[fieldId].Type != FieldType.Float &&
			    Table.Fields[fieldId].Type != FieldType.Byte &&
			    Table.Fields[fieldId].Type != FieldType.Int)
				throw new ArgumentException(string.Format(Constants.ErrInvalidDateField, name));
			SetData(fieldId, value.ToString());
		}

		internal void SetField(Record record)
		{
			if (Table == null || record?.Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			// same type ? 
			if (!ReferenceEquals(Table, record.Table)) throw new ArgumentException(Constants.ErrInvalidArgumentField);
			//copy all fields
			var elementCount = Table.Fields.Length;
			for (var i = 0; i < elementCount; ++i) _data[i] = record._data[i];
		}

		public void ChangeToNew()
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var fieldId = Table.GetFieldIndex(Table.PrimaryKey.Name);
			if (fieldId != Constants.FieldNameNotFound) SetData(fieldId, null);
		}

		public bool IsExactly(Record record)
		{
			bool result;
			if (record.Table == null || Table == null || Table.Name != record.Table.Name) return false;
			if (record._data != null && _data != null)
			{
				var i = 0;
				result = true;
				while (i < record._data.Length && result)
				{
					if (record._data[i] != _data[i]) result = false;
					++i;
				}
			}
			else
			{
				result = record._data != null || _data != null;
			}
			return result;
		}

		internal void SetRelation(string name, long id)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var relationId = Table.GetRelationIndex(name);
			if (relationId == Constants.RelationNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, name, Table.Name));
			if (_extraInfo == null) _extraInfo = new SortedDictionary<int, long>();
			if (_extraInfo.ContainsKey(relationId)) _extraInfo[relationId] = id;
			else _extraInfo.Add(relationId, id);
		}

		internal void SetRelation(string name, string id)
		{
			if (Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
			var relationId = Table.GetRelationIndex(name);
			if (relationId == Constants.RelationNameNotFound)
				throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, name, Table.Name));
			if (_extraInfo == null) _extraInfo = new SortedDictionary<int, long>();

			if (id != null)
			{
				long tempObjid;
				if (!long.TryParse(id, out tempObjid)) return;
				if (_extraInfo.ContainsKey(relationId)) _extraInfo[relationId] = tempObjid;
				else _extraInfo.Add(relationId, tempObjid);
			}
			else
			{
				// set relation to null 
				if (_extraInfo.ContainsKey(relationId))
					_extraInfo[relationId] = long.MinValue;
				else _extraInfo.Add(relationId, long.MinValue);
			}
		}

#if DEBUG
		public override string ToString()
		{
			var sb = new StringBuilder();
			if (Table == null) return sb.ToString();
			if (Table.FieldsById == null || Table.FieldsById.Length == 0) return sb.ToString();
			if (Table.Type == TableType.Mtm) return Table.Name;

			// Case object: first char in upper case
			sb.Append(char.ToUpper(Table.Name[0]) + Table.Name.Substring(1));
			sb.AppendLine(Constants.DisplayObject);
			// Use the select to define order 
			var fieldList = Table.FieldsById;
			for (var i = 0; i < fieldList.Length; ++i)
			{
				var field = fieldList[i];
				if (field == null) continue;

				sb.Append(Constants.FieldIndent);
				sb.Append(field.Name);
				sb.Append(Constants.DefaultDisplaySpace);

				var index = Table.GetFieldIndex(field.Name);
				if (_data[index] == null)
				{
					#region null field

					switch (field.Type)
					{
						case FieldType.String:
							sb.AppendLine(Constants.DefaultDisplayString);
							break;
						case FieldType.ShortDateTime:
						case FieldType.DateTime:
						case FieldType.LongDateTime:
							sb.AppendLine(Constants.DefaultDisplayDate);
							break;
						default:
							sb.AppendLine(Constants.DefaultDisplayNumber);
							break;
					}

					#endregion
				}
				else
				{
					#region format field value

					switch (field.Type)
					{
						case FieldType.String:
							sb.Append(Constants.DisplayStringSeparator);
							sb.Append(GetField(field.Name));
							sb.AppendLine(Constants.DisplayStringSeparator);
							break;
						case FieldType.ShortDateTime:
						case FieldType.DateTime:
						case FieldType.LongDateTime:
							var date = GetDateFromFieldId(index); // position into _data
							//dd
							sb.Append(date.Day.ToString().PadLeft(2, Constants.DisplayDatePadding));
							//mm
							sb.Append(Constants.DisplayDateSeparator);
							sb.Append(date.Month.ToString().PadLeft(2, Constants.DisplayDatePadding));
							//yyyy
							sb.Append(Constants.DisplayDateSeparator);
							sb.Append(date.Year.ToString().PadLeft(4, Constants.DisplayDatePadding));
							sb.Append(Constants.DefaultDisplaySpace);
							//hh24
							sb.Append(date.Hour.ToString().PadLeft(2, Constants.DisplayDatePadding));
							//MI
							sb.Append(Constants.DisplayTimeSeparator);
							sb.Append(date.Minute.ToString().PadLeft(2, Constants.DisplayDatePadding));
							//ss
							sb.Append(Constants.DisplayTimeSeparator);
							sb.Append(date.Second.ToString().PadLeft(2, Constants.DisplayDatePadding));
							break;
						default:
							sb.AppendLine(GetField(field.Name));
							break;
					}

					#endregion
				}
			}
			return sb.ToString();
		}

#endif

		public bool Equals(Record record)
		{
			// compare references 
			if (record.Table == null || Table == null || !ReferenceEquals(Table, record.Table)) return false;
			if (record._data == null || _data == null) return record._data == null && _data == null;
			var i = 0;
			var result = true;
			while (i < record._data.Length && result)
			{
				if (record._data[i] != _data[i]) result = false;
				++i;
			}
			return result;
		}

		/// <summary>
		///     Add a deep copy of record
		/// </summary>
		public Record Copy()
		{
			var result = new Record {Table = Table};
			if (result.Table == null) return result;
			if (_data != null)
			{
				result._data = new string[_data.Length];
				Array.Copy(_data, result._data, _data.Length);
			}
			result._extraInfo = _extraInfo?.Copy();
			return result;
		}

		#region private methods 

		private void SetIntegerField(int fieldId, string value)
		{
			long lng;
			if (long.TryParse(value, out lng)) SetData(fieldId, lng.ToString());
		}
		private void SetStringField(int fieldId, string value)
		{
			if (value != null && value.Length <= Table.Fields[fieldId].Size)
				SetData(fieldId, value);
			else //truncate value
				SetData(fieldId, value?.Substring(0, Table.Fields[fieldId].Size));
		}

		private void SetFloatField(int fieldId, string value)
		{
			double dc;
			if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out dc))
				SetData(fieldId, dc.ToString(CultureInfo.InvariantCulture));
		}
		private void SetDateTimeField(string name, string value)
		{
			DateTime dt;
			if (DateTime.TryParse(value, out dt)) SetField(name, dt);
		}
		private void SetBooleanField(string name, string value)
		{
			bool bl;
			if (value != null && Constants.BooleanTrue.Equals(value.Trim())) SetField(name, true);
			else if (value != null && Constants.BooleanFalse.Equals(value.Trim())) SetField(name, false);
			else if (bool.TryParse(value, out bl)) SetField(name, bl);
		}

		private DateTime GetDateFromFieldId(int fieldId)
		{
			if (_data[fieldId] == null || _data[fieldId].Length < 0) return DateTime.MinValue;
			DateTime result;
			if (!DateTime.TryParse(_data[fieldId],
				CultureInfo.CurrentCulture,
				DateTimeStyles.AssumeUniversal,
				out result))
				result = DateTime.MinValue;
			return result;
		}

		private bool FieldChange(int fieldId)
		{
			var key = fieldId;
			key >>= Constants.RcdDefaultShiftLeft;
			key = -key; // --> set to negatif
			--key;
			if (_extraInfo?.ContainsKey(key) == true)
				return (_extraInfo?[key] & (1L << (fieldId & Constants.Mask64Bits))) != 0L;
			return false;
		}

		private void SetData(int fieldId, string value)
		{
			if (_data[fieldId] == value) return; // detect no change 
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
			_data[fieldId] = value;
		}

		#endregion
	}
}