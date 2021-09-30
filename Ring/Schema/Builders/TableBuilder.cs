using Ring.Data;
using Ring.Data.Enums;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;

namespace Ring.Schema.Builders
{
    internal sealed class TableBuilder : EntityBuilder
    {
        private readonly FieldBuilder _fieldBuilder = new FieldBuilder();
        private readonly IndexBuilder _indexBuilder = new IndexBuilder();

        /// <summary>
        /// Get Meta Dictionary instance eg. dba_object, sqlite_master, ...
        /// </summary>
        public Table GetDictionaryTable(DatabaseProvider provider)
        {
            string physicalName;
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var relations = new Relation[0];
            var indexes = new Index[0];
            var primaryKey = _fieldBuilder.GetInstance(0, Constants.SqliteDictionaryName, string.Empty, FieldType.String, 0, true); // default 
            switch (provider)
            {
                case DatabaseProvider.Sqlite:
                    physicalName = Constants.SqliteDictionary;
                    fieldList.Add(_fieldBuilder.GetInstance(1, Constants.SqliteDictionaryType, string.Empty, FieldType.String, 0, true));
                    fieldList.Add(primaryKey);
                    fieldList.Add(_fieldBuilder.GetInstance(2, Constants.SqliteDictionaryTblName, string.Empty, FieldType.String, 0, true));
                    fieldList.Add(_fieldBuilder.GetInstance(3, Constants.SqliteDictionaryRootPage, string.Empty, FieldType.Long, 0, true));
                    fieldList.Add(_fieldBuilder.GetInstance(4, Constants.SqliteDictionarySql, string.Empty, FieldType.String, 0, true));
                    break;
                case DatabaseProvider.PostgreSql:
                    physicalName = Constants.PostgreDictionary;
                    primaryKey = _fieldBuilder.GetInstance(0, _fieldBuilder.GetFieldName(provider, TableType.TableDictionary, FieldKey.Name),
                        string.Empty, FieldType.String, 0, true);
                    fieldList.Add(primaryKey);
                    fieldList.Add(_fieldBuilder.GetInstance(2, _fieldBuilder.GetFieldName(provider, TableType.TableDictionary, FieldKey.SchemaName),
                        string.Empty, FieldType.String, 0, true));
                    fieldList.Add(_fieldBuilder.GetInstance(4, Constants.PostgreDictionaryOwner, string.Empty, FieldType.String, 0, true));
                    break;
                case DatabaseProvider.Oracle:
                    physicalName = Constants.OracleDictionary;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }

            // compute select string
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);  // NO NEED already sorted by Id 

            var port = Constants.DefaultMetaSchemaId; // get dictionary table 

            return new Table(
                (int)TableType.TableDictionary,
                Constants.MetaDictionary,
                string.Empty,
                string.Empty,
                physicalName,
                TableType.TableDictionary,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                port,
                Constants.FieldNameNotFound,
                Constants.DefaultLexiconIndexes,
                PhysicalType.View,
				false,
                true,
                true,
                true,
                false,
                new CacheId(new object(), 0L, 0L, 0));
        }

        /// <summary>
        /// Get Meta Dictionary instance eg. dba_object, sqlite_master, ...
        /// </summary>
        public Table GetDictionaryTableSpace(DatabaseProvider provider)
        {
            string physicalName;
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var relations = new Relation[0];
            var indexes = new Index[0];
            Field primaryKey; // default 
            switch (provider)
            {
                case DatabaseProvider.PostgreSql:
                    physicalName = Constants.PostgreDictionaryTableSpace;
                    primaryKey = _fieldBuilder.GetInstance(0, _fieldBuilder.GetFieldName(provider, TableType.TableSpaceDictionary, FieldKey.Name),
                        string.Empty, FieldType.String, 0, true);
                    fieldList.Add(primaryKey);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // compute select string
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);  // NO NEED already sorted by Id 

            var port = Constants.DefaultMetaSchemaId; // get dictionary table 

            return new Table(
                (int)TableType.TableSpaceDictionary,
                Constants.MetaDictionaryTableSpace,
                string.Empty,
                string.Empty,
                physicalName,
                TableType.TableSpaceDictionary,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                port,
                Constants.FieldNameNotFound,
                Constants.DefaultLexiconIndexes,
                PhysicalType.View,
				false,
                true,
                true,
                true,
                false,
                new CacheId(new object(), 0L, 0L, 0));
        }

        /// <summary>
        /// Get fake Table
        /// </summary>
        public static Table GetFake(ItemType itemType)
        {
            var relations = new Relation[0];
            var indexes = new Index[0];
            var fieldType = FieldType.NotDefined;
            var primaryKeyhIndex = 0;
            var fieldBuilder = new FieldBuilder();

            switch (itemType)
            {
                case ItemType.String:
                    fieldType = FieldType.String;
                    break;
                case ItemType.Long:
                    fieldType = FieldType.Long;
                    break;
            }

            var fieldList = new List<Field>
            {
                fieldBuilder.GetInstance(0, Constants.FakeRecordFieldValue, string.Empty, fieldType, 0, true),
            };

            var fieldArr = fieldList.ToArray();

            return new Table(
                (int)TableType.TableDictionary,
                Constants.MetaDictionary,
                string.Empty,
                string.Empty,
                null,
                TableType.TableDictionary,
                relations,
                fieldArr,
                fieldArr,
                indexes,
                null,
                0,
                primaryKeyhIndex,
                Constants.DefaultLexiconIndexes,
                PhysicalType.View,
				false,
                true,
                true,
                true,
                false,
                new CacheId(new object(), 0L, 0L, 0));
        }

        /// <summary>
        /// Get Meta table
        /// </summary>
        public Table GetMeta(string schemaName, DatabaseProvider provider)
        {
            var fieldList = new List<Field>();
            var fieldBuilder = new FieldBuilder();
            var fieldListById = new List<Field>();
            var metaName = Constants.MetaTableName;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[0];
            var primaryKey = fieldBuilder.GetInstance(0, Constants.MetaDataId, string.Empty, FieldType.Int, 0, true);
            var port = Constants.DefaultMetaSchemaId; // get dictionary table 
            var physicalName = GetPhysicalName(provider, schemaName, metaName);

            // "id","schema_id","reference_id","object_type"
            fieldList.Add(primaryKey);
            fieldList.Add(fieldBuilder.GetInstance(1, Constants.MetaDataSchemaId, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(2, Constants.MetaDataObjectType, string.Empty, FieldType.Byte, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(3, Constants.MetaDataRefId, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(4, Constants.MetaDataDataType, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(5, Constants.MetaDataFlags, string.Empty, FieldType.Long, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(6, Constants.MetaDataName, string.Empty, FieldType.String,
                Constants.MaxSizeObjectName, true));
            fieldList.Add(fieldBuilder.GetInstance(7, Constants.MetaDataDescription, string.Empty, FieldType.String, 0,
                false));
            fieldList.Add(fieldBuilder.GetInstance(8, Constants.MetaDataValue, string.Empty, FieldType.String, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(9, Constants.MetaDataActive, string.Empty, FieldType.Boolean, 0,
                true));

            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            /*
            (0)  id, 
            (1)  schema_id,  
            (2)  object_type, 
            (3)  reference_id,
            (4)  data_type, 
            (5)  flags, 
            (6)  name, 
            (7)  description, 
            (8)  value, 
            (9) active

             ----- 
            unique key (1)      id; reference_id; object_type
            unique key (2)      object_type; reference_id; lower(name)
            */

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);

            return new Table(
                (int)TableType.Meta,
                metaName,
                metaDesc,
                null,
                physicalName,
                TableType.Meta,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                port,
                Constants.FieldNameNotFound,
                Constants.DefaultLexiconIndexes,
                PhysicalType.Table,
                false,true, true, false, false,
                new CacheId(new object(), 0L, 0L, 0));
        }

        /// <summary>
        /// Generate the lexicon table 
        /// </summary>
        public Table GetUser(string schemaName, DatabaseProvider provider)
        {
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var metaName = Constants.UserTableName;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[1];
            var fieldBuilder = new FieldBuilder();
            var primaryKey = fieldBuilder.GetInstance(0, Constants.MetaDataId, string.Empty, FieldType.Int, 0, true);
            var physicalName = GetPhysicalName(provider, schemaName, metaName);

            fieldList.Add(primaryKey);
            fieldList.Add(fieldBuilder.GetInstance(1, Constants.UserSchemaId, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(2, Constants.UserLoginName, string.Empty, FieldType.String, 180, true, false));
            fieldList.Add(fieldBuilder.GetInstance(3, Constants.UserEmail, string.Empty, FieldType.String, 180, true));
            fieldList.Add(fieldBuilder.GetInstance(4, Constants.UserFieldPswrd, string.Empty, FieldType.String, 512, true));
            fieldList.Add(fieldBuilder.GetInstance(5, Constants.UserActive, string.Empty, FieldType.Boolean, 0, true));

            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);
            var result = new Table(
                (int)TableType.User,
                metaName,
                metaDesc,
                null,
                physicalName,
                TableType.User,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                0,
                GetFieldIndex(fieldList, primaryKey.Name),
                Constants.DefaultLexiconIndexes,
                PhysicalType.Table,
                false, true, true, true, false,
                new CacheId(new object(), 0L, 0L, 0));
            /*
            -----
            unique key(1)      schema_id, username
			*/

            result.Indexes[0] = _indexBuilder.GetInstance(1, metaName, string.Empty, result, new[] { Constants.UserSchemaId, Constants.UserLoginName }, true, false);

            return result;
        }

        /// <summary>
        /// Generate the lexicon table 
        /// </summary>
        public Table GetLexicon(string schemaName, DatabaseProvider provider)
        {
            const int metaId = (int)TableType.Lexicon;
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var metaName = Constants.LexiconTableName;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[2];
            var fieldBuilder = new FieldBuilder();
            var primaryKey = fieldBuilder.GetInstance(0, Constants.MetaDataId, string.Empty, FieldType.Int, 0, true);
            var port = Constants.DefaultMetaSchemaId; // get dictionary table 
            var physicalName = GetPhysicalName(provider, schemaName, metaName);

            // "id","schema_id","name","description","guid","table_id","field_id", "relation_id", "relation_value", "active"
            fieldList.Add(primaryKey);
            fieldList.Add(fieldBuilder.GetInstance(1, Constants.MetaDataSchemaId, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(2, Constants.MetaDataName, string.Empty, FieldType.String, 80, true));
            fieldList.Add(fieldBuilder.GetInstance(3, Constants.MetaDataDescription, string.Empty, FieldType.String, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(4, Constants.LexiconGuid, string.Empty, FieldType.String, 36, true));
            fieldList.Add(fieldBuilder.GetInstance(5, Constants.LexiconTableId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(6, Constants.LexiconFromFieldId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(7, Constants.LexiconToFieldId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(8, Constants.LexiconRelationId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(9, Constants.LexiconRelationValue, string.Empty, FieldType.Long, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(10, Constants.MetaDataModifyStmp, string.Empty, FieldType.DateTime, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(11, Constants.MetaDataActive, string.Empty, FieldType.Boolean, 0, true));
            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            /*
            (0)  id, 
            (1)  schema_id,  
            (2)  name, 
            (3)  description, 
            (4)  guid, 
            (5)  table_id, 
            (6)  field_id,
            (7)  relation_id,
            (8)  relation_value,
            (9)  active,

            */

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);
            var result = new Table(
                metaId,
                metaName,
                metaDesc,
                null,
                physicalName,
                TableType.Lexicon,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                port,
                GetFieldIndex(fieldList, primaryKey.Name),
                Constants.DefaultLexiconIndexes,
                PhysicalType.Table,
                false, true, true, true, false,
                new CacheId(new object(), 0L, 0L, 0));
            /*
            -----
            unique key(1)      guid
            unique key(2)      schema_id, table_id, field_id, relation_id, relation_value
            */

            var arrIndex = 0;

            result.Indexes[arrIndex] =
                _indexBuilder.GetInstance(metaId + arrIndex, metaName, string.Empty, result, Constants.LexiconGuid, true, false);
            ++arrIndex;
            result.Indexes[arrIndex] = _indexBuilder.GetInstance(metaId + arrIndex, metaName, string.Empty, result,
                new[] { Constants.MetaDataSchemaId, Constants.LexiconTableId, Constants.LexiconToFieldId, Constants.LexiconRelationId,
                    Constants.LexiconRelationValue }, true, false);
            return result;
        }

        /// <summary>
        /// Generate the lexicon item table 
        /// </summary>
		public Table GetLexiconItem(string schemaName, DatabaseProvider provider)
        {
            const int metaId = (int)TableType.LexiconItem;
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var metaName = Constants.LexiconItemTableName;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[1];
            var fieldBuilder = new FieldBuilder();
            var primaryKey = fieldBuilder.GetInstance(0, Constants.MetaDataId, string.Empty, FieldType.Int, 0, true);
            var port = Constants.DefaultMetaSchemaId; // get dictionary table 
            var physicalName = GetPhysicalName(provider, schemaName, Constants.LexiconItemPhysTableName);

            // "id","lexicon_id","reference_id", "value"
            fieldList.Add(primaryKey);
            fieldList.Add(fieldBuilder.GetInstance(1, Constants.LexiconItemLexiId, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(2, Constants.LexiconItemRefId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(3, Constants.LexiconItemValue, string.Empty, FieldType.String, 0, false));

            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            /*
            (0)  id, 
            (1)  lexicon_id,  
            (2)  reference_id, 
            (3)  value, 
            */

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);

            var result = new Table(
                metaId,
                metaName,
                metaDesc,
                null,
                physicalName,
                TableType.LexiconItem,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                port,
                Constants.FieldNameNotFound,
                Constants.DefaultLexiconIndexes,
                PhysicalType.Table,
                false, true, true, true, false,
                new CacheId(new object(), 0L, 0L, 0));
            /*
            -----
            unique key(1)      id
            */
            result.Indexes[0] = _indexBuilder.GetInstance(metaId, metaName, string.Empty, result,
                new[] { Constants.MetaDataId, Constants.LexiconItemLexiId, Constants.LexiconItemRefId }, true, false);
            return result;
        }

        /// <summary>
        /// Generate the table to querying if a schema exist
        /// </summary>
        public Table GetDictionarySchema(DatabaseProvider provider)
        {
            const int metaId = (int)TableType.SchemaDictionary;
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var metaName = Constants.MetaDictionarySchema;
            var physicalName = string.Empty;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[0];
            var port = Constants.DefaultMetaSchemaId; // get dictionary table 
            Field primaryKey = null;

            switch (provider)
            {
                case DatabaseProvider.PostgreSql:
                    physicalName = Constants.PostgreDictionarySchema;
                    primaryKey = _fieldBuilder.GetInstance(0, Constants.PostgreDictionarySchemaNameField,
                        string.Empty, FieldType.String, 0, true); // default 
                    break;
            }

            // "name"
            fieldList.Add(primaryKey);

            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            /*
            (0)  name, 
            */

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            fieldListById.Sort((x, y) => x.Id - y.Id);

            var result = new Table(
                metaId,
                metaName,
                metaDesc,
                null,
                physicalName,
                TableType.SchemaDictionary,
                relations,
                fieldList.ToArray(),
                fieldListById.ToArray(),
                indexes,
                primaryKey,
                port,
                0,
                Constants.DefaultLexiconIndexes,
                PhysicalType.View,
				false,
                true,
                true,
                true,
                false,
                new CacheId(new object(), 0L, 0L, 0));

            return result;
        }

        /// <summary>
        /// Get @log
        /// </summary>
        public Table GetLog(string schemaName, DatabaseProvider provider)
        {
            const int metaId = 0; // set id to 0 to create first this table before others
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var metaName = Constants.MetaLogTableName;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[2];
            var fieldBuilder = new FieldBuilder();
            var port = Constants.DefaultMetaSchemaId; // get dictionary table 
            var physicalName = GetPhysicalName(provider, schemaName, metaName);

            // "id","entry_time","level_id","thread_id","call_site","message","description","machine_name"
            fieldList.Add(fieldBuilder.GetInstance(0, Constants.MetaLogId, string.Empty, FieldType.Short, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(1, Constants.MetaLogEntryTime, string.Empty, FieldType.LongDateTime, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(2, Constants.MetaLogLevel, string.Empty, FieldType.Short, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(3, Constants.MetaSchemaId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(3, Constants.MetaLogThreadId, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(4, Constants.MetaLogCallSite, string.Empty, FieldType.String, 255, false));
            fieldList.Add(fieldBuilder.GetInstance(5, Constants.MetaLogJobId, string.Empty, FieldType.Long, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(6, Constants.MetaLogMethod, string.Empty, FieldType.String, 80, false));
            fieldList.Add(fieldBuilder.GetInstance(7, Constants.MetaLogLineNumber, string.Empty, FieldType.Int, 0, false));
            fieldList.Add(fieldBuilder.GetInstance(8, Constants.MetaLogMessage, string.Empty, FieldType.String, 255, false));
            fieldList.Add(fieldBuilder.GetInstance(9, Constants.MetaLogDescription, string.Empty, FieldType.String, int.MaxValue, false));

            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));

            var result = new Table(
                   metaId,
                   metaName,
                   metaDesc,
                   null,
                   physicalName,
                   TableType.Log,
                   relations,
                   fieldList.ToArray(),
                   fieldListById.ToArray(),
                   indexes,
                   null,
                   port,
                   Constants.FieldNameNotFound,
                   Constants.DefaultLexiconIndexes,
                   PhysicalType.Table,
                   true, true, true, false, false,
                   new CacheId(new object(), 0L, 0L, 0));

            //  id, name, description, Table table, string fieldName, bool unique
            var arrIndex = 0;
            result.Indexes[arrIndex] =
                _indexBuilder.GetInstance(metaId + arrIndex, metaName, string.Empty, result, Constants.MetaLogEntryTime, false, false);
            ++arrIndex;
            result.Indexes[arrIndex] =
                _indexBuilder.GetInstance(metaId + arrIndex, metaName, string.Empty, result, Constants.MetaLogJobId, false, false);
            return result;
        }

        /// <summary>
        /// Get MetaId table
        /// </summary>
        public Table GetMetaId(string schemaName, DatabaseProvider provider)
        {
            var fieldList = new List<Field>();
            var fieldListById = new List<Field>();
            var metaName = Constants.MetaTableIdName;
            var metaDesc = string.Empty;
            var relations = new Relation[0];
            var indexes = new Index[0];
            var fieldBuilder = new FieldBuilder();
            var primaryKey = fieldBuilder.GetInstance(0, Constants.MetaDataId, string.Empty, FieldType.Int, 0, true);
            var port = Constants.DefaultMetaSchemaId; // get dictionary table 

            fieldList.Add(primaryKey);
            fieldList.Add(fieldBuilder.GetInstance(2, Constants.MetaDataSchemaId, string.Empty, FieldType.Int, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(4, Constants.MetaDataObjectType, string.Empty, FieldType.Byte, 0, true));
            fieldList.Add(fieldBuilder.GetInstance(8, Constants.MetaDataValue, string.Empty, FieldType.Long, 0, true));

            // build select 
            for (var i = 0; i < fieldList.Count; ++i) fieldListById.Add(fieldList[i]);

            /*
            (1)  id, 
            (2)  schema_id,  
			(3)  object_type,  
            (4)  value, 

             ----- 
            unique key (1)      id; schema_id; object_type
            */

            // (1)
            // (2)
            // (3)

            // sort field 
            fieldList.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));

            return new Table(
                   (int)TableType.MetaId,
                   metaName,
                   metaDesc,
                   null,
                   GetPhysicalName(provider, schemaName, metaName),
                   TableType.MetaId,
                   relations,
                   fieldList.ToArray(),
                   fieldListById.ToArray(),
                   indexes,
                   primaryKey,
                   port,
                   Constants.FieldNameNotFound,
                   Constants.DefaultLexiconIndexes,
                   PhysicalType.Table,
                   false, true, true, true, false,
                   new CacheId(new object(), 0L, 0L, 0));
        }

        /// <summary>
        /// Get Mtm tables
        /// </summary>
        public Table[] GetMtms(int schemaId, string schemaName, DatabaseProvider driver, List<Relation> relation)
        {
            var tableDico = new Dictionary<string, Table>(); // <tableName.Tolower(), Table>
            var result = new List<Table>();
            var emptyFields = new Field[0];

            // create table 
            for (var i = 0; i < relation.Count; ++i)
            {
                var currentRelation = relation[i];
	            var currentRelationIndex = GetRelationIndex(currentRelation);
	            if (!tableDico.ContainsKey(currentRelation.MtmTable.ToLower()))
	            {
		            var relations = new Relation[2];
		            var index = new Index[1];
		            var currentTable = new Table(int.MinValue, currentRelation.MtmTable, string.Empty, string.Empty,
			            GetPhysicalName(driver, schemaName, currentRelation.MtmTable), TableType.Mtm, relations,
			            emptyFields, emptyFields, index, null, schemaId,
			            Constants.FieldNameNotFound, null, PhysicalType.Table, false, false, true, false, false,
			            Constants.MtmCacheId);
		            relations[currentRelationIndex] = currentRelation;
		            tableDico.Add(currentRelation.MtmTable.ToLower(), currentTable);
		            result.Add(currentTable);
	            }
	            else
		            tableDico[currentRelation.MtmTable.ToLower()].Relations[currentRelationIndex] = currentRelation;
            }
	        LoadMtmIndexes(result);
			return result.ToArray();
        }

		/// <summary>
		/// Get table instance
		///					!!! Important !!! metaElements must be sorted by name
		/// </summary>
		public Table GetInstance(string searchPath, MetaData meta, MetaData[] metaElements, int indexMin, int indexMax, SchemaLoadType loadType,
            SchemaSourceType sourceType, DatabaseProvider provider, int schemaId, ref int lexiconId)
        {
            var fieldList = new List<Field>();
			var fieldListById = new List<Field>();
            var primaryKey = _fieldBuilder.GetDefaultPrimaryKey(sourceType);
            var physicalName = GetPhysicalName(provider, sourceType, searchPath, meta.Name);
            var subject = meta.Value ?? string.Empty;
	        var lexiconList = new List<LexiconIndex>();

			// build object arrays
	        LoadFieldLists(metaElements, indexMin, indexMax, fieldList, fieldListById, lexiconList, loadType, sourceType, provider, ref lexiconId);

			// sort lexicon indexers 
			if (lexiconList.Count > 0) lexiconList.Sort((x, y) => x.FieldId.CompareTo(y.FieldId));

            // get position of primary key into Fields list if it's not defined !!
            // insert primary key to right place
            var primaryKeyFieldIndex = GetFieldIndex(fieldList, primaryKey.Name);
	        if (primaryKeyFieldIndex == Constants.FieldNameNotFound)
	        {
				fieldList.Insert(GetFieldPositionByName(fieldList, primaryKey.Name), primaryKey);
		        fieldListById.Insert(0, primaryKey); 
	        }
	        else primaryKey = fieldList[primaryKeyFieldIndex]; 
            
            fieldListById.Sort((x, y) => x.Id - y.Id);

            return new Table(
                int.Parse(meta.Id),
                meta.GetEntityName(),
                meta.GetEntityDescription(loadType),
                subject,
                physicalName,
                TableType.Business,
                new Relation[GetRelationCount(metaElements, indexMin, indexMax)],
                fieldList.ToArray(),
                fieldListById.ToArray(),
                new Index[GetIndexCount(metaElements, indexMin, indexMax)],
                primaryKey,
                schemaId,
                primaryKeyFieldIndex,
                lexiconList.Count > 0 ? lexiconList.ToArray() : Constants.DefaultLexiconIndexes,
                PhysicalType.Table,
				false,
                meta.IsBaselined(),
                meta.IsEnabled(),
                meta.IsTableCached(),
                meta.IsTableReadonly(),
                meta.GetCacheId());
        }

        /// <summary>
        /// Get schema name from Physical Tablename
        /// </summary>
        /// <param name="tableName">Physical Tablename</param>
        public static string GetSchemaName(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return tableName;
            var index = tableName.IndexOf(Constants.TableNameSperator);
            return index > 0 ? tableName.Substring(0, index) : string.Empty;
        }

        /// <summary>
        /// Get logical table name from physical definition
        /// </summary>
        /// <param name="tableName">Physical Tablename</param>
        public static string GetTableName(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return tableName;
            var index = tableName.IndexOf(Constants.TableNameSperator);
            var result = index > 0 ? tableName.Substring(index + 1) : string.Empty;
            return result.Trim(Constants.TrimSeparatorList);
        }

        #region private methods

        /// <summary>
        /// calculate index of relation into @mtm tables
        /// </summary>
        /// <param name="relation"></param>
        /// <returns>0 or 1</returns>
        private static int GetRelationIndex(Relation relation) =>
            string.CompareOrdinal(relation.Name, relation.InverseRelationName) >= 0 ? 1 : 0;

        /// <summary>
        /// get Field index before table exists 
        /// </summary>
        /// <returns></returns>
        private static int GetFieldIndex(List<Field> fields, string fieldName)
        {
            int indexerLeft = 0, indexerRigth = fields.Count - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(fieldName, fields[indexerMiddle].Name);
                if (indexerCompare == 0) return indexerMiddle;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return Constants.FieldNameNotFound;
        }

        /// <summary>
        /// Generate physical name
        /// </summary>
        private static string GetPhysicalName(DatabaseProvider provider, string schemaName, string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var template = Constants.MetaPhysicalTableNameSperator;
            if (char.IsLetter(name[0])) template = Constants.CommonPhysicalTableNameSperator;
            var schema = schemaName != null && provider == DatabaseProvider.PostgreSql ? schemaName.ToLower() : schemaName;
            switch (provider)
            {
                case DatabaseProvider.MySql:
                case DatabaseProvider.PostgreSql:
                case DatabaseProvider.Oracle:
                    return schemaName != null ? schema + Constants.TableNameSperator + string.Format(template, name) : string.Format(template, name);
                case DatabaseProvider.Sqlite:
                    return string.Format(Constants.MetaPhysicalTableNameSperator, name);
            }
            throw new ArgumentOutOfRangeException(provider.ToString(), provider, null);
        }

        /// <summary>
        /// Generate physical name for common tables 
        /// </summary>
        private static string GetPhysicalName(DatabaseProvider provider, SchemaSourceType sourceType, string schemaName, string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            switch (sourceType)
            {
                case SchemaSourceType.ClfyDataBase:
                case SchemaSourceType.ClfyXml:
                    return GetPhysicalName(provider, schemaName, Constants.TablePrefixClfy + name);
                case SchemaSourceType.NativeDataBase:
                case SchemaSourceType.NativeXml:
                    return GetPhysicalName(provider, schemaName, Constants.TablePrefixDefault + name);
            }
            throw new ArgumentOutOfRangeException(sourceType.ToString(), sourceType, null);
        }

	    /// <summary>
	    /// Get position index for a new field into a list sorted by name
	    /// </summary>
	    private static int GetFieldPositionByName(List<Field> fields, string newFieldName)
	    {
		    var result = 0;
		    //TODO use dichotomic search (REDO)
		    for (var i = 0; i < fields.Count; ++i, ++result)
			    if (string.CompareOrdinal(fields[i].Name, newFieldName) > 0)
				    break;
		    return result;
	    }


	    /// <summary>
	    /// Get position index for a new field into a list sorted by name
	    /// </summary>
	    private int GetRelationCount(MetaData[] metaElements, int indexMin, int indexMax)
	    {
		    var result = 0; 
		    for (var i = indexMin; i <= indexMax; ++i) if (metaElements[i].GetEntityType() == EntityType.Relation) ++result;
			return result;
	    }

	    /// <summary>
	    /// Get position index for a new field into a list sorted by name
	    /// </summary>
	    private int GetIndexCount(MetaData[] metaElements, int indexMin, int indexMax)
	    {
		    var result = 0;
		    for (var i = indexMin; i <= indexMax; ++i) if (metaElements[i].GetEntityType() == EntityType.Index) ++result;
		    return result;
	    }

		/// <summary>
		/// Get List object array 
		/// </summary>
	    private void LoadFieldLists(MetaData[] metaElements, int indexMin, int indexMax, List<Field> fieldList, List<Field> fieldListById, List<LexiconIndex> lexiconList, 
			SchemaLoadType loadType, SchemaSourceType sourceType, DatabaseProvider provider, ref int lexiconId)
	    {
			for (var i = indexMin; i <= indexMax; ++i)
		    {
			    var entityType = metaElements[i].GetEntityType();
			    switch (entityType)
			    {
				    case EntityType.Field:
					    var fieldNewField = _fieldBuilder.GetInstance(provider, metaElements[i], loadType, sourceType);
					    fieldList.Add(fieldNewField);
					    if (fieldNewField.Multilingual)
					    {
						    if (lexiconList.Count == 0) lexiconList.Add(new LexiconIndex(-1, lexiconId++));
						    lexiconList.Add(new LexiconIndex(fieldNewField.Id, lexiconId++));
					    }
					    fieldListById.Add(fieldList[fieldList.Count - 1]);
					    break;
			    }
		    }

		}

	    /// <summary>
	    /// Get List object array 
	    /// </summary>
	    private void LoadMtmIndexes(List<Table> tableList)
	    {
		    for (var i = 0; i < tableList.Count; ++i)
			    tableList[i].Indexes[0] = _indexBuilder.GetInstance(tableList[i].Relations);
	    }


		#endregion

	}
}
