using Ring.Data.Enums;
using Ring.Data.Helpers;
using Ring.Data.Models;

namespace Ring.Data.Core.Extensions
{
    internal static class BulkSaveQueryExtension
    {

        /// <summary>
        /// BindRelation()
        /// </summary>
        /// <param name="query"></param>
        public static void BindRelation(this BulkSaveQuery query)
        {
            if (query?.ParentQuery == null || query.Relation == null) return;
            //if (_parentQuery == null || _bindVariable == null) return;
            //_recordRef.SetRelation(_bindVariable, _currentRecord.IsNew ? null: _currentRecord.GetField());
            switch (query.Type)
            {
                case BulkSaveType.BindRelation:
                    // new BulkSaveQuery(query1, BulkSaveType.BindRelation, sourceRecord, query2.CurrentRecord, relation);
                    // query1.currentRecord.objid <-- query2.CurrentRecord.objid
                    query.ParentQuery.CurrentRecord.SetRelation(query.Relation.Name, query.CurrentRecord.GetField());
                    break;
                case BulkSaveType.RelateRecords:
                    // new BulkSaveQuery(query2, BulkSaveType.RelateRecords, sourceRecord.Copy(), targetRecord, relation);
                    query.CurrentRecord.SetRelation(query.Relation.Name, query.ParentQuery.CurrentRecord.GetField());
                    break;
            }
        }

        /// <summary>
        /// GetQuery()
        /// </summary>
        internal static IDbCommand GetQuery(this BulkSaveQuery query, IDbConnection connection) => SqlHelper.GetQuery(connection, query);

        /// <summary>
        ///  Copy fields from query.RefRecord to query.CurrentRecord
        /// </summary>
        internal static void UpdateCurrentRecord(this BulkSaveQuery query) => query?.CurrentRecord?.SetField(query.RefRecord);
    }
}
