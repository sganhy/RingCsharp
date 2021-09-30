using Ring.Data.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using RecordList = Ring.Data.List;

namespace Ring.Data.Models
{
    internal sealed class BulkRetrieveQuery : IDisposable
	{
        internal readonly Table TargetObject;
        internal readonly BulkQueryType Type;
        internal readonly List<BulkRetrieveFilter> Filters;
        internal readonly List<BulkSort> Sorts;
        internal readonly RecordList Result;
        internal readonly List<Relation> ForeignKeys;
        internal readonly Relation Relation;
        internal readonly BulkRetrieveQuery ParentQuery;

        // modifiable members
        internal TimeSpan ExecutionTime;
        internal int PageSize;
        internal int PageNumber;
        internal int ItemCount;
        internal int PartitionSize;
        internal bool UniqueFilter;
        internal bool Launched;
        internal bool SubQuery;
        internal bool MultipleQuery;     // multiple query to get the result 
        internal IDbCommand Sql;
        internal long[] Parameters;      // multiple query complete parameter list (contains objid or foreignKey)

        /// <summary>
        /// Ctor
        /// </summary>
        public BulkRetrieveQuery(BulkQueryType type, Table table)
        {
            TargetObject = table;
            Relation = null;
            Type = type;
            Filters = new List<BulkRetrieveFilter>();
            Result = new RecordList(ItemType.Record);
            Sorts = new List<BulkSort>();
            ForeignKeys = new List<Relation>();
            Sql = null;
            ParentQuery = null;
            PageSize = 0;
            ItemCount = 0;
            PageNumber = 0;
            PartitionSize = 0;
            UniqueFilter = false;
            SubQuery = false;
            MultipleQuery = false;
            Launched = false;
            Parameters = null;
            ExecutionTime = new TimeSpan();
        }
        public BulkRetrieveQuery(BulkQueryType type, Table table, BulkRetrieveQuery parentQuery, Relation relation) : this(type, table)
        {
            ParentQuery = parentQuery;
            Relation = relation;
        }

		public void Dispose() => Sql?.Dispose();

#if DEBUG
		public override string ToString()
        {
            return Type + "; " +  Sql?.CommandText;
        }

#endif

	}

}
