using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;
using RecordList = Ring.Data.List;

namespace Ring.Data
{
    public sealed class BulkRetrieve : IDisposable
    {
        private readonly List<BulkRetrieveQuery> _data;
        private Database _schema;
        private bool _disposed;
        private Language _language;

        /// <summary>
        /// Ctor
        /// </summary>
        public BulkRetrieve()
        {
            _data = new List<BulkRetrieveQuery>();
            _disposed = false;
            _schema = Global.Databases.DefaultSchema;
            _language = Global.Databases.DefaultSchema.DefaultLanguage;
        }

        /// <summary>
        /// Current SchemaExtension
        /// </summary>
        internal Database Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        internal Language Language
        {
            get { return _language; }
            set { _language = value; }
        }
        internal void AppendFilter(int entryIndex, string fieldName, OperationType operation, string operand, bool caseSensitiveSearch)
        {
            if (entryIndex < 0 || entryIndex >= _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, entryIndex.ToString()));
            var field = _data[entryIndex].TargetObject.GetField(fieldName);
            if (field == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, fieldName, _data[entryIndex].TargetObject.Name));
            if (field.IsNumeric() && operand != null && !IsNumber(operand))
                throw new ArgumentException(string.Format(Constants.ErrInvalidNumber, operand));
            if (field.IsDateTime())
                throw new ArgumentException(Constants.ErrInvalidDate);
            if (operation == OperationType.In)
                throw new ArgumentException(string.Format(Constants.ErrInvalidList, entryIndex.ToString()));
            if (string.IsNullOrEmpty(operand)) operand = null;
            AppendFilter(entryIndex, field, operation, operand, caseSensitiveSearch);
        }
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, string operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand, true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, int operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand.ToString(), true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, long operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand.ToString(), true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, float operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand.ToString(CultureInfo.InvariantCulture), true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, double operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand.ToString(CultureInfo.InvariantCulture), true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, decimal operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand.ToString(CultureInfo.InvariantCulture), true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, bool operand) =>
            AppendFilter(entryIndex, fieldName, operation, operand.ToString(), true);
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, DateTime operand)
        {
            var dt = operand.ToUniversalTime().ToString(Constants.Iso8601Format);
            var field = _data[entryIndex].TargetObject.GetField(fieldName);
            if (field == null) throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, fieldName,
                    _data[entryIndex].TargetObject.Name));
            AppendFilter(entryIndex, field, operation, dt, true);
        }
        public void AppendFilter(int entryIndex, string fieldName, OperationType operation, RecordList operands)
        {
            if (entryIndex < 0 || entryIndex >= _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, entryIndex.ToString()));
            var field = _data[entryIndex].TargetObject.GetField(fieldName);
            if (field == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, fieldName, _data[entryIndex].TargetObject.Name));
            // Supported type are: "long", "integer", "single", "string", "record" and "variant ".
            if (field.IsNumeric() && operands.Type != ItemType.Integer)
                throw new ArgumentException(string.Format(Constants.ErrInvalidListType, entryIndex.ToString()));
            if (field.IsDateTime())
                throw new ArgumentException(string.Format(Constants.ErrInvalidOperation, entryIndex.ToString()));
            if (operation != OperationType.In)
                throw new ArgumentException(string.Format(Constants.ErrInvalidOperation, entryIndex.ToString()));
            if (operands.Count == 0) throw new ArgumentException(Constants.ErrCriteriaListEmpty);

            var obj = new BulkRetrieveFilter(field, operation, new string[operands.Count]);
            for (var i = 0; i < operands.Count; ++i)
                obj.Operands[i] = operands[i].GetField(Constants.DefaultFieldName);
            _data[entryIndex].Filters.Add(obj);
        }
        public void AppendSort(int entryIndex, string fieldName, SortOrderType sortOrder)
        {
            if (entryIndex < 0 || entryIndex >= _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, entryIndex.ToString()));
            var field = _data[entryIndex].TargetObject.GetField(fieldName);
            if (field == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowFieldName, fieldName, _data[entryIndex].TargetObject.Name));
            var obj = new BulkSort(field, sortOrder);
            _data[entryIndex].Sorts.Add(obj);
        }

        /// <summary>
        /// The SimpleQuery method creates an entry in a BulkRetrieve object, associates it
        /// with the specified entry index number, and places a query in that entry.
        /// </summary>
        /// <param name="entryIndex">The index to be associated with the query.</param>
        /// <param name="objectname">The object type of the database records to be retrieved by the simple query.</param>
        public void SimpleQuery(int entryIndex, string objectname)
        {
            if (entryIndex > _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, _data.Count.ToString()));
            if (entryIndex < _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrIndexAlreadyExist, entryIndex.ToString()));
            var table = _schema.GetTable(objectname);
            if (table == null)
                throw new ArgumentException(string.Format(Constants.ErrInvalidObject, objectname));
            _data.Add(new BulkRetrieveQuery(BulkQueryType.SimpleQuery, table));
        }

        public void Clear() => _data.Clear();

        /// <summary>
        /// The GetRecordList method returns the list of records in the BulkRetrieve object
        /// that was returned by the query (SimpleQuery or TraverseFromRoot) associated with the specified entry index(or name).
        /// </summary>
        /// <param name="entryIndex">Specify the index (or name) of the entry in the BulkRetrieve object that contains the list of records.</param>
        /// <returns></returns>
        public RecordList GetRecordList(int entryIndex)
        {
            if (entryIndex > _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, _data.Count.ToString()));
            return _data[entryIndex].Result;
        }

        /// <summary>
        /// The GetRelatedRecordList method returns a list of records from the BulkRetrieve object, 
        /// using a parent record and a relation name to point to the list you want.You must use this method 
        /// to get any of the multiple lists of records returned by a TraverseFromParent query.
        /// </summary>
        /// <param name="parentRecord">Specify the (parent) record in the BulkRetrieve object from which you want to derive the list of related records.</param>
        /// <param name="relationName">Specify the relation name used to traverse from the parent record to the directly related records in the database.</param>
        /// <returns>This method returns a list of records.</returns>
        public RecordList GetRelatedRecordList(Record parentRecord, string relationName)
        {
            var clfyObj = parentRecord.Table;
            if (clfyObj == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
            var relation = clfyObj.GetRelation(relationName);
            if (relation == null) throw new ArgumentException(Constants.ErrUnknowRelName);
            var query = FindQuery(relation.To, relation);
            if (query != null) return query.GetSubListResult(long.Parse(parentRecord.GetField()));
            return new RecordList();
        }
        public void SetPage(int entryIndex, int pageNumber, int pageSize)
        {
            if (entryIndex < 0 || entryIndex >= _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, entryIndex.ToString()));
            if (pageSize <= 0) throw new ArgumentException(Constants.ErrInvalidPageSize);
            if (pageNumber <= 0) throw new ArgumentException(Constants.ErrInvalidPageNumber);
            _data[entryIndex].PageSize = pageSize;
            _data[entryIndex].PageNumber = pageNumber;
        }

        public void SetSchema(string schemaName)
        {
            var schema = Global.Databases.GetSchema(schemaName);
            if (schema == null) throw new ArgumentException(string.Format(Constants.ErrInvalidSchemaName, schemaName));
            _schema = schema;
        }

        /// <summary>
        /// This method sets the root record for the BulkRetrieve object, using the object type and object ID that you provide.
        /// </summary>
        /// <param name="objectName">The type of the root object. The type is case-sensitive.</param>
        /// <param name="id">The object ID of the root object.</param>
        public void SetRootById(string objectName, long id)
        {
            var table = _schema.GetTable(objectName);
            if (table == null) throw new ArgumentException(string.Format(Constants.ErrInvalidObject, objectName));
            var rcd = new Record(table);
            rcd.SetField(id);
            SetRoot(rcd);
        }

        /// <summary>
        /// The SetRoot method sets the root record for a BulkRetrieve object. The root record specified by this method must be locally available.
        /// </summary>
        /// <param name="rootRecord">The database record that you want to use as the root record.</param>
        public void SetRoot(Record rootRecord)
        {
            var table = rootRecord.Table;
            if (table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
            var query = new BulkRetrieveQuery(BulkQueryType.SetRoot, table);
            _data.Add(query);
            // === add filter ===
            var filter = new BulkRetrieveFilter(query.TargetObject.PrimaryKey, OperationType.Equal, rootRecord.GetField(), true);
            query.Filters.Add(filter);
            query.UniqueFilter = true;
            // === add to result ===
            query.Result.AppendItem(rootRecord);
        }

        /// <summary>
        /// The TraverseFromParent method creates a TraverseFromParent query in a BulkRetrieve object at 
        /// the specified entry index, using the specified parent query and relation name.
        /// The method retrieves each directly related record that is related by 
        /// RelationName to the records retrieved by the parent query.
        /// </summary>
        /// <param name="entryIndex">The index to be associated with the query.</param>
        /// <param name="relationName">The relation name used to traverse from the parent object to the directly related objects in the database.</param>
        /// <param name="parentEntryIndex">The name or index of the parent query.</param>
        public void TraverseFromParent(int entryIndex, string relationName, int parentEntryIndex)
        {
            if (entryIndex > _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, _data.Count.ToString()));
            if (entryIndex < _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrIndexAlreadyExist, entryIndex.ToString()));
            if (parentEntryIndex >= _data.Count)
                throw new ArgumentException(string.Format(Constants.ErrParentEntryIndex,
                    parentEntryIndex.ToString(), entryIndex.ToString()));
            if (_data[parentEntryIndex].Type == BulkQueryType.SetRoot ||
                _data[parentEntryIndex].Type == BulkQueryType.Undefined)
                throw new ArgumentException(string.Format(Constants.ErrTraverseFromRoot, entryIndex.ToString()));

            // check relation 
            var parentQuery = _data[parentEntryIndex];
            var clfyObj = parentQuery.TargetObject;
            var relation = clfyObj.GetRelation(relationName);
            if (relation == null) throw new ArgumentException(Constants.ErrUnknowRelName);
            TraverseFromRelation(relation, parentQuery);
        }

        /// <summary>
        /// The TraverseFromRoot method creates a query in a BulkRetrieve object at the specified entry index.
        /// When the query is performed, it retrieves all of the database records related by RelationName 
        /// to the root record specified for the BulkRetrieve object.
        /// </summary>
        /// <param name="entryIndex">The index to be associated with the query.</param>
        /// <param name="relationName">The relation name used to traverse from the root object to the related objects in the database.</param>
        public void TraverseFromRoot(int entryIndex, string relationName)
        {
            if (entryIndex >= _data.Count) throw new ArgumentException(string.Format(Constants.ErrInvalidIndex, _data.Count.ToString()));
            if (entryIndex < _data.Count - 1)
                throw new ArgumentException(string.Format(Constants.ErrIndexAlreadyExist, entryIndex.ToString()));
            if (_data[entryIndex].Type != BulkQueryType.SetRoot)
                throw new ArgumentException(string.Format(Constants.ErrTraverseFromRoot, entryIndex.ToString()));

            var parentQuery = _data[entryIndex];
            var clfyObj = parentQuery.TargetObject;
            var relation = clfyObj.GetRelation(relationName);
            if (relation == null) throw new ArgumentException(Constants.ErrUnknowRelName);
            TraverseFromRelation(relation, parentQuery);
        }

        /// <summary>
        /// The RetrieveRecords method performs all of the queries specified in the
        /// BulkRetrieve object and stores the retrieved records in the BulkRetrieve object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RetrieveRecords()
        {
            // build sqlcommands object
            var connection = _schema.Connections.Get();
            try
            {
                RetrieveRecords(connection);
            }
            finally
            {
                // return connection immediatly
                _schema.Connections.Put(connection);
            }
        }
        internal void RetrieveRecords(IDbConnection connection)
        {
            #region reset loaded flag
            for (var i = 0; i < _data.Count; ++i)
            {
                var query = _data[i];
                if (query == null) continue;
                query.Launched = false;
                if (query.ParentQuery != null)
                    query.ParentQuery.Launched = false;
            }
            #endregion
            #region execute queries
            for (var i = 0; i < _data.Count; ++i)
            {
                var query = _data[i];
                // get readonly connection 
                if (!query.Launched) query.LoadResult(connection);
            }
            #endregion
        }
        public void Dispose()
        {
            Dispose(true);
        }
        ~BulkRetrieve()
        {
            Dispose(false);
        }

        #region private methods

        private BulkRetrieveQuery FindQuery(Table table, Relation relation)
        {
            for (var i = 0; i < _data.Count; ++i)
            {
                var query = _data[i];
                if (query.Type != BulkQueryType.SimpleQuery &&
                    query.Type != BulkQueryType.SetRoot &&
                    query.TargetObject.Id == table.Id &&
                    query.Relation != null &&
                    query.Relation.Name == relation.Name)
                    return query;
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendFilter(int entryIndex, Field field, OperationType operation, string operand, bool caseSensitifSearch)
        {
            var obj = new BulkRetrieveFilter(field, operation, caseSensitifSearch ? operand : operand.ToUpper(), caseSensitifSearch);
            // Is there unique field in the filter ? 
            if (string.CompareOrdinal(field.Name, _data[entryIndex].TargetObject.PrimaryKey.Name) == 0 && operation == OperationType.Equal)
                _data[entryIndex].UniqueFilter = true;
            _data[entryIndex].Filters.Add(obj);

        }
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            var b = disposing;
            if (!b) return;
            _disposed = true;
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            // ==> GC.SuppressFinalize(this);
        }
        private static bool IsNumber(string digit)
        {
            // we need to compare null integer then in this case null digit is Number !!
            if (digit == null) return true;
            decimal dec;
            return decimal.TryParse(digit, out dec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TraverseFromRelation(Relation relation, BulkRetrieveQuery parentQuery)
        {
            BulkRetrieveQuery query;
            switch (relation.Type)
            {
                case RelationType.Mto:
                case RelationType.Otop:
                    query = new BulkRetrieveQuery(BulkQueryType.TraverseFromParent, relation.To, parentQuery, relation);
                    _data.Add(query);
                    if (!parentQuery.ContainsForeignKey(relation.Id)) parentQuery.ForeignKeys.Add(relation);
                    break;
                case RelationType.Otm:
                case RelationType.Otof:
                    query = new BulkRetrieveQuery(BulkQueryType.TraverseFromParent, relation.To, parentQuery, relation);
                    if (!query.ContainsForeignKey(relation.Id)) query.ForeignKeys.Add(relation.GetInverseRelation());
                    _data.Add(query);
                    break;
                case RelationType.Mtm:
                    // insert a new bulkretrieveQuery between indexes
                    var mtmQuery = new BulkRetrieveQuery(BulkQueryType.TraverseFromParent, _schema.GetMtmTable(relation.MtmTable),
                        parentQuery, relation);
                    mtmQuery.ForeignKeys.Add(relation);
                    mtmQuery.ForeignKeys.Add(relation.GetInverseRelation());
                    query = new BulkRetrieveQuery(BulkQueryType.TraverseFromParent, relation.To, mtmQuery, relation);
                    _data.Add(query);
                    break;
            }
        }

        #endregion

    }

}
