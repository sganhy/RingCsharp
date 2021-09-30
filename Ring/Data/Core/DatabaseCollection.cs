using Ring.Data.Core.Extensions;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data.Core
{
    internal sealed class DatabaseCollection
    {
        private Database _defaultSchema;
        private Database _metaSchema;
        private Database _pendingSchema;
        private DatabaseCollectionStatus _status;
        private Table _metaDataTable;
        private Table _metaDataIdTable;
        private Table _lexiconTable;
        private Table _lexiconItemTable;
        private Table _logTable;
        private Table _userTable;
        private Database[] _schemaList; // schema list per id 
        private readonly Logger _log = new Logger(typeof(DatabaseCollection));
        private readonly SchemaBuilder _schemaBuilder = new SchemaBuilder();
        private readonly MetaDataBuilder _metaDataBuilder = new MetaDataBuilder();
        private readonly LexiconBuilder _lexiconBuilder = new LexiconBuilder();
        private long _upgradeJobId;

        /// <summary>
        /// Ctor
        /// </summary>
        public DatabaseCollection()
        {
            _defaultSchema = null;
            _metaSchema = null;
            _pendingSchema = null;
            _schemaList = new Database[0];
            _status = DatabaseCollectionStatus.NotReady;
            _upgradeJobId = 0L;
        }

        public long UpgradeJobId => _upgradeJobId;
        public int SchemaCount => _schemaList.Length;
        public Database DefaultSchema => _defaultSchema;
        public Database MetaSchema => _metaSchema;
        public Database PendingSchema => _pendingSchema;
        public Table MetaDataTable => _metaDataTable;
        public Table MetaDataIdTable => _metaDataIdTable;
        public Table LexiconTable => _lexiconTable;
        public Table UserTable => _userTable;
        public Table LogTable => _logTable;
        public DatabaseCollectionStatus Status => _status;

        /// <summary>
        /// Get type id from the Clarify object name (this method Thread safe)
        /// </summary>
        /// <param name="objectName">{schema name}.{Clarify object name eg. case, subcase, ...}</param>
        /// <returns>Return {schemaId (AH)}{TableId (AL)} </returns>
        public Table this[string objectName]
        {
            get
            {
                if (objectName == null) return null;
                var schema = _defaultSchema; // found here the rigth schema 
                //var databaseId = (long)_defaultSchema.DefaultPort << 32;
                var table = schema.GetTable(objectName);
                // TODO: manage multiple schema HERE 
                // ....
                // ....
                return table;
            }
        }

        /// <summary>
        /// Add a metadata schema from a database connection
        /// </summary>
        public void Add(IDbConnection connection)
        {
            if (_metaSchema != null) return; // load just once @meta and list of schema
            _status = DatabaseCollectionStatus.Loading;

            // create connection pool for meta database
            var connectionPool = new ConnectionPool(3, 10, connection);

            // generate metaData instance
            _metaSchema = _schemaBuilder.GetInstance(connectionPool);
            _defaultSchema = _metaSchema;
            _metaDataTable = _metaSchema.GetTable(TableType.Meta);
            _metaDataIdTable = _metaSchema.GetTable(TableType.MetaId);
            _lexiconTable = _metaSchema.GetTable(TableType.Lexicon);
            _lexiconItemTable = _metaSchema.GetTable(TableType.LexiconItem);
            _logTable = _metaSchema.GetTable(TableType.Log);
            _userTable = _metaSchema.GetTable(TableType.User);
            Add(_metaSchema);

            // create schema information_schema
            if (!_metaSchema.Exists()) _metaSchema.Create();

            // load global sequences
            Global.LoadGlobalSequences();

            var jobId = Global.SequenceJobId.NextValue(); //1

            // initialize logger (important just after _metaSchema init) + log
            _log.Info(jobId, 1, Constants.MsgLoggerInitialized);

            // load default schema 
            // TODO change to load multiple schema (just the default schema)
            var schemaList = GetSchemaList();
            for (var i = 0; i < schemaList.Count; ++i) LoadSchema(jobId, int.Parse(schemaList[i].GetField(Constants.MetaDataId)));
            _status = DatabaseCollectionStatus.Ready;

        }

        /// <summary>
        /// Add/ modify lexicons
        /// </summary>
        /// <param name="lexicons"></param>
        public void Add(Lexicon[] lexicons)
        {

        }

        /// <summary>
        /// Get schema by id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Database GetSchema(int id)
        {
            var schemaArr = _schemaList;
            int indexerLeft = 0, indexerRigth = schemaArr.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = id - schemaArr[indexerMiddle].Id;
                if (indexerCompare == 0) return schemaArr[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Get schema by name 
        /// </summary>
        public Database GetSchema(string name)
        {
            var schemaArr = _schemaList;
            for (var i = 0; i < schemaArr.Length; ++i)
                if (string.Equals(schemaArr[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return schemaArr[i];
            return null;
        }

        /// <summary>
        /// Change database collection status
        /// </summary>
        public void SetStatus(DatabaseCollectionStatus status) => _status = status;

        /// <summary>
        /// Set pending schema 
        /// </summary>
        public void SetPendingSchema(Database schema) => _pendingSchema = schema;

        /// <summary>
        /// Generate id for a new schema
        /// </summary>
        public int GetSchemaId()
        {
            var result = 0;
            for (var i = 0; i < _schemaList.Length; ++i)
                if (_schemaList[i].Id > result)
                    result = _schemaList[i].Id;
            return result + 1;
        }

        /// <summary>
        /// Load schema by port from Database 
        /// </summary>
        public void LoadSchema(long jobId, int schemaId)
        {
            try
            {
                var br = new BulkRetrieve { Schema = _metaSchema };
                br.SimpleQuery(0, _metaDataTable.Name);
                br.AppendFilter(0, Constants.MetaDataSchemaId, OperationType.Equal, schemaId);
                br.AppendFilter(0, Constants.MetaDataEnabled, OperationType.Equal, true);
                br.SimpleQuery(1, _metaDataIdTable.Name);
                br.AppendFilter(1, Constants.MetaDataSchemaId, OperationType.Equal, schemaId);
                br.RetrieveRecords();

                var resultSchema = br.GetRecordList(0);
                var resultSchemaId = br.GetRecordList(1);
                br.Dispose();

                if (resultSchema.Count <= 0) return;

                var metaDataList = _metaDataBuilder.GetInstances(resultSchema);
                var currentSchema = _schemaBuilder.GetInstance(DatabaseProvider.PostgreSql, metaDataList, SchemaLoadType.Full);
                //TODO create job if doesn't exist 

                //TODO: insert missing  @meta_id
                for (var i = 0; i < resultSchemaId.Count; ++i)
                {
                    int tableId;
                    resultSchemaId[i].GetField(Constants.MetaDataId, out tableId);
                    var table = currentSchema.GetTable(tableId); // get table by Id

                    //TODO manage warning useless @meta_id defined
                    if (table != null)
                    {
                        long currentId;
                        resultSchemaId[i].GetField(Constants.MetaDataValue, out currentId);
                        table.CacheId.CurrentId = currentId == 0 ? 1 : currentId;
                    }
                }
                LoadLexicons(currentSchema); // load lexicon first after add schema
                Add(currentSchema);
                _pendingSchema = null; // after upgrade 
                // log
                _log.Info(schemaId, jobId, 2, string.Format(Constants.MsgSchemaLoaded, currentSchema.Name),
                      string.Format(Constants.MsgSchemaLoadedDesc, metaDataList.Length.ToString()));

                currentSchema.WebServer.Start();
            }
            catch (Exception ex)
            {
                _log.Error(schemaId, 0L, ex);
            }
        }

        /// <summary>
        /// Generate id 
        /// </summary>
        /// <param name="jobType"></param>
        public long GenerateNewJobId(JobType jobType)
        {
            switch (jobType)
            {
                case JobType.Upgrade: return _upgradeJobId = Global.SequenceJobId.NextValue();
            }
            throw new NotImplementedException();
        }


        #region privates methods     

        /// <summary>
        /// Add Schema to Database collection
        /// </summary>
        private void Add(Database schema)
        {
            if (schema == null) return;

            // Is schema already there ? 
            if (GetSchema(schema.Id) == null)
            {
                // manage connection string ?? 
                var schemaArr = _schemaList;
                var newSchemaArr = new Database[schemaArr.Length + 1];

                // copy prev schema cache 
                for (var i = 0; i < schemaArr.Length; ++i) newSchemaArr[i] = schemaArr[i];

                // assign new array 
                newSchemaArr[schemaArr.Length] = schema;

                // sort by id 
                Array.Sort(newSchemaArr, (x, y) => x.Id - y.Id);

                // default schema management
                if (_metaSchema.Id == _defaultSchema.Id) _defaultSchema = schema;
                _schemaList = newSchemaArr;
            }
            else
            {
                // assign upgraded schema - no need to sort (already sorted)
                for (var i = 0; i < _schemaList.Length; ++i) if (_schemaList[i].Id == schema.Id) _schemaList[i] = schema;
                if (_defaultSchema?.Id == schema.Id) _defaultSchema = schema;
            }
        }

        /// <summary>
        /// Get the list of available schemas from Db 
        /// </summary>
        private List GetSchemaList()
        {
            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            br.SimpleQuery(0, _metaDataTable.Name);
            br.AppendFilter(0, Constants.MetaDataObjectType, OperationType.Equal, (int)EntityType.Schema);
            br.RetrieveRecords();
            return br.GetRecordList(0);
        }

        /// <summary>
        /// Load lexicons of a schema
        /// </summary>
        private void LoadLexicons(Database database)
        {
            var br = new BulkRetrieve { Schema = _metaSchema };
            br.SimpleQuery(0, _lexiconTable.Name);
            br.AppendFilter(0, Constants.MetaDataSchemaId, OperationType.Equal, database.Id);
            br.AppendFilter(0, Constants.MetaDataEnabled, OperationType.Equal, true);
            br.RetrieveRecords();
            var lexiconList = br.GetRecordList(0);

            for (var i = 0; i < lexiconList.Count; ++i)
            {
                br.Clear();
                var rcdLexicon = lexiconList[i];
                var lexiconId = rcdLexicon.GetField();

                br.SimpleQuery(0, _lexiconItemTable.Name);
                br.AppendFilter(0, Constants.LexiconItemLexiId, OperationType.Equal, lexiconId);
                br.RetrieveRecords();
                var lexiconitemList = br.GetRecordList(0);
                var lexicon = _lexiconBuilder.GetInstances(rcdLexicon, lexiconitemList);
                var lexiconIndex = -1;
                if (lexicon.ToField != null && lexicon.Table != null) lexiconIndex = lexicon.Table.GetLexiconIndex(lexicon.ToField.Id);
                if (lexicon.ToField == null && lexicon.Table != null) lexiconIndex = lexicon.Table.GetLexiconIndex();
                if (lexiconIndex >= 0 && lexiconIndex < database.Lexicons.Length) database.Lexicons[lexiconIndex] = lexicon;
            }
        }

        #endregion 

    }
}

