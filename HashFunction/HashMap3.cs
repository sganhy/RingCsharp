﻿using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace ConsoleApplication8
{
    internal class HashMap3
    {

		private class Entry
        {
            public string Key;
            public string Value;
            public Entry Next;
        }

		private Entry[] _buckets;
	    private int _count;

		public HashMap3() : this(16) { }
        public HashMap3(int capacity)
        {
            _count = 0;
			//_buckets = new Entry[capacity < Constants.MinCapacity ? Constants.MinCapacity : capacity];
	        _buckets = new Entry[capacity];
        }

	    public int Count => _count;

		public void Add(string key, string value)
        {
            //var hashcode = Hash32(key);
            //var hashcode = GetHashCode(value);
            var hashcode = HashHelper.Java(key);
            //var hashcode = value.GetHashCode();
            var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            Entry ent = null;

	        ent = _buckets[targetBucket];

			// Search for existing key
			for (; ent != null; ent = ent.Next)
            {
	            if (string.CompareOrdinal(ent.Key, key) != 0) continue;
	            // Key already exists
	            ent.Value = value;
	            return;
            }

            // Rehash if necessary
            if (_count + 1 > _buckets.Length)
            {
                Expand();
                targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            }

            // Create new entry to house key-value pair
            ent = new Entry()
            {
                Key = key,
                Value = value,
            };

            // And add to table
            ent.Next = _buckets[targetBucket];
            _buckets[targetBucket] = ent;
            ++_count;
        }

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

		/// <summary>
		/// Hash code method: djb2
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
	    private static int GetHashCode(string input)
	    {
		    var hash = 5381;
		    for (var i=0;i<input.Length; ++i) hash = (hash << 5) + hash + input[i]; /* hash * 33 + c */
			return hash;
	    }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(string key) => Find(key)?.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Entry Find(string key)
        {
            //var hashcode = Hash32(key);
            //var hashcode = GetHashCode(key);
            var hashcode = HashHelper.Java(key);
            //var hashcode = key.GetHashCode();
            var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            // Search for entry
            for (var ent = _buckets[targetBucket]; ent != null; ent = ent.Next)
                if (string.CompareOrdinal(ent.Key,key)==0) return ent;
            return null;
        }

        private void Expand() => Rehash(_buckets.Length << 1);

        private void Rehash(int newCapacity)
        {
			/*
            // Resize bucket array and redistribute entries
            var oldCapacity = _buckets.Length;
            int targetBucket;
            Entry ent, nextEntry;
            var newBuckets = new Entry[newCapacity];

            for (var i = 0; i < oldCapacity; i++)
            {
                if (_buckets[i] != null)
                {
                    ent = _buckets[i];
                    while (ent != null)
                    {
                        targetBucket = (ent.Hashcode & int.MaxValue) % newCapacity;
                        nextEntry = ent.Next;
                        ent.Next = newBuckets[targetBucket];
                        newBuckets[targetBucket] = ent;
                        ent = nextEntry;
                    }
                }
            }
			
            _buckets = newBuckets;
			*/
        }

	    public int GetMaxLevel()
	    {
		    var result = 0;

		    foreach (var entry in _buckets)
		    {
			    var currentLevcel = 0;
			    for (var ent = entry; ent != null; ent = ent.Next) ++currentLevcel;
			    if (currentLevcel > result)
			    {
				    result = currentLevcel;
			    }
		    }
		    return result;
	    }


	}
}
