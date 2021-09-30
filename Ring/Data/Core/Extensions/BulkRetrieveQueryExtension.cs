using Ring.Data.Enums;
using Ring.Data.Helpers;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Util.Core.Extensions;
using System;
using System.Collections.Generic;
using RecordList = Ring.Data.List;

namespace Ring.Data.Core.Extensions
{
    internal static class BulkRetrieveQueryExtension
    {
        /// <summary>
        /// Calculate the number of bind variable
        /// </summary>
        public static int GetBindVariableCount(this BulkRetrieveQuery query)
        {
            if (query?.Filters == null || query.Filters.Count == 0) return 0;
            var result = 0;
            var filterList = query.Filters;
            for (var i = 0; i < filterList.Count; ++i)
            {
                var filter = filterList[i];
                if (filter.Operand != null) ++result;
                if (filter.Operands != null) result += filter.Operands.Length;
            }
            return result;
        }

        /// <summary>
        /// Get 
        /// </summary>
        public static RecordList GetSubListResult(this BulkRetrieveQuery query, long id)
        {
            var result = new RecordList(ItemType.Record);
            if (query == null) return Constants.DefaultRecordList;
            if (query.Relation != null && query.ParentQuery != null && query.Result.Count > 0)
            {
                switch (query.Relation.Type)
                {
                    case RelationType.Mto:
                    case RelationType.Otop:
                        #region mto
                        {
                            var subResult = new Record[1];
                            var targetObjid = 0L;
                            var elementCount = query.ParentQuery.Result.Count;
                            for (var i = 0; i < elementCount; ++i)
                            {
                                if (long.TryParse(query.ParentQuery.Result[i].GetField(), out targetObjid))
                                {
                                    if (id == targetObjid)
                                    {
                                        targetObjid = query.ParentQuery.Result[i].GetRelation(query.Relation.Name);
                                        break;
                                    }
                                    targetObjid = 0L;
                                }
                            }
                            if (targetObjid == 0L) return result;
                            for (var i = 0; i < query.Result.Count; ++i)
                            {
                                long objid;
                                if (long.TryParse(query.Result[i].GetField(), out objid) && objid == targetObjid)
                                {
                                    subResult[0] = query.Result[i];
                                    break;
                                }
                            }
                            result.AddRange(subResult);
                        }
                        #endregion 
                        break;
                    case RelationType.Otm:
                    case RelationType.Otof:
                        #region otm
                        {
                            var subResult = new List<Record>();
                            var inverseRelation = query.Relation.InverseRelationName;
                            var elementCount = query.Result.Count;
                            for (var i = 0; i < elementCount; ++i)
                                if (query.Result[i].GetRelation(inverseRelation) == id)
                                    subResult.Add(query.Result[i]);
                            result.AddRange(subResult.ToArray());
                        }
                        #endregion 
                        break;
                    case RelationType.Mtm:
                        #region mtm
                        {
                            var tempObjid = new SortedDictionary<long, bool>();
                            var inverseRelationName = query.Relation.InverseRelationName;
                            long objid;
                            var subResult = new List<Record>();
                            var elementCount = query.ParentQuery.Result.Count;
                            for (var i = 0; i < elementCount; ++i)
                            {
                                var rcd = query.ParentQuery.Result[i];
                                if (rcd.GetRelation(inverseRelationName) == id)
                                    tempObjid.TryToAdd(rcd.GetRelation(query.Relation.Name), false);
                            }
                            for (var i = 0; i < query.Result.Count; ++i)
                                if (long.TryParse(query.Result[i].GetField(), out objid) && tempObjid.ContainsKey(objid))
                                    subResult.Add(query.Result[i]);
                            result.AddRange(subResult.ToArray());
                        }
                        #endregion 
                        break;
                }
            }
            return result;
        }

        /// <summary>
        ///  Get distinct list id from a relation name
        /// </summary>
        public static long[] GetDistinctRelationId(this BulkRetrieveQuery query, string relationName)
        {
            if (query?.Result == null || query.Result?.Count == 0) return Constants.DefaultDistinctRelationId;
            var value = query.Result?[0]?.GetRelation(relationName) ?? 0L;
            if (query.Result.Count == 1) return value == 0L ? new long[0] : new[] { value };
            var result = new List<long>();
            // TODO to be improved HERE
            var dico = new SortedDictionary<long, bool>();
            var count = query.Result.Count;
            for (var i = 0; i < count; ++i)
            {
                value = query.Result[i].GetRelation(relationName);
                if (dico.ContainsKey(value) || value == 0L) continue;
                dico.Add(value, false);
                result.Add(value);
            }
            return result.ToArray();
        }

        /// <summary>
        ///  Get distinct list of id 
        /// </summary>
        public static long[] GetDistinctId(this BulkRetrieveQuery query)
        {
            if (query == null) return new long[0];
            if (query.Result == null || query.Result?.Count == 0) return new long[0];
            long value;
            if (query.Result?.Count == 1)
            {
                if (!long.TryParse(query.Result?[0]?.GetField(), out value)) value = 0L;
                return value == 0L ? new long[0] : new[] { value };
            }
            var result = new List<long>();
            // TODO to be improved HERE
            var count = query.Result?.Count;
            // objid is already unique - no need Dictionary
            for (var i = 0; i < count; ++i)
            {
                if (!long.TryParse(query.Result[i].GetField(), out value)) continue;
                result.Add(value);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Is BulkRetrieveQuery contains relation.Id exist in ForeignKeys list
        /// </summary>
        public static bool ContainsForeignKey(this BulkRetrieveQuery query, int relationId)
        {
            if (query.ForeignKeys.Count <= 0) return false; // cannot be null
            var elementCount = query.ForeignKeys.Count;
            var foreignList = query.ForeignKeys;
            for (var i = 0; i < elementCount; ++i)
                if (relationId == foreignList[i].Id) return true;
            return false;
        }

        /// <summary>
        /// Load result into BulkRetrieveQuery structure
        /// </summary>
        public static void LoadResult(this BulkRetrieveQuery query, IDbConnection connection)
        {
            // *** build query 
            query.Sql = SqlHelper.GetQuery(connection, query);
            var sql = query.Sql;
            if (sql == null)
            {
                query.ExecutionTime = Constants.DefaultExecutionTime;
                return;
            }
            var startTime = DateTime.Now;

            // TODO: add check if filter on unique field 
            if (query.MultipleQuery && query.ParentQuery != null && query.Parameters != null)
            {
                #region multiple queries (disconnected mode)
                var numberOfqueries = Math.Ceiling((decimal)query.ParentQuery.Result.Count / query.PartitionSize);
                // allow final structure
                query.Result.AddRange(new Record[query.Parameters.Length]);
                for (var i = 0; i < numberOfqueries; ++i)
                {
                    // *** update parameters with 
                    LoadParameters(query, i);

                    // *** execute query 
                    sql.Connection = connection;
                    sql.ExecuteNonQuery();

                    // *** multiple query 
                    using (var result = connection.CreateNewAdapterInstance(sql))
                    using (var ds = connection.CreateNewDataSetInstance())
                    {
                        result.Fill(ds);
                        LoadResult(query, ds);
                    }
                }
                #endregion 
            }
            else if (query.PageSize > 0 || query.UniqueFilter || query.Filters?.Count > 2)
            {
                #region disconnected mode 
                // *** execute query 
                sql.Connection = connection;
                sql.ExecuteNonQuery();

                // *** fill Result - via DATAsET
                using (var result = connection.CreateNewAdapterInstance(sql))
                using (var ds = connection.CreateNewDataSetInstance())
                {
                    result.Fill(ds);
                    LoadResult(query, ds);
                }
                #endregion 
            }
            else
            {
                #region connected mode 
                // *** execute query 
                sql.Connection = connection;
                sql.ExecuteNonQuery();

                // *** fill Result - via DATArEADER (for big amount of records)
                using (var reader = sql.ExecuteReader())
                {
                    LoadResult(query, reader);
                    reader.Close();
                }
                #endregion 
            }
            sql.Connection = null; // remove reference of database connection
            query.Launched = true;
            query.ExecutionTime = DateTime.Now - startTime;
        }

        #region private methods

        /// <summary>
        /// LoadResult() via DataReader - connected mode 
        /// </summary>
        private static void LoadResult(BulkRetrieveQuery query, IDbDataReader reader)
        {
            if (!reader.HasRows) return;
            if (query == null) return;
            var recordSize = query.TargetObject.Fields.Length;
            var recordList = new List<Record>();
            var targetObject = query.TargetObject;
            var foreignKeys = query.ForeignKeys;
            while (reader.Read())
            {
                var data = new string[recordSize];
                var rcd = new Record(targetObject, data);
                for (var z = 0; z < targetObject.Fields.Length; ++z)
                {
                    var field = targetObject.Fields[z];
                    string currentValue = null;
                    if (field.Type != FieldType.Array) currentValue = reader.Get(field.Name);
                    if (string.CompareOrdinal(currentValue, field.DefaultValue) == 0) currentValue = field.DefaultValue;
                    // manage float - double  
                    switch (field.Type)
                    {
                        case FieldType.Double:
                        case FieldType.Float:
                            currentValue = FormatFloat(currentValue);
                            break;
                        case FieldType.ShortDateTime:
                            currentValue = FormatShortDate(currentValue);
                            break;
                        case FieldType.Boolean:
                            currentValue = string.Equals(bool.TrueString, currentValue, StringComparison.OrdinalIgnoreCase) ? bool.TrueString : bool.FalseString;
                            break;

                    }
                    data[z] = currentValue;
                }
                if (foreignKeys.Count > 0)
                {
                    for (var j = 0; j < foreignKeys.Count; ++j)
                    {
                        var tempResult = reader.Get(foreignKeys[j].Name);
                        rcd.SetRelation(foreignKeys[j].Name, string.IsNullOrEmpty(tempResult) ? 0L : long.Parse(tempResult));
                    }
                }
                recordList.Add(rcd);
            }
            query.ItemCount = recordList.Count; // temp item count 
            query.Result.AddRange(recordList.ToArray());
        }

        /// <summary>
        /// LoadResult() via DataTable - disconnected mode 
        /// </summary>
        private static void LoadResult(BulkRetrieveQuery query, IDbDataSet result)
        {
            if (query == null) return;
            using (var currTable = result.FirstTable)
            {
                if (currTable.RowsCount <= 0) return;

                query.ItemCount = currTable.RowsCount; // temp item count 
                int i;
                var targetObject = query.TargetObject;
                var recordSize = targetObject.Fields.Length;
                var recordList = new Record[currTable.RowsCount];
                var foreignKeys = query.ForeignKeys;

                // replace existing record 
                if (query.Type == BulkQueryType.SetRoot) query.Result.Clear();
                if (foreignKeys.Count > 0)
                {
                    var maxRelId = 0;
                    for (i = 0; i < foreignKeys.Count; ++i)
                        if (foreignKeys[i].Id > maxRelId)
                            maxRelId = targetObject.GetRelationIndex(foreignKeys[i].Name);
                    recordSize += maxRelId + 1;
                }
                for (i = 0; i < recordList.Length; ++i)
                {
                    var data = new string[recordSize];
                    var rcd = new Record(targetObject, data);

                    // load data field by field in rcd object
                    int j;
                    for (j = 0; j < targetObject.Fields.Length; ++j)
                    {
                        var field = targetObject.Fields[j];
                        string currentValue = null;
                        if (field.Type != FieldType.Array) currentValue = currTable.Rows(i, field.Name);
                        if (string.CompareOrdinal(currentValue, field.DefaultValue) == 0) currentValue = field.DefaultValue;
                        switch (field.Type)
                        {
                            case FieldType.Double:
                            case FieldType.Float:
                                currentValue = FormatFloat(currentValue);
                                break;
                            case FieldType.ShortDateTime:
                                currentValue = FormatShortDate(currentValue);
                                break;
                            case FieldType.Boolean:
                                currentValue = string.Equals(bool.TrueString, currentValue, StringComparison.OrdinalIgnoreCase) ? bool.TrueString : bool.FalseString;
                                break;

                        }
                        data[j] = currentValue;
                    }
                    if (foreignKeys.Count > 0)
                    {
                        int k;
                        for (k = 0; k < foreignKeys.Count; ++k)
                        {
                            var tempResult = currTable.Rows(i, foreignKeys[k].Name);
                            rcd.SetRelation(foreignKeys[k].Name, string.IsNullOrEmpty(tempResult) ? 0L : long.Parse(tempResult));
                        }
                    }
                    recordList[i] = rcd;
                }
                query.Result.AddRange(recordList);
            }
        }

        /// <summary>
        /// Load parameters 
        /// </summary>
        private static void LoadParameters(BulkRetrieveQuery query, int index)
        {
            var baseIndex = index * query.PartitionSize;
            for (var i = 0; i < query.PartitionSize; ++i)
                if (baseIndex + i >= query.Parameters.Length)
                    query.Sql.SetParameterValue(i, Constants.EmptyParameter);
                else query.Sql.SetParameterValue(i, query.Parameters[baseIndex + i].ToString());
        }

        private static string FormatFloat(string value) => value == null ? null :
            double.Parse(value).ToString(Constants.FloatFormat);
        private static string FormatShortDate(string value) => value == null || value.Length < 10 ? null :
            value.Substring(0, 10);


        #endregion 

    }
}
