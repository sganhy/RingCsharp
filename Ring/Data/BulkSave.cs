using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data
{
    /// <summary>
    /// Bulksave object encapsulates a set of database updates into one database operation
    ///         * Perf (sqlite) ==> +- 10500 insert per second
    /// </summary>
    public sealed class BulkSave
    {
        private readonly List<BulkSaveQuery> _data;
        private bool _bindRelation;
        private Database _schema;

        /// <summary>
        /// Ctor
        /// </summary>
        public BulkSave()
        {
            _data = new List<BulkSaveQuery>();
            _bindRelation = false;
            _schema = Global.Databases.DefaultSchema;
        }

        /// <summary>
        /// Current SchemaExtension
        /// </summary>
        internal Database Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }

        /// <summary>
        /// The CancelRecord method removes one record from the BulkSave object only (not from the database) and 
        /// is used prior to the Save method or in the Form_Save callback.
        /// </summary>
        /// <param name="recordToCancel">The record you want to remove from the BulkSave object</param>
        public void CancelRecord(Record recordToCancel)
        {
            if (recordToCancel == null) return;
            var rcdList = FindAllQueriesByRecord(recordToCancel);
            for (var i = 0; i < rcdList.Length; ++i)
                rcdList[i].UpdateCurrentRecord();
        }

        /// <summary>
        /// The ChangeRecord method updates a record that was previously placed in the BulkSave object by the 
        /// InsertRecord or UpdateRecord methods.
        /// </summary>
        /// <param name="recordToChange">Specifies the record that you want to change in the BulkSave object.</param>
        public void ChangeRecord(Record recordToChange)
        {
            if (recordToChange == null) return;
            var rcdList = FindAllQueriesByRecord(recordToChange);
            for (var i = 0; i < rcdList.Length; ++i)
                rcdList[i].Cancelled = true;
        }

        /// <summary>
        /// If you provide the optional parameter ObjectType, this method returns the number of records of the specified type 
        /// in a BulkSave object. If you do not provide the object type, this method returns the count of all of the records 
        /// in the BulkSave object.
        /// </summary>
        /// <returns>This method returns an integer value indicating the number of records of the specified type in this object</returns>
        public int CountByType(string objectType)
        {
            var result = 0;
            if (string.IsNullOrEmpty(objectType)) return result;
            for (var i = 0; i < _data.Count; ++i)
                if ((_data[i].Type == BulkSaveType.InsertRecord || _data[i].Type == BulkSaveType.UpdateRecord || _data[i].Type == BulkSaveType.DeleteRecord) &&
                    _data[i].CurrentRecord.RecordType.IndexOf(objectType, StringComparison.OrdinalIgnoreCase) >= 0) ++result;
            return result;
        }

        /// <summary>
        /// The DeleteRecord  method is used to delete a record in the database.
        /// </summary>
        /// <param name="record">Specify the record you want to delete from the database.</param>
        public void DeleteRecord(Record record)
        {
            // cannot use DeleteRecordById() coz of @meta objects
            if (record.Table == null) return;
            if (record.IsNew && record.Table.Type == TableType.Business) return;
            _data.Add(new BulkSaveQuery(null, BulkSaveType.DeleteRecord, record, record.Copy(), null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordType"></param>
        /// <param name="id"></param>
        public void DeleteRecordById(string recordType, long id)
        {
            var rcd = new Record { RecordType = recordType };
            rcd.SetField(id);
            if (!rcd.IsNew)
                _data.Add(new BulkSaveQuery(null, BulkSaveType.DeleteRecord, rcd, rcd.Copy(), null));
        }

		/// <summary>
		/// The GetRecordByIndex method returns the record in the BulkSave object that is associated with the index value you provide.
		/// </summary>
		/// <param name="index">The index value of the record.</param>
		/// <param name="objectType">The index value of the record.</param>
		/// <returns>Returns the record at the specified index</returns>
		public Record GetRecordByIndex(int index, string objectType)
        {
            int currentIndex = 0;
            for (var i = 0; i < _data.Count; ++i)
            {
                if ((_data[i].Type == BulkSaveType.InsertRecord || _data[i].Type == BulkSaveType.UpdateRecord ||
                     _data[i].Type == BulkSaveType.DeleteRecord) &&
                    _data[i].CurrentRecord.RecordType.IndexOf(objectType, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (currentIndex == index) return _data[i].RefRecord;
                    ++currentIndex;
                }
            }
            return null;
        }
        public void InsertRecord(Record record)
        {
            if (record?.Table == null) throw new ArgumentException(Constants.BulksErrUnknowRecordType);
            if (record.Table.Readonly)
                throw new ArgumentException(string.Format(Constants.BulksReadOnlyErrorMsg, record.Table.Name));
            if (record.IsNew)
                _data.Add(new BulkSaveQuery(null, BulkSaveType.InsertRecord,
                    record, record.Copy(), null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceRecord">the record that is the source of the relation</param>
        /// <param name="targetRecord"></param>
        /// <param name="relationName"></param>
        public void RelateRecords(Record sourceRecord, Record targetRecord, string relationName)
        {
            // (TESTED)
            if (sourceRecord.Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
            var currentObject = sourceRecord.Table;
            var relation = currentObject.GetRelation(relationName);
            if (relation == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, relationName, currentObject.Name));
            switch (relation.Type)
            {
                case RelationType.Mto:
                case RelationType.Otop:
                    RelateMtoRecord(sourceRecord, targetRecord, relation);
                    break;
                case RelationType.Otm:
                case RelationType.Otof:
                    RelateMtoRecord(targetRecord, sourceRecord, relation.GetInverseRelation());
                    break;
                case RelationType.Mtm:
                    RelateMtmRecord(sourceRecord, targetRecord, relation);
                    break;
            }
        }
        public void RelateRecordsFromId(string sourceRecordType, long sourceRecordId, Record targetRecord, string relationName)
        {
            var sourceRecord = new Record { RecordType = sourceRecordType };

            // first relation validation
            var currentObject = sourceRecord.Table;
            var relation = currentObject.GetRelation(relationName);
            if (relation == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, relationName, currentObject.Name));

            // second relation validation
            currentObject = targetRecord?.Table;
            var inverseRelationName = relation.GetInverseRelation().Name;
            relation = currentObject.GetRelation(inverseRelationName);
            if (relation == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, inverseRelationName, currentObject?.Name));

            sourceRecord.SetField(sourceRecordId);
            RelateRecords(sourceRecord, targetRecord, relationName);
        }
        public void RelateRecordsFromToId(string sourceRecordType, long sourceRecordId, string targetRecordType, long targetRecordId, string relationName)
        {
            // (TESTED)
            var sourceRecord = new Record { RecordType = sourceRecordType };
            var targetRecord = new Record { RecordType = targetRecordType };

            // first relation validation
            var currentObject = sourceRecord.Table;
            var relation = currentObject?.GetRelation(relationName);
            if (relation == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, relationName, currentObject?.Name));

            // second relation validation
            currentObject = targetRecord.Table;
            var inverseRelationName = relation.GetInverseRelation().Name;
            relation = currentObject?.GetRelation(inverseRelationName);
            if (relation == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, inverseRelationName, currentObject?.Name));

            sourceRecord.SetField(sourceRecordId);
            targetRecord.SetField(targetRecordId);
            RelateRecords(sourceRecord, targetRecord, relationName);
        }
        public void RelateRecordsToId(Record sourceRecord, string targetRecordType, long objid, string relationName)
        {
            // (TESTED)
            // compare reference 
            // Object (object)a == (object)c
            // reference to compare 
            if (sourceRecord?.RecordType == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
            var currentObject = sourceRecord.Table;
            var relationId = currentObject.GetRelationIndex(relationName);
            if (relationId == Constants.RelationNameNotFound)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, relationName, currentObject.Name));
            var rcd = new Record { RecordType = targetRecordType };
            rcd.SetField(objid);
            RelateRecords(sourceRecord, rcd, relationName);
        }

        /// <summary>
        /// Commits the records and relations contained in the BulkSave object to database
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save()
        {
            // build sqlcommands object
            if (_data.Count == 0) return; // do nothing (no queries)
            var connection = _schema.Connections.Get();
            try
            {
                Save(connection);
            }
            finally
            {
                // return connection immediatly
                _schema.Connections.Put(connection);
            }
            #region clear object
            _bindRelation = false;
            #endregion
        }
        internal void Save(IDbConnection connection)
        {
            if (_data.Count == 0) return;

            //  generate id
            var isIdGenerate = GenerateId(connection);
            
            // fields & relations to binded
            if (_bindRelation)
            {
                for (var i = 0; i < _data.Count; ++i)
                {
                    var query = _data[i];
                    if (query.Type == BulkSaveType.BindRelation || query.Type == BulkSaveType.RelateRecords)
                        query.BindRelation();
                }
            }
            
            //TODO if more than 100K multiple transactions
            //TODO throw exception ==> invalid insert into with id==0
            if (_data.Count == 1) SaveWithoutTransaction(connection);
            if (_data.Count > 1) SaveWithTransaction(connection);
	        if (isIdGenerate) GenerateMissingId();

            // clear pipe 
            _data.Clear();
        }
        public void UnrelateRecords(Record sourceRecord, Record targetRecord, string relationName)
        {
            // (TESTED)
            if (sourceRecord?.Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);
            var currentObject = sourceRecord.Table;
            var relation = currentObject.GetRelation(relationName);
            if (relation == null)
                throw new ArgumentException(string.Format(Constants.ErrUnknowRelName, relationName, currentObject.Name));
            switch (relation.Type)
            {
                case RelationType.Mto:
                case RelationType.Otop:
                    UnRelateMtoRecord(sourceRecord, relation);
                    break;
                case RelationType.Otm:
                case RelationType.Otof:
                    UnRelateMtoRecord(targetRecord, relation.GetInverseRelation());
                    break;
                case RelationType.Mtm:
                    UneRelateMtmRecord(sourceRecord, targetRecord, relation);
                    break;
            }
        }
        public void UpdateRecord(Record record)
        {
            if (record?.Table == null) throw new ArgumentException(Constants.BulksErrUnknowRecordType);
            if (record.Table.Readonly) throw new ArgumentException(string.Format(Constants.BulksReadOnlyErrorMsg, record.Table.Name));
            if (record.IsNew) throw new ArgumentException(Constants.BulksErrorMsg);
            _data.Add(new BulkSaveQuery(null, BulkSaveType.UpdateRecord, record, record.Copy(), null));
        }

		#region private methods 

		/// <summary>
		/// update _data[x]._recordRef.objid && set record as !dirty
		/// </summary>
		private void GenerateMissingId()
	    {
		    for (var i = 0; i < _data.Count; ++i)
		    {
			    var query = _data[i];
			    if (query.Type == BulkSaveType.InsertRecord)
				    query.RefRecord.SetField(query.CurrentRecord.GetField());
			    if (query.CurrentRecord != null) query.RefRecord.SetDirty(false);
		    }
		}
		private void SaveWithoutTransaction(IDbConnection connection)
		{
			IDbCommand sql;
			//TODO if more than 100K multiple transactions
			//TODO throw exception ==> invalid insert into with id==0
			sql = _data[0].GetQuery(connection);
			// null - do nothing 
			if (sql != null && !_data[0].Cancelled)
			{
				sql.Connection = connection;
				sql.CommandTimeout = Constants.CommandLineTimeOut;
				sql.ExecuteNonQuery();
				sql.Dispose();
			}
		}
	    private void SaveWithTransaction(IDbConnection connection)
	    {
		    IDbCommand sql;
			using (var trans = connection.BeginTransaction())
			{
				try
				{
					var lstSql = new List<IDbCommand>();
					for (var i = 0; i < _data.Count; ++i)
					{
						var bsQuery = _data[i];
						sql = bsQuery.GetQuery(connection);
						if (sql == null || bsQuery.Cancelled) continue;
						sql.Transaction = trans;
						sql.Connection = connection;
						sql.ExecuteNonQuery();
						// remove the connection reference
						sql.Connection = null;
						lstSql.Add(sql);
					}
					trans.Commit();
					for (var i = 0; i < lstSql.Count; ++i) lstSql[i].Dispose();
				}
				catch
				{
					//_logger.Error(ex);
					trans.Rollback();
					throw;
				}
			}
		}

		/// <summary>
		/// Get count of insert by typeId
		/// </summary>
		private KeyValuePair<int, int> [] GetInsertCountByType(int insertCount)
		{
			var result = new Dictionary<int, int>(insertCount); // <type_id,count>
			for (var i = 0; i < _data.Count; ++i)
			{
				var query = _data[i];
				if (query.Type == BulkSaveType.InsertRecord && query.CurrentRecord.Table.Type == TableType.Business)
				{
					var typeId = query.CurrentRecord.Table.Id;
					if (!result.ContainsKey(typeId)) result.Add(typeId, 0);
					++result[typeId];
				}
			}
			return result.ToArray();
		}
	    private SortedDictionary<int, long> GenerateId(IDbConnection connection,KeyValuePair<int, int>[] idCountArr)
	    {
			var result = new SortedDictionary<int, long>(); // <type_id,id>
		    for (var i = 0; i < idCountArr.Length; ++i)
		    {
			    var keyValue = idCountArr[i];
			    var table = _schema.GetTable(keyValue.Key);
			    var newId = table.GetId(connection, keyValue.Value);

			    result.Add(keyValue.Key, newId);
		    }
		    return result;
	    }
	    private void SetId(SortedDictionary<int, long> dicoId)
	    {
		    for (var i = 0; i < _data.Count; ++i)
		    {
			    var query = _data[i];
			    if (query.Type == BulkSaveType.InsertRecord)
			    {
				    query.CurrentRecord.SetField(dicoId[query.CurrentRecord.Table.Id]);
				    ++dicoId[query.CurrentRecord.Table.Id];
			    }
		    }
		}
	    private bool GenerateId(IDbConnection connection)
        {
            var insertCount = 0;
            // number of insert 
            // NO id for meta table !! 
            for (var i = 0; i < _data.Count; ++i)
            {
                var query = _data[i];
                if (query.Type == BulkSaveType.InsertRecord && query.CurrentRecord.Table.Type == TableType.Business) ++insertCount;
            }
            if (insertCount > 0)
            {
	            var idCountArr = GetInsertCountByType(insertCount);
	            var dicoId = GenerateId(connection, idCountArr);
	            SetId(dicoId);
            }
            return insertCount > 0;
        }
        private BulkSaveQuery FindQueryByRecord(Record record)
        {
            for (var i = _data.Count - 1; i >= 0; --i)
                if (_data[i].Type != BulkSaveType.RelateRecords &&
                    _data[i].Type != BulkSaveType.BindRelation &&
                    ReferenceEquals(record, _data[i].RefRecord))
                    return _data[i];
            return null;
        }
        private BulkSaveQuery[] FindAllQueriesByRecord(Record record)
        {
            var result = new List<BulkSaveQuery>();
            for (var i = _data.Count - 1; i >= 0; --i)
                if (ReferenceEquals(record, _data[i].RefRecord)) result.Add(_data[i]);
            return result.ToArray();
        }
        private void RelateMtoRecord(Record sourceRecord, Record targetRecord, Relation relation)
        {
            if (sourceRecord == null || targetRecord == null || relation == null) return;

            var record1 = sourceRecord.IsNew;
            var record2 = targetRecord.IsNew;

			// is record1 && record are new ?
			//  FALSE | TRUE
			if (!record1 && record2)
			{
				RelateMtoRecord01(sourceRecord, targetRecord, relation);
				return;
            }
			// TRUE | FALSE
			if (record1 && !record2)
            {
				RelateMtoRecord10(sourceRecord, targetRecord, relation);
				return;
            }
	        // FALSE | FALSE
			if (!record1)
            {
				// no late binding
				RelateMtoRecord00(sourceRecord, targetRecord, relation);
				return;
            }
			// TRUE | TRUE
			RelateMtoRecord11(sourceRecord, targetRecord, relation);
		}
	    private void RelateMtoRecord01(Record sourceRecord, Record targetRecord, Relation relation)
	    {
		    var query1 = FindQueryByRecord(sourceRecord); // source
		    var query2 = FindQueryByRecord(targetRecord); // target is new 
		    _bindRelation = true;                         // late binding here !! 

		    //TODO throw exception HERE !!!
		    if (query2 == null) return;

		    // avoid to retrieve RefRecord - then send a copy() (TESTED)
		    if (query1 == null)
		    {
			    // sourceRecord.relation <-- new parent.currentRecord.objid 
			    // update InsertRecord later
			    _data.Add(new BulkSaveQuery(query2, BulkSaveType.RelateRecords, sourceRecord.Copy(),
				    targetRecord, relation));
		    }
		    else
		    {
			    // SetRelation later after generate Id (TESTED)
			    // update UpdateRecord later
			    // query1 & query2 not null
			    if (query1.Type == BulkSaveType.DeleteRecord) return; // do nothing, the record will be deleted
			    _data.Add(new BulkSaveQuery(query1, BulkSaveType.BindRelation, sourceRecord,
				    query2.CurrentRecord, relation));
		    }
		}
	    private void RelateMtoRecord10(Record sourceRecord, Record targetRecord, Relation relation)
	    {
			// no late binding - (TESTED)
		    var query = FindQueryByRecord(sourceRecord);
		    //TODO throw exception HERE
		    query?.CurrentRecord.SetRelation(relation.Name, targetRecord.GetField());
		}
	    private void RelateMtoRecord00(Record sourceRecord, Record targetRecord, Relation relation)
	    {
			var query = FindQueryByRecord(sourceRecord);
		    if (query != null)
		    {
			    // (TESTED)
			    if (query.Type == BulkSaveType.DeleteRecord) return; // do nothing, the record will be deleted
			    query.CurrentRecord.SetRelation(relation.Name, targetRecord.GetField());
		    }
		    else
		    {
			    // avoid to retrieve RefRecord - then send a copy() (TESTED)
			    var currentRecord = sourceRecord.Copy(); // avoid to update 
			    currentRecord.SetRelation(relation.Name, targetRecord.GetField());
			    _data.Add(new BulkSaveQuery(null, BulkSaveType.RelateRecords, sourceRecord.Copy(),
				    currentRecord, null));
		    }
		}
	    private void RelateMtoRecord11(Record sourceRecord, Record targetRecord, Relation relation)
	    {
		    // (TESTED)
		    var query1 = FindQueryByRecord(sourceRecord); // source
		    var query2 = FindQueryByRecord(targetRecord); // target is new 
		    _bindRelation = true;                         // late binding here !! 

		    //TODO throw exception HERE !!!
		    if (query1 == null || query2 == null) return;

		    // query1 get relation later
		    _data.Add(new BulkSaveQuery(query1, BulkSaveType.BindRelation, sourceRecord, query2.CurrentRecord, relation));
		}
		private void RelateMtmRecord(Record sourceRecord, Record targetRecord, Relation relation)
        {
            if (sourceRecord == null || targetRecord == null || relation == null) return;
            var record1 = sourceRecord.IsNew;
            var record2 = targetRecord.IsNew;
            _bindRelation = true;                        // always late binding here !! 

            if (!record1 && record2)
            {
                #region FALSE|TRUE
                // (TESTED)
                var query = FindQueryByRecord(targetRecord); // source
                                                             //TODO throw exception HERE !!!
                if (query == null) return;
                var table = _schema.GetMtmTable(relation.MtmTable);
                var rcdMtm = new Record(table);
                rcdMtm.SetRelation(relation.InverseRelationName, sourceRecord.GetField());
                var newQuery = new BulkSaveQuery(null, BulkSaveType.InsertMtm, rcdMtm, rcdMtm, null);
                _data.Add(newQuery);
                _data.Add(new BulkSaveQuery(newQuery, BulkSaveType.BindRelation, sourceRecord, query.CurrentRecord, relation));
                #endregion
                return;
            }
            if (record1 && !record2)
            {
                #region TRUE|FALSE
                // (TESTED)
                // same than other - recursive call
                RelateMtmRecord(targetRecord, sourceRecord, relation.GetInverseRelation());
                #endregion
                return;
            }
            if (!record1)
            {
                #region FALSE|FALSE
                // easy here !!
                var table = _schema.GetMtmTable(relation.MtmTable);
                var rcdMtm = new Record(table);
                rcdMtm.SetRelation(relation.Name, targetRecord.GetField());
                rcdMtm.SetRelation(relation.InverseRelationName, sourceRecord.GetField());
                _data.Add(new BulkSaveQuery(null, BulkSaveType.InsertMtmIfNotExist, rcdMtm, rcdMtm, relation));
                #endregion
                return;

            }
            #region TRUE|TRUE
            {
                // (TESTED)
                var query1 = FindQueryByRecord(sourceRecord); // source
                var query2 = FindQueryByRecord(targetRecord); // source
                                                              //TODO throw exception HERE !!!
                if (query1 == null || query2 == null) return;
                var table = _schema.GetMtmTable(relation.MtmTable);
                var rcdMtm = new Record(table);
                var newQuery = new BulkSaveQuery(null, BulkSaveType.InsertMtm, rcdMtm, rcdMtm, null);
                _data.Add(newQuery);
                _data.Add(new BulkSaveQuery(newQuery, BulkSaveType.BindRelation, sourceRecord, query1.CurrentRecord, relation.GetInverseRelation()));
                _data.Add(new BulkSaveQuery(newQuery, BulkSaveType.BindRelation, targetRecord, query2.CurrentRecord, relation));
            }
            #endregion
        }
        private void UneRelateMtmRecord(Record sourceRecord, Record targetRecord, Relation relation)
        {
            // (TESTED)
            var record1 = sourceRecord.IsNew;
            var record2 = targetRecord.IsNew;
            if (!record1 && !record2)
            {
                var table = _schema.GetMtmTable(relation.MtmTable);
                var rcdMtm = new Record(table);
                rcdMtm.SetRelation(relation.Name, targetRecord.GetField());
                rcdMtm.SetRelation(relation.InverseRelationName, sourceRecord.GetField());
                _data.Add(new BulkSaveQuery(null, BulkSaveType.DeleteRecord, rcdMtm, rcdMtm, relation));
            }
            // otherwise nothing to do
        }
        private void UnRelateMtoRecord(Record sourceRecord, Relation relation)
        {
            // (TESTED)
            if (sourceRecord == null || relation == null) return;
            var query = FindQueryByRecord(sourceRecord); // source
            if (query != null)
                query.CurrentRecord.SetRelation(relation.Name, 0L);
            else
            {
                if (!sourceRecord.IsNew)
                {
                    sourceRecord.SetRelation(relation.Name, 0L);
                    _data.Add(new BulkSaveQuery(null, BulkSaveType.UpdateRecord, sourceRecord,
                        sourceRecord.Copy(), null));
                }
            }

        }

        #endregion

    }
}
