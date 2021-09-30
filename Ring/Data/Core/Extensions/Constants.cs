using System;
using System.Globalization;

namespace Ring.Data.Core.Extensions
{
    internal static class Constants
    {

        internal static readonly string EmptyParameter = @"0";
        internal static readonly CultureInfo FloatFormat = CultureInfo.InvariantCulture;

        /// <summary>
        /// BulkRetrieveFilterExtension class constants
        /// </summary>

        // @log table 
        internal static readonly string MetaLogId = Schema.Builders.Constants.MetaLogId;
        internal static readonly string MetaSchemaId = Schema.Builders.Constants.MetaSchemaId;

        internal static readonly string MetaLogEntryTime = Schema.Builders.Constants.MetaLogEntryTime;
        internal static readonly string MetaLogLevel = Schema.Builders.Constants.MetaLogLevel;
        internal static readonly string MetaLogThreadId = Schema.Builders.Constants.MetaLogThreadId;
        internal static readonly string MetaLogJobId = Schema.Builders.Constants.MetaLogJobId;
        internal static readonly string MetaLogCallSite = Schema.Builders.Constants.MetaLogCallSite;
        internal static readonly string MetaLogLineNumber = Schema.Builders.Constants.MetaLogLineNumber;
        internal static readonly string MetaLogMethod = Schema.Builders.Constants.MetaLogMethod;
        internal static readonly string MetaLogMessage = Schema.Builders.Constants.MetaLogMessage;
        internal static readonly string MetaLogDescription = Schema.Builders.Constants.MetaLogDescription;
        internal static readonly TimeSpan DefaultExecutionTime = new TimeSpan();

        /// <summary>
        /// BulkRetrieveQueryExtension class constants
        /// </summary>
        internal static readonly List DefaultRecordList = new List();
        internal static readonly long[] DefaultDistinctRelationId = new long[0];


        /// <summary>
        /// DatabaseProviderExtension class constants
        /// </summary>

        /// <summary>
        /// DatabaseCollectionExtension class constants
        /// </summary>
        // copy 
        internal const int CommandLineTimeOut = Data.Constants.CommandLineTimeOut;


    }
}
