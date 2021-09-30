namespace Ring.Data.Core
{
    internal static class Constants
    {
        internal const int DefaultMetaSchemaId = Schema.Builders.Constants.DefaultMetaSchemaId;

        /// <summary>
        /// Connection pool 
        /// </summary>
        internal static readonly string ArgObjectPoolExceptionName = Util.Constants.ArgObjectPoolExceptionName;
        internal static readonly string ArgObjectPoolExceptionMsg = Util.Constants.ArgObjectPoolExceptionMsg;
        internal static readonly string MetaDataEnabled = Schema.Builders.Constants.MetaDataActive;
        internal const int BaseConnectionId = 10000;
        internal const int DefaultPoolId = 0;

        // Default value
        internal static readonly string MetaDataSchemaId = Schema.Builders.Constants.MetaDataSchemaId;
        internal static readonly string MetaDataId = Schema.Builders.Constants.MetaDataId;
        internal static readonly string MetaDataValue = Schema.Builders.Constants.MetaDataValue;
        internal static readonly string MetaDataObjectType = Schema.Builders.Constants.MetaDataObjectType;
        internal static readonly string LexiconItemLexiId = Schema.Builders.Constants.LexiconItemLexiId;

        private static readonly string SqlitePragma = "PRAGMA ";
        private static readonly string SqlitePragmaCaseSensitif = @"case_sensitive_like=ON";
        private static readonly string SqlitePragmaJournalMode = @"journal_mode=WAL";

        // database collection - message
        internal static readonly string MsgLoggerInitialized = "Baseline Logger Initialized";
        internal static readonly string MsgSchemaLoaded = "Schema {0} loaded from @meta";
        internal static readonly string MsgSchemaLoadedDesc = "{0} Items loaded";

        internal static readonly string[] SqlitePragmaList =
        {
            SqlitePragma + SqlitePragmaCaseSensitif,
            SqlitePragma + SqlitePragmaJournalMode
        };

    }
}
