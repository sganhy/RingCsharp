using Ring.Data;
using Ring.Data.Core;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Core.Rules.Impl;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util;
using Ring.Web;
using System;
using System.Collections.Generic;
using System.IO;
using DataBase = Ring.Schema.Models.Schema;

namespace Ring.Schema.Builders
{
    internal sealed class SchemaBuilder : EntityBuilder
    {
        private readonly TableBuilder _tableBuilder = new TableBuilder();
        private readonly IndexBuilder _indexBuilder = new IndexBuilder();
        private readonly LanguageBuilder _languageBuilder = new LanguageBuilder();
        private readonly MetaDataBuilder _metaDataBuilder = new MetaDataBuilder();
        private readonly RelationBuilder _relationBuilder = new RelationBuilder();
        private readonly SequenceBuilder _sequenceBuilder = new SequenceBuilder();
        private readonly TableSpaceBuilder _tableSpaceBuilder = new TableSpaceBuilder();

        /// <summary>
        /// Get instance from Xml/ Json file 
        /// </summary>
        public DataBase GetInstance(Stream doc, DatabaseProvider provider, SchemaLoadType loadType)
        {
            var source = GetSchemaType(doc);
            var metaArr = _metaDataBuilder.GetInstances(doc);
            var connPool = GetConnectionPool(metaArr);
            var metaList = new List<MetaData>(_metaDataBuilder.GetInstances(doc));

            #region validatons
            var validations = new ValidationResult();
            for (var i = 0; i < Constants.MetadaValidators.Length; ++i)
                validations.Merge(Constants.MetadaValidators[i].Validate(metaArr));
            #endregion

            // generate - connection pool 
            var schema = GetInstance(connPool, provider, validations, metaList.ToArray(), loadType, source);

            return schema;
        }

        /// <summary>
        /// Get instance of schema from database
        /// </summary>
        public DataBase GetInstance(DatabaseProvider provider, MetaData[] metalist, SchemaLoadType loadType)
        {
            #region validatons

            var validations = new ValidationResult();
            for (var i = 0; i < Constants.MetadaValidators.Length; ++i)
                validations.Merge(Constants.MetadaValidators[i].Validate(metalist));

            #endregion

            var sourceType = GetSchemaType(metalist);
            // define source here 
            var schema = GetInstance(GetConnectionPool(metalist), provider, validations, metalist, loadType, sourceType);

            //TODO to remove just for test 
            //var meta = MetaDataMapper.GetInstances(schema);

            return schema;
        }

        /// <summary>
        /// Get @meta schema instance 
        /// </summary>
        public DataBase GetInstance(ConnectionPool pool)
        {
            var defaultSchema = GetDefaultSearchPath(true, pool.ConnectionRef.Provider);
            var metaTable = _tableBuilder.GetMeta(defaultSchema, pool.ConnectionRef.Provider);
            var metaIdTable = _tableBuilder.GetMetaId(defaultSchema, pool.ConnectionRef.Provider);
            var metaLogTable = _tableBuilder.GetLog(defaultSchema, pool.ConnectionRef.Provider);
            var lexiconTable = _tableBuilder.GetLexicon(defaultSchema, pool.ConnectionRef.Provider);
            var lexiconItemTable = _tableBuilder.GetLexiconItem(defaultSchema, pool.ConnectionRef.Provider);
            var userTable = _tableBuilder.GetUser(defaultSchema, pool.ConnectionRef.Provider);
            var defaultLanguage = _languageBuilder.GetDefaultInstance();
            var defaultTableSpaces = new TableSpace[0];
            var id = Constants.DefaultMetaSchemaId;
            var mtm = new Table[0];

            // generate dictionary table 
            var metaDictionary = _tableBuilder.GetDictionaryTable(pool.ConnectionRef.Provider);
            var metaDictionarySchema = _tableBuilder.GetDictionarySchema(pool.ConnectionRef.Provider);
            var metaDictionaryTableSpace = _tableBuilder.GetDictionaryTableSpace(pool.ConnectionRef.Provider);

            var tablesById = new[]
            {
                metaDictionary, metaTable, metaIdTable, metaLogTable, lexiconTable, lexiconItemTable, metaDictionaryTableSpace,
                metaDictionarySchema, userTable
            };
            var tablesByName = new[]
            {
                metaDictionary, metaTable, metaIdTable, metaLogTable, lexiconTable, lexiconItemTable, metaDictionaryTableSpace,
                metaDictionarySchema, userTable
            };

            Array.Sort(tablesByName, (x, y) => string.CompareOrdinal(x.Name, y.Name));
            Array.Sort(tablesById, (x, y) => x.Id - y.Id);

            return new DataBase(id, metaTable.Name, null, true, true,
                null, null, id, true, defaultLanguage, pool.ConnectionRef.Provider, tablesByName, tablesById,
                mtm, new Sequence[0], new Lexicon[0],
                pool, SchemaSourceType.NativeXml, SchemaLoadType.Full,
                new ValidationResult(), null, DateTime.Now, defaultTableSpaces, defaultSchema);
        }

        #region private methods 

        /// <summary>
        /// LoadXml the relations for a table of a schema
        /// </summary>
        /// <param name="table">MetaData of table</param>
        /// <param name="metaElements">Table definition relation, indexes and fields</param>
        /// <param name="currentSchema">SchemaExtension</param>
        /// <param name="indexMin">Start index of element in meta list</param>
        /// <param name="indexMax">End index of element in meta list</param>
        /// <param name="relationDico">Inverse relation id</param>
        private void LoadRelations(MetaData table, MetaData[] metaElements, DataBase currentSchema, int indexMin, int indexMax,
            Dictionary<int, Dictionary<string, int>> relationDico)
        {
            if (currentSchema == null) return;

            var relIndex = 0;
            var currentTable = currentSchema.GetTable(int.Parse(table.Id));

            for (var i = indexMin; i <= indexMax; ++i)
            {
                var currentRelation = metaElements[i];
                if (!currentRelation.IsRelation()) continue;
                var inverseRelationId = -1;

                if (relationDico.ContainsKey(currentRelation.DataType) &&
                    relationDico[currentRelation.DataType].ContainsKey(currentRelation.Value.ToUpper()))
                    inverseRelationId = relationDico[currentRelation.DataType][currentRelation.Value.ToUpper()];

                currentTable.Relations[relIndex] =
                    _relationBuilder.GetInstance(currentRelation, table, currentSchema.GetTable(currentRelation.DataType),
                        currentSchema.LoadType, currentSchema.Source, inverseRelationId);

                ++relIndex;
            }
        }

        /// <summary>
        /// LoadXml the indexes for a table of a schema
        /// </summary>
        /// <param name="table">MetaData of table</param>
        /// <param name="metaElements">Table definition relation, indexes and fields</param>
        /// <param name="currentSchema">SchemaExtension</param>
        /// <param name="indexMin">Start index of element in meta list</param>
        /// <param name="indexMax">End index of element in meta list</param>
        private void LoadIndexes(MetaData table, MetaData[] metaElements, DataBase currentSchema, int indexMin, int indexMax)
        {
            var index = 0;
            var currentTable = currentSchema.GetTable(int.Parse(table.Id));
            if (currentTable == null) return;
            var comparisonType = GetComparisonType(currentSchema); // used to find field/ relation object 

            // loop for each indexes of table
            for (var i = indexMin; i <= indexMax; ++i)
            {
                var currentMetaElm = metaElements[i];
                if (!currentMetaElm.IsIndex()) continue;
                if (string.IsNullOrWhiteSpace(currentMetaElm.Value)) continue;
                var elementList = currentMetaElm.Value;
                // remove last semicolon
                if (currentMetaElm.Value.EndsWith(Constants.IndexFieldSeparator.ToString()))
                    elementList = currentMetaElm.Value.Substring(0, currentMetaElm.Value.Length - 1);
                var arrSplit = elementList.Split(Constants.IndexFieldSeparator);
                var fieldList = new List<BaseEntity>();

                for (var j = 0; j < arrSplit.Length; ++j)
                {
                    // is it a field or a relation?
                    // from database use ordinal comparison
                    var field = currentTable.GetField(arrSplit[j], comparisonType);
                    if (field != null)
                    {
                        fieldList.Add(field);
                        continue;
                    }
                    var relation = currentTable.GetRelation(arrSplit[j], comparisonType);
                    if (relation == null)
                    {
                        throw new ArgumentException();
                    }
                    fieldList.Add(relation);
                    //TODO : add logging 
                }

                currentTable.Indexes[index] = _indexBuilder.GetInstance(currentMetaElm, fieldList.ToArray(), currentTable);
                ++index;
            }
        }

        /// <summary>
        /// Define source type from a file 
        /// </summary>
        private SchemaSourceType GetSchemaType(Stream doc) => IsClarifySChema(doc) ? SchemaSourceType.ClfyXml : SchemaSourceType.NativeXml;

        /// <summary>
        /// define Schema source type from a metalist 
        /// </summary>
        private static SchemaSourceType GetSchemaType(MetaData[] metalist)
        {
            var result = SchemaSourceType.NotDefined;
            for (var i = 0; i < metalist.Length; ++i)
            {
                if (!metalist[i].IsSchema()) continue;
                result = metalist[i].GetSchemaType();
                break;
            }
            return result;
        }

        /// <summary>
        /// Define search 
        /// </summary>
        private static StringComparison GetComparisonType(DataBase currentSchema) =>
            currentSchema.Source == SchemaSourceType.ClfyDataBase || currentSchema.Source == SchemaSourceType.NativeDataBase
            ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Add an instance of schema model
        /// </summary>
        private DataBase GetInstance(ConnectionPool pool, DatabaseProvider provider, ValidationResult validation,
            MetaData[] metalist, SchemaLoadType loadType, SchemaSourceType source)
        {
            int i, startIndex;
            var tableCount = 0;
            var mtmCount = 0;
            var version = string.Empty;
            var name = string.Empty;
            var connectionString = string.Empty;
            var mtmRelationList = new List<Relation>();
            var sequences = new List<Sequence>();
            var defaultInstance = _languageBuilder.GetDefaultInstance();
            var lexiconCount = GetNumberOfLexicon(metalist);
            var lexiconId = 0;
            var searchPath = GetDefaultSearchPath(true, provider);
            var schemId = 0;
            var port = 0;

            MetaData metaTable;
            string objId;
            sequences.Clear();

            #region first pass read table information
            // number of table + schema info 
            for (i = 0; i < metalist.Length; ++i)
            {
                var meta = metalist[i];
                if (meta.IsTable()) ++tableCount;
                if (meta.IsRelation() &&
                    meta.GetRelationType() == RelationType.Mtm)
                    ++mtmCount;
                if (meta.IsLanguage()) defaultInstance = _languageBuilder.GetInstance(meta);
                if (!meta.IsSchema()) continue;
                name = meta.Name;
                schemId = int.Parse(meta.Id);
                port = meta.DataType;
                searchPath = string.IsNullOrEmpty(meta.Value) ? NamingConvention.ToSnakeCase(meta.Name) : NamingConvention.ToSnakeCase(meta.Value);
                connectionString = meta.Description;
            }
            if (tableCount <= 0) return null;
            mtmCount >>= 1;  // mtmCount div by 2 ==> 1 mtm table = 2 relations
            #endregion
            #region sort by RefId, Object_type, Name asc
            Array.Sort(metalist, (x, y) => x.RefId != y.RefId ?
                           string.CompareOrdinal(x.RefId, y.RefId) :
                           x.ObjectType != y.ObjectType ?
                           x.ObjectType.CompareTo(y.ObjectType) :
                           string.Compare(x.Name, y.Name, StringComparison.Ordinal));     // order by RefId, Object_type, name asc --> not so bad !!!
            #endregion
            #region sequences 
            // number of table + schema info 
            for (i = 0; i < metalist.Length; ++i)
                if (metalist[i].IsSequence())
                    sequences.Add(_sequenceBuilder.GetInstance(port, metalist[i]));
            #endregion
            #region default sequences 
            #endregion
            #region allow structures
            var tablesByName = new Table[validation.IsBlockingDefect ? 0 : tableCount];
            var tablesById = new Table[validation.IsBlockingDefect ? 0 : tableCount];
            var tablesMtm = new Table[validation.IsBlockingDefect ? 0 : mtmCount];
            var result = new DataBase(schemId, name, null, true, true, version, connectionString, port,
                false, defaultInstance, provider, tablesByName, tablesById, tablesMtm, sequences.ToArray(), new Lexicon[lexiconCount],
                pool, source, loadType, validation, new WebServer(port), DateTime.Now, GetTableSpaces(metalist, schemId), searchPath);
            if (validation.IsBlockingDefect) return result;
            #endregion
            #region generate table arrays & manage fields (first pass)
            i = 0;
            var tableIndex = 0;
            while (i < metalist.Length && tableIndex < tablesByName.Length)
            {
                objId = metalist[i].RefId;
                startIndex = i;
                metaTable = Constants.NullMetada; // inialize 

                for (; i < metalist.Length && metalist[i].RefId == objId; ++i)
                    if (metalist[i].IsTable()) metaTable = metalist[i];

                long tableId;
                if (!long.TryParse(metaTable.Id, out tableId) || !metaTable.IsTable()) continue;
                var currentTableObj = _tableBuilder.GetInstance(searchPath, metaTable, metalist,
                                        startIndex, i - 1, loadType, source, provider, schemId, ref lexiconId);

                tablesByName[tableIndex] = currentTableObj;
                tablesById[tableIndex] = currentTableObj;
                ++tableIndex;
            }
            #endregion
            #region sort array tables
            Array.Sort(tablesByName, (x, y) => string.CompareOrdinal(x.Name, y.Name));
            Array.Sort(tablesById, (x, y) => x.Id - y.Id);
            #endregion
            #region build inverse relation dictionary
            // <table_id, <relationShip name, relation_id>
            var relationDico = new Dictionary<int, Dictionary<string, int>>();
            for (i = 0; i < metalist.Length; ++i)
            {
                var metaObject = metalist[i];
                if (!metaObject.IsRelation()) continue;
                if (metaObject.GetRelationType() == RelationType.Mtm)
                {
                    var refId = int.Parse(metaObject.RefId);
                    if (!relationDico.ContainsKey(refId)) relationDico.Add(refId, new Dictionary<string, int>());
                    if (!relationDico[refId].ContainsKey(metaObject.Name.ToUpper()))
                        relationDico[refId].Add(metaObject.Name.ToUpper(), int.Parse(metaObject.Id));
                }
            }
            #endregion
            #region manage relations & indexes (second pass)
            i = 0;
            tableIndex = 0;
            while (i < metalist.Length && tableIndex < tablesByName.Length)
            {
                objId = metalist[i].RefId;
                startIndex = i;
                metaTable = Constants.NullMetada; // inialize 

                for (; i < metalist.Length && metalist[i].RefId == objId; ++i)
                    if (metalist[i].IsTable()) metaTable = metalist[i];
                long tableId;
                if (!long.TryParse(metaTable.Id, out tableId) || !metaTable.IsTable()) continue;

                LoadRelations(metaTable, metalist, result, startIndex, i - 1, relationDico);
                LoadIndexes(metaTable, metalist, result, startIndex, i - 1);

                ++tableIndex;
            }
            #endregion
            #region generate mtm 
            for (i = 0; i < result.TablesByName.Length; ++i)
            {
                var currentTable = result.TablesByName[i];
                for (var j = 0; j < currentTable.Relations.Length; ++j)
                    if (currentTable.Relations[j].MtmTable != null)
                        mtmRelationList.Add(currentTable.Relations[j]);
            }
            var mtmRelList = _tableBuilder.GetMtms(schemId, searchPath, provider, mtmRelationList);
            for (i = 0; i < mtmRelList.Length && i < result.MtmTables.Length; ++i)
                result.MtmTables[i] = mtmRelList[i];
            Array.Sort(tablesMtm, (x, y) => string.CompareOrdinal(x.Name, y.Name)); // sort tables 
            #endregion

            return result;
        }

        /// <summary>
        /// Generate a connection pool 
        /// </summary>
        private static ConnectionPool GetConnectionPool(MetaData[] metalist)
        {
            // Generate connection Pool
            var i = 0;
            var schemaId = (long)EntityType.Schema;
            var nullId = (long)EntityType.Null;
            var schemaMetadata = Constants.NullMetada;
            var connectionRef = Global.Databases.MetaSchema?.Connections.ConnectionRef;

            while (i < metalist.Length && schemaMetadata.ObjectType == nullId)
            {
                if (metalist[i].ObjectType == schemaId)
                    schemaMetadata = metalist[i];
                ++i;
            }
            if (schemaMetadata.ObjectType == nullId) return null;
            // connectionstring defined ? 
            if (string.IsNullOrWhiteSpace(schemaMetadata.Description))
            {
                // take connection pool from _metaSchema
                // but make a copy 
                return new ConnectionPool(5, 25, connectionRef?.CreateNewInstance(true, DatabaseProvider.PostgreSql, connectionRef.ConnectionString));
            }
            // generate new connectionPool 
            // return empty schema 
            // generate a connectioRef 

            // TODO: MANAGE MULTIPLE DATABASE ( only sqlite for the moment) + param of min/ max connection 
            var connectionPool = new ConnectionPool(5, 25, connectionRef?.CreateNewInstance(true, DatabaseProvider.Sqlite, schemaMetadata.Description));
            return connectionPool;
        }

        /// <summary>
        /// Count max number of lexicon
        /// </summary>
        private static int GetNumberOfLexicon(MetaData[] metalist)
        {
            if (metalist == null || metalist.Length == 0) return 0;
            var result = 0;
            var dicoTableId = new HashSet<string>();
            for (var i = 0; i < metalist.Length; ++i)
            {
                var meta = metalist[i];
                if (!meta.IsField()) continue;
                if (!meta.IsFieldMultilingual()) continue;
                if (!dicoTableId.Contains(meta.RefId))
                {
                    ++result;
                    dicoTableId.Add(meta.RefId);
                }
                ++result;
            }
            return result;
        }


        /// <summary>
        /// Get Default schema
        /// </summary>
        private TableSpace[] GetTableSpaces(MetaData[] metalist, int schemaId)
        {
            var result = new List<TableSpace>();
            for (var i = 0; i < metalist.Length; ++i)
                if (metalist[i].IsTablespace())
                    result.Add(_tableSpaceBuilder.GetInstance(metalist[i], schemaId));
            return result.ToArray();
        }
        /// <summary>
        /// Get Default schema
        /// </summary>
        private static string GetDefaultSearchPath(bool meta, DatabaseProvider provider)
        {
            switch (provider)
            {
                case DatabaseProvider.PostgreSql:
                case DatabaseProvider.MySql:
                    return meta ? Constants.DefaultSchema : null;
                case DatabaseProvider.Oracle:
                case DatabaseProvider.Sqlite:
                    return null;
            }
            return null;
        }

        #endregion

    }
}
