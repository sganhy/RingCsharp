namespace Ring.Schema.Models
{
    internal struct CacheId
    {
        public readonly object Sync;
        public long CurrentId;
        public long MaxId;
        public int ReservedRange;  // cache a range of id 

        public CacheId(object sync, long currentId, long maxId, int reservedRange)
        {
            CurrentId = currentId;
            MaxId = maxId;
            ReservedRange = reservedRange;
            Sync = sync;
        }
    }
}