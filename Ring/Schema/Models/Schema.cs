using Ring.Data;
using Ring.Data.Core;
using Ring.Schema.Core.Rules.Impl;
using Ring.Schema.Enums;
using Ring.Web;
using System;

namespace Ring.Schema.Models
{
    internal sealed class Schema : BaseEntity
    {
        public readonly ConnectionPool Connections;
        public readonly string ConnectionString;
        public readonly Language DefaultLanguage;
        public readonly int DefaultPort;      // listening port 
        public readonly DatabaseProvider Driver;
        public readonly ValidationResult Feedback;
        public readonly bool IsMetaSchema;    // listening port 
        public readonly Lexicon[] Lexicons;   // sorted table by name (case sensitif)
        public readonly DateTime LoadingTime; // the time at which it is loaded.
        public readonly SchemaLoadType LoadType;
        public readonly Table[] MtmTables;    // sorted table by id
        public readonly Sequence[] Sequences; // sorted sequence by name (case sensitif)
        public readonly SchemaSourceType Source;
        public readonly Table[] TablesById;   // sorted table by id
        public readonly Table[] TablesByName; // sorted table by name (case sensitif)
        public readonly TableSpace[] TableSpaces;
        public readonly string Version;
        public readonly WebServer WebServer;
        public readonly string SearchPath;

        /// <summary>
        ///     Ctor
        /// </summary>
        internal Schema(int id, string name, string description, bool active, bool baseline, string version,
            string connectionString, int defaultport, bool metaSchema, Language defaultLanguage, DatabaseProvider driver,
            Table[] tableByName, Table[] tableById, Table[] mtm, Sequence[] sequences, Lexicon[] lexicons,
            ConnectionPool connectionPool, SchemaSourceType source, SchemaLoadType loadType, ValidationResult feedback,
            WebServer webServer, DateTime loadingTime, TableSpace[] tableSpaces, string searchPath)
            : base(id, name, description, active, baseline)
        {
            TablesByName = tableByName;
            TablesById = tableById;
            MtmTables = mtm;
            Version = version;
            DefaultLanguage = defaultLanguage;
            ConnectionString = connectionString;
            Driver = driver;
            Connections = connectionPool;
            Feedback = feedback;
            Lexicons = lexicons;
            DefaultPort = defaultport;
            IsMetaSchema = metaSchema;
            Source = source;
            LoadType = loadType;
            WebServer = webServer;
            Sequences = sequences;
            LoadingTime = loadingTime;
            TableSpaces = tableSpaces;
            SearchPath = searchPath;
        }

    }
}