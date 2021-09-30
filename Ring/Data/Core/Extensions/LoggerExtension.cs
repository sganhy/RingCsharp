using Ring.Data.Enums;
using Ring.Data.Helpers;
using Ring.Data.Models;
using Ring.Schema.Enums;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ring.Data.Core.Extensions
{
    internal static class LoggerExtension
    {

        /// <summary>
        /// Log via BulkSave
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(this Logger logger, short id, string message, string description, [CallerLineNumber] int line = 0) =>
            Log(null, null, logger, LogLevel.Error, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(this Logger logger, int schemaId, short id, string message, string description, [CallerLineNumber] int line = 0) =>
            Log(null, schemaId, logger, LogLevel.Error, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Info(this Logger logger, short id, string message, string description = null, [CallerLineNumber] int line = 0) =>
            Log(null, null, logger, LogLevel.Info, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Info(this Logger logger, int schemaId, short id, string message, string description = null, [CallerLineNumber] int line = 0) =>
            Log(null, schemaId, logger, LogLevel.Info, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Info(this Logger logger, int schemaId, long jobId, short id, string message, string description = null, [CallerLineNumber] int line = 0) =>
            Log(jobId, schemaId, logger, LogLevel.Info, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Info(this Logger logger, long jobId, short id, string message, string description = null, [CallerLineNumber] int line = 0) =>
            Log(jobId, null, logger, LogLevel.Info, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Warn(this Logger logger, long jobId, short id, string message, string description = null, [CallerLineNumber] int line = 0) =>
            Log(jobId, null, logger, LogLevel.Info, id, message, description, null, line);


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Warn(this Logger logger, int schemaId, long jobId, short id, string message, string description = null, [CallerLineNumber] int line = 0) =>
            Log(jobId, schemaId, logger, LogLevel.Info, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Warn(this Logger logger, short id, string message, string description, [CallerLineNumber] int line = 0) =>
            Log(null, null, logger, LogLevel.Warning, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Fatal(this Logger logger, short id, string message, string description, [CallerLineNumber] int line = 0) =>
            Log(null, null, logger, LogLevel.Fatal, id, message, description, null, line);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(this Logger logger, Exception ex) =>
            Log(null, null, logger, LogLevel.Error, short.MinValue, ex.Message, ex.StackTrace, ex.TargetSite?.Name, GetLineNumber(ex));

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(this Logger logger, int schemaId, Exception ex) =>
            Log(null, schemaId, logger, LogLevel.Error, short.MinValue, ex.Message, ex.StackTrace, ex.TargetSite?.Name, GetLineNumber(ex));

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(this Logger logger, int schemaId, long jobId, Exception ex) =>
            Log(jobId, schemaId, logger, LogLevel.Error, short.MinValue, ex.Message, ex.StackTrace, ex.TargetSite?.Name, GetLineNumber(ex));

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Fatal(this Logger logger, Exception ex) =>
            Log(null, null, logger, LogLevel.Fatal, short.MinValue, ex.Message, ex.StackTrace, ex.TargetSite?.Name, GetLineNumber(ex));

        /// <summary>
        /// 
        /// </summary>
        private static int GetLineNumber(Exception ex)
        {
            var st = new StackTrace(ex, true);
            var frame = st.GetFrame(0);
            return frame.GetFileLineNumber();
        }

        /// <summary>
        /// without connection error
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Log(long? jobId, int? schemaId, Logger logger, LogLevel logLevel, short id, string message, string description, string methodName, int lineNumber)
        {
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get();

            // check if db connection is available
            try
            {
                // ligth version of Bulksave
                var method = methodName;
                if (methodName == null)
                {
                    var stackTrace = new StackTrace();
                    method = stackTrace.FrameCount > 2 ? stackTrace.GetFrame(2).GetMethod().Name : string.Empty;
                }
                var rcd = GetRecord(schemaId, jobId, logger.Type, id, logLevel, message, description, method, lineNumber);
                var query = new BulkSaveQuery(null, BulkSaveType.InsertRecord, rcd, rcd, null);
                var sql = SqlHelper.GetQuery(connection, query);
                sql.Connection = connection;
                sql.CommandTimeout = Constants.CommandLineTimeOut;
                sql.ExecuteNonQuery();
                sql.Dispose();
            }
            finally
            {
                // return connection immediatly
                schema.Connections.Put(connection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static Record GetRecord(int? schemaId, long? jobId, Type type, short id, LogLevel logLevel, string message, string description, string method, int lineNumber)
        {
            var result = new Record(Global.Databases.LogTable);
            result.SetField(Constants.MetaSchemaId, schemaId?.ToString());
            result.SetField(Constants.MetaLogId, id);
            result.SetField(Constants.MetaLogEntryTime, DateTime.Now);
            result.SetField(Constants.MetaLogLevel, (short)logLevel);
            result.SetField(Constants.MetaLogThreadId, Thread.CurrentThread.ManagedThreadId);
            result.SetField(Constants.MetaLogCallSite, type.FullName);
            result.SetField(Constants.MetaLogJobId, jobId?.ToString());
            result.SetField(Constants.MetaLogMethod, method);
            result.SetField(Constants.MetaLogMessage, message);
            result.SetField(Constants.MetaLogLineNumber, lineNumber);
            result.SetField(Constants.MetaLogDescription, description);
            return result;
        }

    }
}
