namespace Ring.Util
{
    internal class HashMap<TKey, TValue>
    {
        private class Entry
        {
            public TKey Key;
            public TValue Value;
            public Entry Next;
            public int Hashcode;
        }

        private Entry[] _buckets;
        private int _count;

        public HashMap() : this(Constants.MinCapacity) { }
        public HashMap(int capacity)
        {
            _count = 0;
            //_buckets = new Entry[capacity < Constants.MinCapacity ? Constants.MinCapacity : capacity];
            _buckets = new Entry[capacity];
        }

        public void Add(TKey key, TValue value)
        {
            var hashcode = key.GetHashCode();
            var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            Entry ent;

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

            // Rehash if necessary
            if (_count + 1 > _buckets.Length)
            {
                Expand();
                targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            }

            // Add new entry to house key-value pair
            ent = new Entry()
            {
                Key = key,
                Value = value,
                Hashcode = hashcode
            };

            // And add to table
            ent.Next = _buckets[targetBucket];
            _buckets[targetBucket] = ent;
            ++_count;
        }

        public TValue Get(TKey key)
        {
            var ent = Find(key);
            if (ent != null)
                return ent.Value;
            return default(TValue);
        }

        public void Remove(TKey key)
        {
            int hashcode = key.GetHashCode();
            int targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            Entry ent = _buckets[targetBucket];
            Entry last = ent;

            if (ent == null)
                return;

            // Found entry at head of linked list
            if (ent.Hashcode == hashcode && ent.Key.Equals(key))
            {
                _buckets[targetBucket] = ent.Next;
                _count--;
            }
            else
            {
                while (ent != null)
                {
                    if (ent.Hashcode == hashcode && ent.Key.Equals(key))
                    {
                        last.Next = ent.Next;
                        _count--;
                    }
                    last = ent;
                    ent = last.Next;
                }
            }
        }

        private Entry Find(TKey key)
        {
            var hashcode = key.GetHashCode();
            var targetBucket = (hashcode & int.MaxValue) % _buckets.Length;
            // Search for entry
            for (var ent = _buckets[targetBucket]; ent != null; ent = ent.Next)
                if (ent.Hashcode == hashcode && ent.Key.Equals(key))
                    return ent;
            return null;
        }

        private void Expand() => Rehash(_buckets.Length << 1);

        private void Rehash(int newCapacity)
        {
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
        }

    }

}