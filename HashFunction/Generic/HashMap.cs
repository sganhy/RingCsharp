using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace ConsoleApplication8.Generic
{
	internal sealed class HashMap<TKey, TValue>: IDictionary<TKey, TValue>, IDictionary
	{
		private class Entry
		{
			public string Key;
			public TValue Value;
			public Entry Next;
			public int Hashcode;
		}

		private readonly Entry[] _buckets;
		private readonly int _capacity;
		private int _count;

		/// <summary>
		/// Ctor
		/// </summary>
		public HashMap(int capacity)
		{
			_count = 0;
			if (capacity*3 < 100) capacity = capacity * 3;
			else if (capacity*2 < 100) capacity = capacity * 2;
			_buckets = new Entry[capacity];
			_capacity = capacity;
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public int Count => _count;
		public object SyncRoot => null;
		public bool IsSynchronized => false;

		[Pure]
		public int Collisions
		{
			get
			{
				var emptyCount = 0;  // number of empty 
				if (_buckets == null) return 0;
				for (var i = 0; i < _buckets.Length; ++i) if (_buckets[i] == null) ++emptyCount;
				return emptyCount - _buckets.Length + _count;
			}
		}

        public void Add(int key, TValue value)
        {
            var hashcode = HashHelper.FullAvalanche(key);
            var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            Entry ent = null;

            // hash code == key 
            // Search for existing key
            for (ent = _buckets[targetBucket]; ent != null; ent = ent.Next)
            {
                if (ent.Hashcode == key)
                {
                    // Key already exists
                    ent.Value = value;
                    return;
                }
            }

            // Create new entry to house key-value pair
            ent = new Entry
            {
                Key = key.ToString(),
                Value = value,
                Hashcode = key,
                Next = _buckets[targetBucket]
            };

            // And add to table
            _buckets[targetBucket] = ent;
            ++_count;
        }
        public void Add(TKey key, TValue value)
		{
			var hashcode = key is string ? HashHelper.Djb2X(key as string) :  key.GetHashCode();
			var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
			Entry ent = null;

			// Search for existing key
			for (ent = _buckets[targetBucket]; ent != null; ent = ent.Next)
			{
				if (ent.Hashcode == hashcode && ent.Key.Equals(key))
				{
					// Key already exists
					ent.Value = value;
					return;
				}
			}

			// Create new entry to house key-value pair
			ent = new Entry
			{
				Key = key.ToString(),
				Value = value,
				Hashcode = hashcode,
				Next = _buckets[targetBucket]
			};

			// And add to table
			_buckets[targetBucket] = ent;
			++_count;
		}

		public bool Remove(TKey key)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			throw new NotImplementedException();
		}

		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue Get(TKey key)
		{
			if (key is string) return Get(key as string);
		    var hashcode = key.GetHashCode();
			var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
			// Search for entry
			for (var ent = _buckets[targetBucket]; ent != null; ent = ent.Next)
				if (ent.Hashcode == hashcode && ent.Key.Equals(key))
					return ent.Value;
			return default(TValue);
		}

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(int key)
        {
            var hashcode = HashHelper.FullAvalanche(key);
            
            // Search for entry
            var ent = _buckets[(hashcode & int.MaxValue) % _capacity];

            while (ent != null)
            {
                if (hashcode==key) return ent.Value;
                ent = ent.Next;
            }
            return default(TValue);
        }

        [Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue Get(string key)
		{
			var hashcode = HashHelper.Djb2X(key);
			// Search for entry
			var ent = _buckets[(hashcode & int.MaxValue) % _capacity];
			while (ent != null)
			{ 
				if (string.CompareOrdinal(ent.Key as string, key)==0) return ent.Value;
				ent = ent.Next;
			}
			return default(TValue);
		}

		/**
		 * {@inheritDoc}
		 */
		[Pure]
		public bool Contains(object key) => IsCompatibleKey(key) && ContainsKey((TKey)key);

		/**
		 * {@inheritDoc}
		 */
		[Pure]
		public bool ContainsKey(TKey key) => IsCompatibleKey(key) && FindEntry(key) >= 0;


		/**
		 * {@inheritDoc}
		 */
		public void Add(object key, object value)
		{
			throw new NotImplementedException();
		}

		/**
		 * {@inheritDoc}
		 */
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			Array.Clear(_buckets,0, _buckets.Length);
			_count = 0;
		}

		/**
		 * {@inheritDoc}
		 */
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		/**
		 * {@inheritDoc}
		 */
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		/**
		 * {@inheritDoc}
		 */
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/**
		 * {@inheritDoc}
		 */
		public void Remove(object key)
		{
			throw new NotImplementedException();
		}

		/**
		 * {@inheritDoc}
		 */
		public object this[object key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		/**
		 * {@inheritDoc}
		 */
		public ICollection Keys { get; }

		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get { return null; }
		}

		/**
		 * {@inheritDoc}
		 */
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get { return null; }
		}

		public ICollection Values { get; }

		/**
		 * {@inheritDoc}
		 */
		public bool IsReadOnly => false;

		/**
		 * {@inheritDoc}
		 */
		public bool IsFixedSize => true;

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool IsCompatibleKey(object key) => key is TKey;

		private int FindEntry(TKey key)
		{
			var hashcode = key is string ? HashHelper.Djb2X(key as string) :  key.GetHashCode();
			var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;

			// Search for entry
			for (var ent = _buckets[targetBucket]; ent != null; ent = ent.Next)
				if (ent.Hashcode == hashcode && ent.Key.Equals(key))
					return targetBucket;
			return -1;
		}

	}

}