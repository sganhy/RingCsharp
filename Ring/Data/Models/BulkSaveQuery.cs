using Ring.Data.Enums;
using Ring.Schema.Models;

namespace Ring.Data.Models
{
    internal sealed class BulkSaveQuery
    {
        public readonly BulkSaveType Type;
        public readonly Record CurrentRecord;
        public readonly Record RefRecord; // copy ref record 
        public readonly BulkSaveQuery ParentQuery;
        public readonly Relation Relation;
        public bool Cancelled;


        /// <summary>
        /// Ctor
        /// </summary>
        public BulkSaveQuery(BulkSaveQuery parentQuery, BulkSaveType type, Record refRecord, Record currentRecord, Relation relation)
        {
            Type = type;
            CurrentRecord = currentRecord;
            RefRecord = refRecord;
            ParentQuery = parentQuery;
            Relation = relation;
            Cancelled = false;
        }

#if DEBUG

        public override string ToString() => Type.ToString();

#endif


    }
}
