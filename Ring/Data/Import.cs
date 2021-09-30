using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util;
using Ring.Util.Builders;
using Ring.Util.Models;
using System;
using System.Collections.Generic;
using Ring.Schema.Builders;
using Database = Ring.Schema.Models.Schema;
using RecordList = Ring.Data.List;

namespace Ring.Data
{
    /// <summary>
    /// Used to import data 
    /// </summary>
    public sealed class Import
    {

        private readonly Logger _log = new Logger(typeof(Import));
        private readonly RecordBuilder _recordBuilder = new RecordBuilder();
	    private readonly IndexBuilder _indexBuilder = new IndexBuilder();
        private int _insertCount;
        private int _updateCount;
	    private Database _currentSchema;

		/// <summary>
		/// Parse Excel file
		/// </summary>
		public long ParseFile(string file, string schemaName) =>
            ParseFile(new ImportParameters { File = file, TrimValue = true, SchemaName = schemaName });
        public long ParseFile(IImportParameters parameters)
        {
            var doc = new ExcelWorkbook { DuplicateMergedCell = true };
            var jobId = Global.SequenceJobId.NextValue();
            var file = parameters.File;
	        _currentSchema = Global.Databases.GetSchema(parameters.SchemaName);
            // int schemaId, long jobId, short id,
            _log.Info(_currentSchema?.Id ?? 0, jobId, 1, Constants.InfoStartImport, string.Format(Constants.InfoStartImportDesc, file));
            if (_currentSchema == null)
            {
                _log.Warn(jobId, 1, Constants.WarnSchemaNotExistsDesc, string.Format(Constants.WarnSchemaNotExistsDesc, parameters.SchemaName));
                //TODO add log 
                return jobId;
            }
            var connection = _currentSchema.Connections.Get();

            _insertCount = 0;
            _updateCount = 0;

            try
            {
                // log starting import
                doc.LoadDocument(file);
                var sheetList = GetSheetList(_currentSchema, doc);

                for (var i = 0; i < sheetList.Length; ++i)
                {
                    var sheet = sheetList[i];
                    var table = _currentSchema.GetTable(sheet.Name, StringComparison.OrdinalIgnoreCase);
                    if (table != null) ProcessEntity(jobId, connection, parameters, sheet, table);
                    else if (Global.Databases.LexiconTable.Name.Equals(sheet.Name, StringComparison.OrdinalIgnoreCase))
                        ProcessLexicon(_currentSchema.Id, sheet);
                }

                _log.Info(_currentSchema.Id, jobId, 2, Constants.InfoEndImport, string.Format(Constants.InfoEndImportDesc,
                    _insertCount.ToString(), _updateCount.ToString()));
            }
            catch (Exception ex)
            {
                _log.Error(_currentSchema.Id, jobId, ex);
            }
            finally
            {
	            _currentSchema.Connections.Put(connection);
            }
            return jobId;
        }

        #region private methods 

        /// <summary>
        /// Process @lexicon & @lexicon_itm entities
        /// </summary>
        private void ProcessLexicon(int shemaId, ExcelSheet sheet)
        {
            var lexiconList = _recordBuilder.GetInstances(shemaId, sheet);
            var lexiconIdList = GetLexiconId(lexiconList);

            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            br.SimpleQuery(0, Global.Databases.LexiconTable.Name);
            br.AppendFilter(0, Constants.LexiconItemLexiId, OperationType.In, lexiconIdList);
            br.RetrieveRecords();

            #region delete existing lexicon_item connected 
            var lst = br.GetRecordList(0);
            if (lst.Count > 0)
            {
                var bs = new BulkSave { Schema = Global.Databases.MetaSchema };
                for (var i = 0; i < lexiconIdList.Count; ++i)
                {
                    var itemId = lexiconIdList[i];
                    bs.DeleteRecord(RecordBuilder.GetInstance(null, int.Parse(itemId.GetField()), null, null));
                    // commit each 100 records 
                    if (i % 100 == 0 && i != 0) bs.Save();
                }
                bs.Save();
            }
            #endregion
            #region insert new lexicon
            if (lexiconList != null)
            {
                var bs = new BulkSave();
                for (var i = 0; i < lexiconList.Length; ++i)
                {
                    bs.InsertRecord(lexiconList[i]);
                    // commit each 100 records 
                    if (i % 100 == 0 && i != 0) bs.Save();
                }
                bs.Save();
            }
            #endregion 

            lexiconIdList.Dispose();
        }

        /// <summary>
        /// Process any business tables
        /// </summary>
        private void ProcessEntity(long jobId, IDbConnection connection, IImportParameters parameters, ExcelSheet sheet, Table table)
        {
            var key = GetKey(table, sheet); // TODO add excel sheet key
	        if (key == null) return; // no key skip it 
			Record rcd;
            var excelFieldList = ExcelFieldBuilder.GetInstances(table, sheet);
            //TODO detect duplicate into workbook !!
            var maxId = 0L;
            var updateCount = 0; // count of modificatio
            var hasMtm = Array.FindLastIndex(excelFieldList, p => p?.Relation?.Type == RelationType.Mtm) >= 0;

	        try
	        {
		        // update sheet by sheet
		        for (var j = sheet.FirstRowNum + 1; j <= sheet.LastRowNum; ++j)
		        {
					// check before to insert in db
					//TODO manage MTM relationship !!
					rcd = FindRecord(connection, j, table, excelFieldList);
					var newRecord = rcd == null;
					if (newRecord) rcd = new Record(table);
					// set value
					var modified = SetRecord(connection, parameters, j, rcd, excelFieldList);

					// detect an empty row we stopping import 
					if (!modified) break;
					var saveResult = SaveRecord(connection, jobId, rcd, newRecord);
					if (saveResult) ++updateCount;
					var currentId = long.Parse(rcd.GetField());
					if (hasMtm) SaveMtm(connection, j, rcd, excelFieldList);
					if (currentId > maxId) maxId = currentId;
				}
				FinalizeTableImport(table, updateCount, maxId);
	        }
	        catch (Exception ex)
	        {
				_log.Error(table.SchemaId, jobId, ex);
			}
		}

        /// <summary>
        /// Define order to proceed sheets 
        /// </summary>
        private bool SaveRecord(IDbConnection connection, long jobId, Record record, bool newRecord)
        {
            var result = false;

			// missing values warning ??
	        var missingValueDesc = IsMissingValue(record);
	        if (missingValueDesc != null)
	        {
				_log.Warn(jobId, -3, Constants.WarnMsgMissingValue, missingValueDesc);
	        }
	        try
			{
                BulkSaveQuery bs = null;
                //using BulkSaveQuery instead of bulkSave to manage readonly tables
                if (newRecord)
                {
                    // id must be generated ? force to generate for non lookup tables
                    if (record[record.Table.PrimaryKeyIdIndex] == null || !record.Table.Readonly) record.SetField(record.Table.GetId(connection, 1));
                    bs = new BulkSaveQuery(null, BulkSaveType.InsertRecord, record, record.Copy(), null);
                    ++_insertCount;
                }
                else if (record.IsDirty)
                {
                    bs = new BulkSaveQuery(null, BulkSaveType.UpdateRecord, record, record.Copy(), null);
                    ++_updateCount;
                }
                if (bs != null)
                {
                    var sql = bs.GetQuery(connection);
                    sql.Connection = connection;
                    sql.CommandTimeout = Constants.CommandLineTimeOut;
                    sql.ExecuteNonQuery();
                    sql.Connection = null;
                    sql.Dispose();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(record.Table.SchemaId, jobId, ex);
            }
            return result;
        }

        /// <summary>
        /// Define order to proceed sheets & get sheet list
        /// </summary>
        private static ExcelSheet[] GetSheetList(Database schema, ExcelWorkbook excelDocument)
        {
            if (excelDocument == null) return new ExcelSheet[0]; // avoid null value - workbook without sheet cannot exists
            var sheetList = new Tuple<int, ExcelSheet>[excelDocument.SheetCount];
            var result = new ExcelSheet[excelDocument.SheetCount];

            // generate temporary object - List<Tuple<priority , ExcelSheet>>
            for (var i = 0; i < excelDocument.SheetCount; ++i)
                sheetList[i] = new Tuple<int, ExcelSheet>(0, excelDocument.GetSheet(i));

            // generate priority
            for (var i = 0; i < sheetList.Length; ++i)
            {
                var sheet = sheetList[i];
                var table = schema.GetTable(sheet.Item2.Name, StringComparison.OrdinalIgnoreCase);
                if (table != null)
                {
                    var excelFieldList = ExcelFieldBuilder.GetInstances(table, sheet.Item2);
                    for (var j = 0; j < excelFieldList.Length; ++j)
                        if (excelFieldList[j].Relation != null &&
                            (excelFieldList[j].Relation.Type == RelationType.Mto || excelFieldList[j].Relation.Type == RelationType.Otop))
                            sheetList[i] = new Tuple<int, ExcelSheet>(sheetList[i].Item1 + 1, sheet.Item2);
                }
            }

            // descending sort on priority
            Array.Sort(sheetList, (x, y) => x.Item1.CompareTo(y.Item1));

            // generate result -  result <-- sheetList 
            for (var i = 0; i < sheetList.Length; ++i) result[i] = sheetList[i].Item2;

            // sort by priority
            return result;
        }

        /// <summary>
        /// Is key value from Excel sheet exist in the current schema ? 
        /// </summary>
        private Record FindRecord(IDbConnection connection, int rowId, Table table, ExcelField[] excelFieldList)
        {
            var brq = new BulkRetrieveQuery(BulkQueryType.SimpleQuery, table);
            for (var i = 0; i < excelFieldList.Length; ++i)
            {
                var excelField = excelFieldList[i];
                // if id present use it as default key
                if (string.Equals(excelField.Field.Name, table.PrimaryKey.Name, StringComparison.OrdinalIgnoreCase) && excelField.Column.Cells.ContainsKey(rowId))
                {
                    var filter = new BulkRetrieveFilter(excelField.Field, OperationType.Equal, excelField.Column.Cells[rowId], true);
                    brq.Filters.Add(filter);
                    // add all relationships !!!
                    break;
                }
                if (excelField.Key)
                {
                    //TODO manage multiple relation id and field id 
                    var keyValue = excelField.Column.Cells.ContainsKey(rowId) ? excelField.Column.Cells[rowId] : null;
                    if (keyValue == null) return null; // no need query 
                                                       // just manage the fields 
                    if (excelField.Relation == null)
                    {
                        var filter = new BulkRetrieveFilter(excelField.Field, OperationType.Equal, keyValue, true);
                        brq.Filters.Add(filter);
                    }
                }
            }
            // manage foreign keys 
            brq.LoadResult(connection);
            var lst = brq.Result;
            return lst.Count == 1 ? lst[0] : null;
        }

        /// <summary>
        /// connect current record to mtm table 
        /// </summary>
        private void SaveMtm(IDbConnection connection, int rowId, Record record, ExcelField[] excelFieldList)
        {
            var mtmToSave = new List<Tuple<string, long>>();
	        var hashValue = new HashSet<string>();
			// find target id 
            for (var i = 0; i < excelFieldList.Length; ++i)
            {
                var excelField = excelFieldList[i];
                if (excelField?.Relation?.Type == RelationType.Mtm)
                {
                    var value = excelField.Column.Cells.ContainsKey(rowId) ? excelField.Column.Cells[rowId] : null;
					if (value == null || hashValue.Contains(value)) continue;
	                hashValue.Add(value);
					var br =  new BulkRetrieve { Schema = _currentSchema };
		            br.SimpleQuery(0, excelField.Relation.To.Name);
		            br.AppendFilter(0, excelField.Field.Name, OperationType.Equal, value);
					br.RetrieveRecords();
		            var lst = br.GetRecordList(0);
		            if (lst.Count != 1) continue;
	                mtmToSave.Add(new Tuple<string, long>(excelField.Relation.Name, long.Parse(lst[0].GetField())));
                }
            }
	        SaveMtm(connection, mtmToSave, record);
        }

		/// <summary>
		/// Tuple of mtm record to save 
		/// </summary>
	    private void SaveMtm(IDbConnection connection, List<Tuple<string, long>> mtmToSave, Record record)
	    {
		    if (mtmToSave.Count == 0) return;

		    var bs = new BulkSave { Schema = _currentSchema };
		    for (var i = 0; i < mtmToSave.Count; ++i)
		    {
			    var relationName = mtmToSave[i].Item1;
			    bs.RelateRecordsToId(record, record.Table.GetRelation(relationName).To.Name, mtmToSave[i].Item2, relationName);
		    }
		    bs.Save(connection);
	    }


		/// <summary>
		/// Set excel values to Record 
		/// </summary>
		/// <returns>Is record modified</returns>
		private bool SetRecord(IDbConnection connection, IImportParameters parameters, int rowId, Record record, ExcelField[] excelFieldList)
        {
            if (record == null || excelFieldList == null) return false;
            var emptyCount = 0;
            var trimValue = parameters.TrimValue;
		    // read by column 
		    for (var i = 0; i < excelFieldList.Length; ++i)
		    {
			    var excelField = excelFieldList[i];

			    if (!excelField.Column.Cells.ContainsKey(rowId))
			    {
				    ++emptyCount;
				    continue;
			    }
			    // manage field
			    if (excelField.Relation == null && excelField.Field != null)
			    {
				    switch (excelField.Field.Type)
				    {
					    case FieldType.LongDateTime:
					    case FieldType.DateTime:
					    case FieldType.ShortDateTime:
						    SetDate(record, excelField.Field.Name, excelField.Column?.Cells[rowId]?.Trim());
						    break;
					    default:
						    record.SetField(excelField.Field.Name,
							    trimValue ? excelField.Column?.Cells[rowId]?.Trim() : excelField.Column?.Cells[rowId]);
						    break;
				    }
			    }
		    }
		    SetRelation(connection, rowId, record, excelFieldList);
			return excelFieldList.Length > emptyCount;
        }

		/// <summary>
		/// Set relation values to Record 
		/// </summary>
		private void SetRelation(IDbConnection connection, int rowId, Record record, ExcelField[] excelFieldList)
	    {
		    // read by column 
		    for (var i = 0; i < excelFieldList.Length; ++i)
		    {
			    var excelField = excelFieldList[i];
			    if (!excelField.Column.Cells.ContainsKey(rowId)) continue;

					// manage MTO relationships 
				if (excelField.Relation != null && excelField.Field != null &&
			        (excelField.Relation.Type == RelationType.Mto || excelField.Relation.Type == RelationType.Otop))
			    {
				    var value = excelField.Column.Cells[rowId];
				    if (string.IsNullOrWhiteSpace(value)) continue;

				    // TODO improve performance: cache transition queries + detect relation differences 
				    var br = new BulkRetrieve { Schema = Global.Databases.GetSchema(record.Table.SchemaId) };

				    br.SimpleQuery(0, excelField.Relation.To.Name);
				    br.AppendFilter(0, excelField.Field.Name, OperationType.Equal, value);
				    br.RetrieveRecords(connection);

				    var lst = br.GetRecordList(0);
				    if (lst.Count > 0)
				    {
					    // check with previous relation
					    var preValue = lst.ItemByIndex(0).GetField();
					    record.SetRelation(excelField.Relation.Name, preValue);
				    }
				    else
				    {
					    // insert if target doesn't exist 
					    var rcd = new Record(excelField.Relation.To);
						rcd.SetField(excelField.Relation.To.GetId(connection, 1L));
						rcd.SetField(excelField.Field.Name, value);
					    var bs = new BulkSaveQuery(null, BulkSaveType.InsertRecord, rcd, rcd.Copy(), null);
					    var sql = bs.GetQuery(connection);
					    sql.Connection = connection;
					    sql.CommandTimeout = Constants.CommandLineTimeOut;
					    sql.ExecuteNonQuery();
					    sql.Connection = null;
					    sql.Dispose();
					    record.SetRelation(excelField.Relation.Name, rcd.GetField());
				    }
			    }
		    }
	    }

		/// <summary>
		/// Get distinct list of LexiconId
		/// </summary>
		private RecordList GetLexiconId(Record[] lexiconItems)
        {
            var result = new RecordList();
            if (lexiconItems == null || lexiconItems.Length == 0) return result;
            result.ItemType = ItemType.Integer.ToString();
            var dico = new HashSet<int>();
            for (var i = 0; i < lexiconItems.Length; ++i)
            {
                var record = lexiconItems[i];
                int tempId;
                if (record.RecordType != Global.Databases.LexiconTable.Name) continue;
                record.GetField(Constants.LexiconItemLexiId, out tempId);
                if (tempId > 0 && !dico.Contains(tempId))
                {
                    dico.Add(tempId);
                    result.AppendItem(tempId);
                }
            }
            return result;
        }

        /// <summary>
        /// Calculate date from import xlsx file 
        /// </summary>
        private static void SetDate(Record record, string fieldName, string value)
        {
            if (value == null) return;
            //1 - try to cast from a number
            double dbl;
            if (double.TryParse(value, out dbl))
            {
                record.SetField(fieldName, GetDate(dbl));
                return;
            }
            //2 -try to cast from string 
            DateTime dt;
            if (DateTime.TryParse(value, out dt)) record.SetField(fieldName, dt);
        }

        /// <summary>
        /// Get date from number
        /// </summary>
        private static string GetDate(double dataId)
        {
            var date = Constants.BaseDate.AddDays(dataId);
            return string.Format(Constants.DateFormat, date.Year.ToString(Constants.Digit4Format), date.Month.ToString(Constants.Digit2Format),
                date.Day.ToString(Constants.Digit2Format), date.Hour.ToString(Constants.Digit2Format), date.Minute.ToString(Constants.Digit2Format),
                date.Second.ToString(Constants.Digit2Format));
        }

		/// <summary>
		/// Find a key into table or excelsheet
		/// </summary>
	    private Index GetKey(Table table, ExcelSheet sheet)
	    {
			var result = table.GetFirstKey();
			if (result==null && table.PrimaryKey!= null)
			{
				for (var i= 0;i<sheet.Columns.Length; ++i)
				{
					var header = sheet.Columns[i].Cells.ContainsKey(sheet.FirstRowNum)
						? sheet.Columns[i].Cells[sheet.FirstRowNum]
						: null;
					if (string.Equals(table.PrimaryKey.Name, header))
						return _indexBuilder.GetPkInstance();
				}
			}
			return result;
	    }

	    private static string IsMissingValue(Record record)
	    {
		    if (record.Table == null) return null;
		    for (var i = 0; i < record.Table.Fields.Length; ++i)
			    if (record.Table.Fields[i].NotNull && record[i] == null && string.IsNullOrEmpty(record.Table.Fields[i].DefaultValue) && i!=record.Table.PrimaryKeyIdIndex)
				    return string.Format(Constants.WarnMsgMissingFieldDesc, record.Table.Name, record.Table.Fields[i].Name);
		    for (var i = 0; i < record.Table.Relations.Length; ++i)
				if ((record.Table.Relations[i].Type == RelationType.Mto || record.Table.Relations[i].Type == RelationType.Otop) &&  
					record.Table.Relations[i].NotNull && record[i] == null && record.GetRelation(record.Table.Relations[i].Name)==0)
				    return string.Format(Constants.WarnMsgMissingReldDesc, record.Table.Name, record.Table.Relations[i].Name);
			return null;
	    }

	    private static void FinalizeTableImport(Table table, int updateCount, long maxId)
	    {
		    if (table.Readonly && updateCount > 0)
		    {
			    System.Threading.Tasks.Task.Factory.StartNew(() =>
			    {
				    table.SetId(maxId); // reset Id (only for readonly tables) 
				    table.Vacuum();
				    table.Analyze();
			    }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
		    }
		}

	    #endregion

	}
}
