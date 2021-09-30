using Ring.Data.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ring.Data
{
    public sealed class List : IEnumerable<Record>, IDisposable
    {
        private Record[][] _data;
        private ItemType _type;   // Supported type are: "long", "integer", "single", "string", "record" and "variant ".
        private int _capacity;
        private int _count;
        private bool _sorted;

        /// <summary>
        /// Ctor
        /// </summary>
        public List()
        {
            _data = null;
            _count = 0;
            _capacity = 0;
            _sorted = false;
            _type = Enums.ItemType.NotDefined;
        }
        internal List(ItemType type)
        {
            _data = null;
            _count = 0;
            _capacity = 0;
            _sorted = false;
            _type = type;
        }
        internal List(ItemType type, int capacity)
        {
            _data = null;
            _count = 0;
            _capacity = capacity;
            _data = new Record[1][];
            _data[0] = new Record[capacity];
            _sorted = false;
            _type = type;
        }
        internal List(List list)
        {
            _data = list._data;
            _count = list._count;
            _capacity = list._capacity;
            _sorted = list._sorted;
            _type = list._type;
            CollapseArrays(false);
        }


        #region properties

        /// <summary>
        /// Supported type are: "long", "integer", "single", "string", "record" and "variant ".
        /// </summary>
        public string ItemType
        {
            get
            {
                return _type != Enums.ItemType.NotDefined ? _type.ToString() : string.Empty;
            }
            set
            {
                if (value == null) throw new ArgumentException(Constants.ErrInvalidType);
                if (!Constants.ListTypeEnumsName.ContainsKey(value.ToLower()))
                    throw new ArgumentException(Constants.ErrInvalidType, value);
                _type = Constants.ListTypeEnumsName[value.ToLower()];
            }
        }
        public int Count => _count;
        public bool Sorted => _sorted;
        internal ItemType Type => _type;

        #endregion
        #region indexers
        internal Record this[int index]
        {
            get
            {
                return ItemByIndex(index);
            }
            set
            {
                ReplaceByIndex(index, value);
            }
        }
        #endregion

        public void Clear()
        {
            // remove all reference !!
            if (_data != null) for (var i = 0; i < _data.Length; ++i) _data[i] = null;
            _data = null;
            _sorted = false;
            _count = 0;
            _capacity = 0;
            _type = Enums.ItemType.NotDefined;
        }
        public int FindFirstIndex(string value)
        {
            return FindFirstIndex(value, true);
        }
        public int FindFirstIndex(int value)
        {
            return FindFirstIndex(value.ToString(), true);
        }
        public int FindFirstIndex(string value, bool caseSensitive)
        {
            var result = Constants.ItemNotFound;
            if (_type == Enums.ItemType.Record)
                throw new ArgumentException(string.Format(Constants.ErrMethodNotSupported, ItemType));
            int j;
            if (!_sorted)
            {
                int i, index;
                if (caseSensitive)
                {
                    for (i = 0, index = 0; i < _data.Length; ++i)
                        for (j = 0; j < _data[i].Length && index < _count; ++j, ++index)
                            if (value == _data[i][j][0])
                            {
                                result = index;
                                i = _data.Length;
                                break;
                            }
                }
                else
                {
                    for (i = 0, index = 0; i < _data.Length; ++i)
                        for (j = 0; j < _data[i].Length && index < _count; ++j, ++index)
                            if (string.Compare(value, _data[i][j][0], StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                result = index;
                                i = _data.Length;
                                break;
                            }
                }
            }
            return result;
        }
        public int FindFirstIndex(string value1, string fieldName1, string value2, string fieldName2, string value3, string fieldName3,
            string value4, string fieldName4, string value5, string fieldName5)
        {
            var result = Constants.ItemNotFound;

            return result;
        }
        public Record ItemByIndex(int index)
        {
            if (index >= 0 && index < _count)
            {
                if (_data.Length == 1) return _data[0][index];
                int i = 0, sum = 0; // i <-- index of current vector
                while (i < _data.Length)
                {
                    sum += _data[i].Length;
                    if (sum > index) return _data[i][index - sum + _data[i].Length];
                    ++i;
                }
                _sorted = false;
            }
            else throw new ArgumentException(
                string.Format(Constants.ErrOutOfRange, index.ToString(), (_count - 1).ToString()));
            return null;
        }
        public void ReplaceByIndex(int index, Record record)
        {
            if (_type == Enums.ItemType.Record || _type == Enums.ItemType.Variant)
            {
                if (index >= 0 && index < _count)
                {
                    if (_data.Length == 1) _data[0][index] = record;
                    else
                    {
                        int i = 0, sum = 0; // i <-- index of current vector
                        while (i < _data.Length)
                        {
                            sum += _data[i].Length;
                            if (sum > index)
                            {
                                _data[i][index - sum + _data[i].Length] = record;
                                break;
                            }
                            ++i;
                        }
                    }
                    _sorted = false;
                }
                else throw new ArgumentException(
                    string.Format(Constants.ErrOutOfRange, index.ToString(), (_count - 1).ToString()));
            }
            else throw new ArgumentException(string.Format(Constants.ErrReplaceInvalidType,
                Enums.ItemType.Record.ToString(), ItemType));
        }
        public void GetItemByIndex(int index, out Record record)
        {
            record = ItemByIndex(index);
        }
        public void GetItemByIndex(int index, out int number)
        {
            ItemByIndex(index).GetField(Constants.DefaultFieldName, out number);
        }
        public void GetItemByIndex(int index, out string item)
        {
            item = ItemByIndex(index).GetField(Constants.DefaultFieldName);
        }
        public void GetItemByIndex(int index, out long number)
        {
            ItemByIndex(index).GetField(Constants.DefaultFieldName, out number);
        }
        public void RemoveByIndex(int index)
        {
            if (index >= 0 && index < _count)
            {
                int i = 0, sum = 0;
                CollapseArrays(false);
                while (i < _data.Length)
                {
                    sum += _data[i].Length;
                    if (sum > index)
                    {
                        int j;
                        for (j = index - sum + _data[i].Length; j < _data[i].Length - 1 && _data[i][j] != null; ++j)
                            _data[i][j] = _data[i][j + 1];
                        _data[i][j] = null;
                        break;
                    }
                    ++i;
                }
                --_count;
                if (_count == 0) Clear();
            }
            else throw new ArgumentException(string.Format(Constants.ErrOutOfRange, index.ToString(), (_count - 1).ToString()));
        }
        public void AppendItem(string newItem)
        {
            if (_type != Enums.ItemType.String && _type != Enums.ItemType.Variant)
                throw new ArgumentException(
                    string.Format(Constants.ErrAppendInvalidType, Enums.ItemType.String.ToString(), ItemType));
            var rcd = new Record(Constants.FakeStringTable, new string[Constants.FakeStringTable.Fields.Length]);
            rcd.SetField(Constants.DefaultFieldName, newItem);
            if (_count >= _capacity) AllowCapicity();
            // find empty slot in the last array - only last aray havbe empty slots
            var i = _data[_data.Length - 1].Length; // i <-- size of last array
            i -= _capacity - _count; // i <-- index of next empty slot
            _data[_data.Length - 1][i] = rcd;
            _sorted = false;
            ++_count;
        }
        public void AppendItem(Record newItem)
        {
            if (_type != Enums.ItemType.Record && _type != Enums.ItemType.Variant)
                throw new ArgumentException(
                    string.Format(Constants.ErrAppendInvalidType, Enums.ItemType.Record.ToString(), ItemType));
            if (_count >= _capacity) AllowCapicity();
            // find empty slot in the last array - only last aray havbe empty slots
            var i = _data[_data.Length - 1].Length; // i <-- size of last array
            i -= _capacity - _count; // i <-- index of next empty slot
            _data[_data.Length - 1][i] = newItem;
            _sorted = false;
            ++_count;

        }
        public void AppendItem(int newItem)
        {
            if (_type != Enums.ItemType.Integer && _type != Enums.ItemType.Variant)
                throw new ArgumentException(
                    string.Format(Constants.ErrAppendInvalidType, Enums.ItemType.Integer.ToString(), ItemType));
            var rcd = new Record(Constants.FakeLongTable, new string[Constants.FakeStringTable.Fields.Length]);
            rcd.SetField(Constants.DefaultFieldName, newItem);
            if (_count >= _capacity) AllowCapicity();
            // find empty slot in the last array - only last aray havbe empty slots
            var i = _data[_data.Length - 1].Length; // i <-- size of last array
            i -= _capacity - _count; // i <-- index of next empty slot
            _data[_data.Length - 1][i] = rcd;
            _sorted = false;
            ++_count;
        }
        public void AppendItem(long newItem)
        {
            if (_type != Enums.ItemType.Long && _type != Enums.ItemType.Variant)
                throw new ArgumentException(
                    string.Format(Constants.ErrAppendInvalidType, Enums.ItemType.Long.ToString(), ItemType));
            var rcd = new Record(Constants.FakeLongTable, new string[Constants.FakeStringTable.Fields.Length]);
            rcd.SetField(Constants.DefaultFieldName, newItem);
            if (_count >= _capacity) AllowCapicity();
            // find empty slot in the last array - only last aray havbe empty slots
            var i = _data[_data.Length - 1].Length; // i <-- size of last array
            i -= _capacity - _count; // i <-- index of next empty slot
            _data[_data.Length - 1][i] = rcd;
            _sorted = false;
            ++_count;
        }
        public void AppendItem(List newItems)
        {
            if (newItems == null || newItems.Count == 0) return;
            if (_type != newItems._type) throw new ArgumentException(
                    string.Format(Constants.ErrAppendInvalidType, Enums.ItemType.Long.ToString(), ItemType));
            //TODO: improve performance
            for (var i = 0; i < newItems.Count; ++i) AppendItem(newItems[i]);
            CollapseArrays(false);
        }


        public void Sort()
        {
            Sort(SortOrderType.Ascending, true);
        }
        public void Sort(bool caseSensitive)
        {
            Sort(SortOrderType.Ascending, caseSensitive);
        }
        public void Sort(SortOrderType sortType)
        {
            Sort(sortType, true);
        }
        public void Sort(SortOrderType sortType, bool caseSensitive)
        {
            switch (_type)
            {
                case Enums.ItemType.Record:
                    throw new ArgumentException(string.Format(Constants.ErrMethodNotSupported,
                        Enums.ItemType.Record.ToString()));
                case Enums.ItemType.Variant:
                    throw new ArgumentException(string.Format(Constants.ErrMethodNotSupported,
                        Enums.ItemType.Variant.ToString()));
            }
            if (_data == null) return;
            if (_data.Length > 1) CollapseArrays(false);
            var sortTypeId = (int)sortType;
            switch (_type)
            {
                case Enums.ItemType.Integer:
                case Enums.ItemType.Long:
                    long l1, l2;
                    Array.Sort(_data[0], delegate (Record rcd1, Record rcd2)
                    {
                        rcd1.GetField(Constants.DefaultFieldName, out l1);
                        rcd2.GetField(Constants.DefaultFieldName, out l2);
                        return l1.CompareTo(l2) * sortTypeId;
                    });
                    break;
                case Enums.ItemType.String:
                    if (caseSensitive)
                        Array.Sort(_data[0], (rcd1, rcd2) => string.CompareOrdinal(rcd1[0], rcd2[0]) * sortTypeId);
                    else
                        Array.Sort(_data[0], (rcd1, rcd2) => string.Compare(rcd1[0], rcd2[0], StringComparison.OrdinalIgnoreCase) * sortTypeId);
                    break;
            }
            _sorted = true;
        }

#if DEBUG

		public override string ToString()
        {
            var sb = new StringBuilder();
            string item;
            switch (_type)
            {
                case Enums.ItemType.Integer:
                case Enums.ItemType.Long:
                    sb.Append('(');
                    for (var i = 0; i < _count; ++i)
                    {
                        GetItemByIndex(i, out item);
                        sb.Append(item);
                        if (i < _count - 1) sb.Append(',');
                    }
                    sb.Append(')');
                    break;
                case Enums.ItemType.String:
                    sb.Append('(');
                    for (var i = 0; i < _count; ++i)
                    {
                        GetItemByIndex(i, out item);
                        sb.Append('\'');
                        sb.Append(item);
                        sb.Append('\'');
                        if (i < _count - 1) sb.Append(',');
                    }
                    sb.Append(')');
                    break;
                case Enums.ItemType.Record:
                    sb.Append("Count = " + Count.ToString());
                    break;
            }
            return sb.ToString();
        }

#endif

        /// <summary>
        /// Deep copy of object
        /// </summary>
        public List Copy()
        {
            List result = new List();
            result._type = _type;
            result._sorted = _sorted;
            if (_data != null)
            {
                result._data = new Record[1][];
                result._data[0] = new Record[_count];
                int i, j, index = 0;
                for (i = 0; i < _data.Length; ++i)
                    for (j = 0; j < _data[i].Length && index < _count; ++j, ++index)
                        result._data[0][index] = _data[i][j].Copy();
            }
            result._count = _count;
            result._capacity = _count;
            return result;
        }
        internal void AddRange(Record[] recordArray)
        {
            if (recordArray == null || recordArray.Length == 0) return;
            if (_capacity > _count)
            {
                // add record in existing record 
                var arr = _data[_data.Length - 1];
                if (_count + recordArray.Length > _capacity)
                {
                    Array.Resize(ref arr, _count + recordArray.Length);
                    _data[_data.Length - 1] = arr;
                }
                for (var i = 0; i < recordArray.Length; ++i) arr[_count + i] = recordArray[i];
                _count += recordArray.Length;
            }
            else
            {
                // allow new structure
                if (_data == null) _data = new Record[1][];
                else Array.Resize(ref _data, _data.Length + 1);
                if (recordArray[0] != null) _count += recordArray.Length;
                _capacity += recordArray.Length;
                _data[_data.Length - 1] = recordArray;
            }
        }
        public void Dispose()
        {
            Clear();
        }
        public IEnumerator<Record> GetEnumerator()
        {
            return Items().GetEnumerator();
        }

        #region private methods
        private void AllowCapicity()
        {
            if (_data == null)
            {
                _data = new Record[1][];
                _data[0] = new Record[2];
                _data[0][0] = null;
                _data[0][1] = null;
                _capacity = 2; // min capacity 
            }
            else
            {
                Record[][] newArray = new Record[_data.Length + 1][];
                int i;
                // copy all sub array
                for (i = 0; i < _data.Length; ++i) newArray[i] = _data[i];
                i = newArray[_data.Length - 1].Length * 2;// i <-- new size
                newArray[_data.Length] = new Record[i]; // two times size of prev allocation
                _capacity += i;
                for (--i; i >= 0; --i) newArray[_data.Length][i] = null;
                _data = newArray;
            }
        }
        private void CollapseArrays(bool keepCapacity)
        {
            if (_data == null || _data.Length <= 1) return;
            var newArray = new Record[1][];
            int i, j, index = 0;
            if (!keepCapacity)
            {
                newArray[0] = new Record[_count];
                _capacity = _count;
            }
            else
            {
                newArray[0] = new Record[_capacity];
                // initialize cells
                for (i = _count; i < newArray.Length; ++i) newArray[0][i] = null;
            }
            for (i = 0; i < _data.Length; ++i)
            {
                for (j = 0; j < _data[i].Length && index < _count; ++j, ++index)
                    newArray[0][index] = _data[i][j];
            }
            _data = null;
            _data = newArray;
        }
        private IEnumerable<Record> Items()
        {
            for (var i = 0; i < _count; ++i) yield return ItemByIndex(i);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }


}
