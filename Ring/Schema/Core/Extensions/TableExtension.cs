using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Helpers;
using Ring.Data.Mappers;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Schema.Core.Extensions
{
    internal static class TableExtension
    {
	    public static Database GetCurrentSchema(this Table table) => Global.Databases.GetSchema(table.SchemaId) ?? Global.Databases.PendingSchema;


	    /// <summary>
		/// Get field by name, case sensitive search ==> O(log n) complexity
		/// </summary>
		/// <param name="table"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Field GetField(this Table table, string name)
        {
            int indexerLeft = 0, indexerRigth = table.Fields.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle].Name);
                if (indexerCompare == 0) return table.Fields[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Get field by name ==> O(n) complexity
        /// </summary>
        public static Field GetField(this Table table, string name, StringComparison comparisonType)
        {
            if (comparisonType == StringComparison.Ordinal) return GetField(table, name);
            /* fieldById can contains null Field objects */
            for (var i = table.FieldsById.Length - 1; i >= 0; --i)
                if (table.FieldsById[i] != null &&
                    string.Equals(name, table.FieldsById[i].Name, comparisonType))
                    return table.FieldsById[i];
            return null;
        }

        /// <summary>
        /// Get Fields by id ==> O(log n) complexity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Field GetField(this Table table, int id)
        {
            int indexerLeft = 0, indexerRigth = table.FieldsById.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = id - table.FieldsById[indexerMiddle].Id;
                if (indexerCompare == 0) return table.FieldsById[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Get position of field in the table FieldExtension array (Compare StringComparison.OrdinalIgnoreCase)
        /// </summary>
        /// <param name="table">Current table</param>
        /// <param name="name">Field name</param>
        /// <returns>Return index position otherwise int.MinValue (Constants.FieldNameNotFound)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFieldIndex(this Table table, string name)
        {
            int indexerLeft = 0, indexerRigth = table.Fields.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle].Name);
                if (indexerCompare == 0) return indexerMiddle;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return Constants.FieldNameNotFound;
        }

        /// <summary>
        /// Get position of default lexicon into Schema.Lexicons array (from first positio)
        /// </summary>
        /// <param name="table">Current table</param>
        /// <returns>Return index position otherwise int.MinValue (Constants.FieldNameNotFound)</returns>
        public static int GetLexiconIndex(this Table table) =>
            table.LexiconIndexes != null && table.LexiconIndexes.Length > 0 ? table.LexiconIndexes[0].Index : Constants.LexiconNotFound;

        /// <summary>
        /// Get position of default lexicon into Schema.Lexicons array (from fieldId)
        /// </summary>
        /// <param name="table">Current table</param>
        /// <param name="fieldId">Current Field.Id</param>
        /// <returns>Return index position otherwise int.MinValue (Constants.FieldNameNotFound)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLexiconIndex(this Table table, int fieldId)
        {
            if (table.LexiconIndexes != null && table.LexiconIndexes.Length > 0)
            {
                int indexerLeft = 1, indexerRigth = table.LexiconIndexes.Length - 1;
                while (indexerLeft <= indexerRigth)
                {
                    var indexerMiddle = indexerLeft + indexerRigth;
                    indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                    var indexerCompare = fieldId - table.LexiconIndexes[indexerMiddle].FieldId;
                    if (indexerCompare == 0) return table.LexiconIndexes[indexerMiddle].Index;
                    if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                    else indexerRigth = indexerMiddle - 1;
                }
            }
            return Constants.LexiconNotFound;
        }

        /// <summary>
        /// Get relation object by name ==> O(log n) complexity
        /// </summary>
        /// <param name="table">Table object</param>
        /// <param name="name">Relation name</param>
        /// <returns>Relation object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Relation GetRelation(this Table table, string name)
        {
            int indexerLeft = 0, indexerRigth = table.Relations.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, table.Relations[indexerMiddle].Name);
                if (indexerCompare == 0) return table.Relations[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Get relation object by name ==> O(n) complexity
        /// </summary>
        /// <param name="table">Table object</param>
        /// <param name="name">Relation name</param>
        /// <param name="comparisonType">Comparison Type</param>
        /// <returns>Relation object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Relation GetRelation(this Table table, string name, StringComparison comparisonType)
        {
            if (comparisonType == StringComparison.Ordinal) return GetRelation(table, name);
            for (var i = table.Relations.Length - 1; i >= 0; --i)
                if (string.Equals(name, table.Relations[i].Name, comparisonType)) return table.Relations[i];
            return null;
        }

        /// <summary>
        /// Get relation object by id ==> O(n) complexity
        /// </summary>
        /// <returns>Relation object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Relation GetRelation(this Table table, int id)
        {
            for (var i = table.Relations.Length - 1; i >= 0; --i)
                if (id == table.Relations[i].Id) return table.Relations[i];
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Return index position otherwise int.MinValue (Constants.RelationNameNotFound)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRelationIndex(this Table table, string name)
        {
            int indexerLeft = 0, indexerRigth = table.Relations.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, table.Relations[indexerMiddle].Name);
                if (indexerCompare == 0) return indexerMiddle;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return Constants.RelationNameNotFound;
        }

        /// <summary>
        /// Generate id with cache management for BulKsave (Hi/Lo algorithm) 
        /// </summary>
        public static long GetId(this Table table, IDbConnection connection, long range)
        {
            long result;
            lock (table.CacheId.Sync)
            {
                // increase and assign
                result = table.CacheId.CurrentId + range;

                // reserve range of id into @meta_id id 
                if (result < table.CacheId.MaxId)
                {
                    result = table.CacheId.CurrentId + 1;
                    table.CacheId.CurrentId += range;
                    return result;
                }

                var newRange = range;

                // re-calculate  .ReservedRange
                if (table.CacheId.ReservedRange > 0)
                {
                    while (range > table.CacheId.ReservedRange) table.CacheId.ReservedRange <<= 1;
                    newRange = table.CacheId.ReservedRange;
                }

                // reserve range of value into @meta_id id 
                var sql = SqlHelper.GetUpdateMetaIdQuery(connection, table.Id, table.SchemaId, Constants.TableId, newRange);

                sql.Connection = connection;
                result = long.Parse(sql.ExecuteScalar().ToString());
                sql.Connection = null; // don't keep a reference to the Db connection

                // ==== end update @meta_id
                table.CacheId.MaxId = result;
                result -= newRange - 1;
                table.CacheId.CurrentId = result + range - 1;  // only at the end !!!!!
                table.CacheId.ReservedRange <<= 1; // double range !! 
            }
            return result;
        }

        /// <summary>
        /// Reset ID 
        /// </summary>
        public static void SetId(this Table table, long newId)
        {
            if (table.CacheId.CurrentId < newId)
            {
                var schema = Global.Databases.MetaSchema;
                var connection = schema.Connections.Get(); // not from new schema !!!
                var log = new Logger(typeof(Table));

                try
                {
                    GetId(table, connection, newId - table.CacheId.CurrentId + 1L);
                }
                catch (Exception ex)
                {
                    log.Error(table.SchemaId, ex);
                }
                finally
                {
                    schema.Connections.Put(connection);
                }
            }
        }

        /// <summary>
        /// Build Insert clause string 
        /// </summary>
        public static string GetInsertClause(this Table table)
        {
            //TODO manage a cache 
            if (table.FieldsById == null || table.FieldsById.Length == 0) return null;
            var sb = new StringBuilder();
            var arrCount = table.FieldsById.Length;
            for (var i = 0; i < arrCount; ++i)
            {
                var field = table.FieldsById[i];
                sb.Append(field.Name);
                sb.Append(Constants.SelectSeparator);
                // manage searchable field
                if (field.Type != FieldType.String || field.CaseSensitive) continue;
                sb.Append(Constants.SearchablePrefixClfy);
                sb.Append(field.Name);
                sb.Append(Constants.SelectSeparator);
            }
            sb.Length = sb.Length - 1;
            return sb.ToString();
        }

        /// <summary>
        /// Build Select clause string (concat all fields from table.FieldById) 
        /// </summary>
        public static void AppendSelectClause(this Table table, StringBuilder query)
        {
            //TODO manage a cache 
            var arrCount = table.FieldsById.Length;
            for (var i = 0; i < arrCount; ++i)
            {
                var field = table.FieldsById[i];
                query.Append(field.Name);
                query.Append(Constants.SelectSeparator);
            }
            query.Length = query.Length - 1;
        }

        /// <summary>
        /// Build Select clause string (concat all fields from fieldList) 
        /// </summary>
        public static void AppendSelectClause(this Table table, StringBuilder query, List<Field> fieldList, List<Relation> relationList)
        {
            var arrCount = fieldList.Count;
            for (var i = 0; i < arrCount; ++i)
            {
                var field = fieldList[i];
                query.Append(field.Name);
                query.Append(Constants.SelectSeparator);
            }
            arrCount = relationList.Count;
            for (var i = 0; i < arrCount; ++i)
            {
                var field = relationList[i];
                query.Append(field.Name);
                query.Append(Constants.SelectSeparator);
            }
            query.Length = query.Length - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string GetConflictClause(this Table table, DatabaseProvider provider, string inputFields)
        {
            if (table == null) return null;
            switch (provider)
            {
                case DatabaseProvider.PostgreSql: return string.Format(Constants.PostgreConflictClause, inputFields);
            }
            return null;
        }

        /// <summary>
        /// Compare to table to dectect changes 
        /// </summary>
        public static bool Equals(this Table table1, Table table2)
        {
            if (table1.Fields.Length != table2.Fields.Length) return false;
            if (table1.Relations.Length != table2.Relations.Length) return false;
            if (table1.Indexes.Length != table2.Indexes.Length) return false;
            if (!table1.Description.Equals(table2.Description)) return false;
            if (table1.Readonly != table2.Readonly) return false;
            if (table1.Baseline != table2.Baseline) return false;
            // check field content && relations content
            if (!string.Equals(GetFielsdKey(table1), GetFielsdKey(table2))) return false;
            if (!string.Equals(GetIndexesdKey(table1), GetIndexesdKey(table2))) return false;
            // don't compare indexes here 
            return true;
        }

        /// <summary>
        /// Alter table from previous version 
        /// </summary>
        public static void Alter(this Table newTable)
        {
            var prevSchema = Global.Databases.GetSchema(newTable.SchemaId);
            var prevTable = prevSchema.GetTable(newTable.Name) ?? prevSchema.GetTable(newTable.Name, StringComparison.OrdinalIgnoreCase);

            #region detect flags or description or  subject changes
            if (newTable.Readonly != prevTable.Readonly ||
                newTable.Baseline != prevTable.Baseline ||
                !string.Equals(newTable.Description, prevTable.Description) ||
                !string.Equals(newTable.Subject, prevTable.Subject))
                UpdateMetadata(prevTable, newTable);
            #endregion
            AlterFields(prevTable, newTable);
            AlterRelations(prevTable, newTable);
            AlterIndexes(prevTable, newTable);
        }

        /// <summary>
        /// Get first key
        /// </summary>
        public static Index GetFirstKey(this Table table)
        {
            if (table.Indexes != null && table.Indexes.Length > 0)
                for (var i = 0; i < table.Indexes.Length; ++i)
                    if (table.Indexes[i].Unique) return table.Indexes[i];
            return null;
        }

        /// <summary>
        /// Create table from scratch & update [@meta] & [@meta_id] tables
        /// </summary>
        public static void Create(this Table table)
        {
            if (table == null) return;

            var databaseCollection = Global.Databases;
            var schema = databaseCollection.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            var log = new Logger(typeof(Table));

            try
            {
                //1> insert into @meta 
                if (table.Type == TableType.Business)
                {
                    var bs = new BulkSave { Schema = Global.Databases.MetaSchema }; // !! not performant !! commit each table !!
                    var metaDataBuilder = new MetaDataBuilder();
                    var metaDataArr = metaDataBuilder.GetInstances(table);

                    // delete previous metaData 
                    DeleteMetadata(connection, table);

                    // int schemaId, int entityId, sbyte entityType, long initialObjid
                    // insert @meta_id record TODO --> only if not exists already
                    bs.InsertRecord(RecordMapper.Map(table.Id, table.SchemaId, (sbyte)EntityType.Table, 0L));

                    // insert fields into @meta
                    for (var i = 0; i < metaDataArr.Length; ++i)
                        if (metaDataArr[i].IsField() || metaDataArr[i].IsTable() || metaDataArr[i].IsRelation())
                            bs.InsertRecord(RecordMapper.Map(table.SchemaId, metaDataArr[i], true));
                    bs.Save(connection);
                }

                //2> TODO is table exist rename it ??

                //3> compare is u need to use another DB connection - here take new schema 
                Helpers.SqlHelper.CreateTable(connection, table, GetTableSpace(table, EntityType.Table));
				if (table.PrimaryKey!=null) Helpers.SqlHelper.CreatePrimaryKey(connection, table, GetTableSpace(table, EntityType.Index));

                //4> create indexes 
                if (table.Indexes != null) for (var i = 0; i < table.Indexes.Length; ++i) table.Indexes[i].Create();
			}
			catch (Exception ex)
            {
                log.Error(table.SchemaId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);

                //3> assign cache id to avoid id generation to 0
                table.CacheId.CurrentId = 1;
            }
        }

        /// <summary>
        /// Non case sensitif method to detect if a table exists physically in the database (very slow) (TESTED)
        /// </summary>
        public static bool Exists(this Table table)
        { 
            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            var schemaName = TableBuilder.GetSchemaName(table.PhysicalName);
            var schema = Global.Databases.GetSchema(table.SchemaId) ?? Global.Databases.PendingSchema;
            var tableName = TableBuilder.GetTableName(table.PhysicalName);
            var fieldBuilder = new FieldBuilder();
            var schemaFieldName = fieldBuilder.GetFieldName(schema.Driver, TableType.TableDictionary, FieldKey.SchemaName);
            br.SimpleQuery(0, Global.Databases.MetaSchema.GetTable(TableType.TableDictionary).Name);
            br.AppendFilter(0, fieldBuilder.GetFieldName(schema.Driver, TableType.TableDictionary, FieldKey.Name), OperationType.Equal, tableName, false);
            if (!string.IsNullOrEmpty(schemaFieldName))
                br.AppendFilter(0, fieldBuilder.GetFieldName(schema.Driver, TableType.TableDictionary, FieldKey.SchemaName), OperationType.Equal, schemaName, false);
            br.RetrieveRecords();
            var lst = br.GetRecordList(0);
            return lst.Count > 0;
        }

        /// <summary>
		/// Drop table + remove @meta data entries
		/// </summary>
		public static void Drop(this Table table)
        {
            if (table.Type == TableType.Meta || table.Type == TableType.MetaId) return;
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get();
            var log = new Logger(typeof(TableExtension));

            //TODO: add to check on exist table
            //TODO: use another connection to operational db 
            try
            {
                // delete @meta & @meta_id
                if (table.Type == TableType.Business) DeleteMetadata(connection, table);
                if ((table.Type == TableType.Business || table.Type == TableType.Mtm) && table.Exists())
                    Helpers.SqlHelper.DropTable(connection, table);
            }
            catch (Exception ex)
            {
                log.Error(table.SchemaId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        /// <summary>
        /// Truncate table 
        /// </summary>
        public static void Truncate(this Table table)
        {
            var schema = Global.Databases.GetSchema(table.SchemaId) ?? Global.Databases.PendingSchema;
            var connection = schema.Connections.Get();
            var log = new Logger(typeof(TableExtension));

            try
            {
                if (table.Type == TableType.Meta || table.Type == TableType.MetaId || table.Type == TableType.User) return;
                Helpers.SqlHelper.TruncateTable(connection, table);
            }
            catch (Exception ex)
            {
                log.Error(table.SchemaId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        /// <summary>
        /// Vaccum table FULL on postgreSql 
        /// </summary>
        public static void Vacuum(this Table table)
        {
            var schema = Global.Databases.GetSchema(table.SchemaId) ?? Global.Databases.PendingSchema;
            var connection = schema.Connections.Get();
            var log = new Logger(typeof(TableExtension));
            var jobId = Global.Databases.UpgradeJobId;

            try
            {
                Helpers.SqlHelper.Vacuum(connection, table);
            }
            catch (Exception ex)
            {
                log.Error(table.SchemaId, jobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        /// <summary>
        /// Analyze table 
        /// </summary>
        public static void Analyze(this Table table)
        {
            var schema = Global.Databases.GetSchema(table.SchemaId) ?? Global.Databases.PendingSchema;
            var connection = schema.Connections.Get();
            var log = new Logger(typeof(TableExtension));
            var jobId = Global.Databases.UpgradeJobId;

            try
            {
                Helpers.SqlHelper.Analyze(connection, table);
            }
            catch (Exception ex)
            {
                log.Error(table.SchemaId, jobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        #region private methods
        private static TableSpace GetTableSpace(Table table, EntityType entityType)
        {
            var schema = Global.Databases.PendingSchema ?? Global.Databases.MetaSchema;
            return schema.GetTableSpace(table, entityType);
        }
        private static void AlterRelations(Table prevTable, Table newTable)
        {
            var dicoNewEntities = new HashSet<string>();
            var dicoPrevEntities = new HashSet<string>();
            #region generate dico of relations
            for (var i = 0; i < prevTable.Relations.Length; ++i)
                if (!dicoPrevEntities.Contains(prevTable.Relations[i].Name.ToUpper()))
                    dicoPrevEntities.Add(prevTable.Relations[i].Name.ToUpper());
            for (var i = 0; i < newTable.Relations.Length; ++i)
                if (!dicoNewEntities.Contains(newTable.Relations[i].Name.ToUpper()))
                    dicoNewEntities.Add(newTable.Relations[i].Name.ToUpper());
            #endregion
            #region add new relations on table
            for (var i = 0; i < newTable.Relations.Length; ++i)
                if (!dicoPrevEntities.Contains(newTable.Relations[i].Name.ToUpper()))
                    newTable.Relations[i].Add();
            #endregion
            #region remove relations on table
            for (var i = 0; i < prevTable.Relations.Length; ++i)
                if (!dicoNewEntities.Contains(prevTable.Relations[i].Name.ToUpper()))
                    prevTable.Relations[i].Remove();
            #endregion
        }
        private static void AlterIndexes(Table prevTable, Table newTable)
        {
            var dicoNewEntities = new HashSet<string>();
            var dicoPrevEntities = new HashSet<string>();
            #region generate dico of indexes
            for (var i = 0; i < prevTable.Indexes.Length; ++i)
            {
                var key = prevTable.Indexes[i].GetKey();
                if (!dicoPrevEntities.Contains(key)) dicoPrevEntities.Add(key);
            }
            for (var i = 0; i < newTable.Indexes.Length; ++i)
            {
                var key = newTable.Indexes[i].GetKey();
                if (!dicoNewEntities.Contains(key)) dicoNewEntities.Add(key);
            }
            #endregion
            #region remove indexes on table (first drop indexes)
            for (var i = 0; i < prevTable.Indexes.Length; ++i)
            {
                var key = prevTable.Indexes[i].GetKey();
                if (!dicoNewEntities.Contains(key)) prevTable.Indexes[i].Remove();
            }
            #endregion
            #region add new indexes on table
            for (var i = 0; i < newTable.Indexes.Length; ++i)
            {
                var key = newTable.Indexes[i].GetKey();
                if (!dicoPrevEntities.Contains(key)) newTable.Indexes[i].Create();
            }
            #endregion
        }
        private static void AlterFields(Table prevTable, Table newTable)
        {
            var dicoNewEntities = new HashSet<string>();
            var dicoPrevEntities = new HashSet<string>();
            #region generate dico of fields
            for (var i = 0; i < prevTable.Fields.Length; ++i)
                if (!dicoPrevEntities.Contains(prevTable.Fields[i].Name.ToUpper()))
                    dicoPrevEntities.Add(prevTable.Fields[i].Name.ToUpper());
            for (var i = 0; i < newTable.Fields.Length; ++i)
                if (!dicoNewEntities.Contains(newTable.Fields[i].Name.ToUpper()))
                    dicoNewEntities.Add(newTable.Fields[i].Name.ToUpper());
            #endregion
            #region add new fields on table
            for (var i = 0; i < newTable.Fields.Length; ++i)
                if (!dicoPrevEntities.Contains(newTable.Fields[i].Name.ToUpper()))
                    newTable.Fields[i].Add(newTable);
            #endregion
            #region remove fields on table
            for (var i = 0; i < prevTable.Fields.Length; ++i)
                if (!dicoNewEntities.Contains(prevTable.Fields[i].Name.ToUpper()))
                    prevTable.Fields[i].Remove(prevTable);
            #endregion
        }
        private static string GetIndexesdKey(Table table)
        {
            var result = new StringBuilder();
            for (var i = 0; i < table.Indexes.Length; ++i) result.AppendLine(table.Indexes[i].GetKey());
            return result.ToString().ToUpper();
        }
        private static string GetFielsdKey(Table table)
        {
            var result = new StringBuilder();
            for (var i = 0; i < table.Fields.Length; ++i) result.AppendLine(table.Fields[i].Name);
            for (var i = 0; i < table.Relations.Length; ++i) result.AppendLine(table.Relations[i].Name);
            return result.ToString().ToUpper();
        }
        private static void UpdateMetadata(Table prevTable, Table newTable)
        {
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get();
            var log = new Logger(typeof(TableExtension));
            var jobId = Global.Databases.UpgradeJobId;
            var metaBuilder = new MetaDataBuilder();

            try
            {
                var meta = new MetaData
                {
                    Flags = metaBuilder.GetFlags(newTable),
                    Description = newTable.Description,
                    Value = newTable.Subject
                };
                SqlHelper.UpdateMetaData(connection, prevTable.SchemaId, prevTable.Id, Constants.TableId, 0, meta);
            }
            catch (Exception ex)
            {
                log.Error(prevTable.SchemaId, jobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        private static void DeleteMetadata(IDbConnection connection, Table table)
        {
            var lst = MetaDataExtension.GetMetaDataList(connection, table.SchemaId, null, table.Id, null);
	        lst.AppendItem(MetaDataExtension.GetMetaDataList(connection, table.SchemaId, EntityType.Table, null, table.Id));
			lst.AppendItem(MetaDataExtension.GetMetaDataIdList(connection, table.SchemaId, EntityType.Table, table.Id));
            MetaDataExtension.DeleteMetaDataList(connection, lst);
        }

        #endregion

    }
}
