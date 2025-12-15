using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Logging;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Util.Builders;

internal sealed class LogEventBuilder
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private static readonly Dictionary<AlterQueryType, ResourceType[]> AlterOperationMapping = new()
        {{ AlterQueryType.CreateTable, new [] { ResourceType.CreateTableNotOk, ResourceType.CreateTableOk }}
    };
    private static readonly string DefaultExecutionTime = "0";
    private readonly ResourceHelper _resourceHelper = new();

    internal long? JobId { set; get; }
    internal int SchemaId { set; get; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal LogEvent GetError(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Error, args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal LogEvent GetWarning(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Warning, args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal LogEvent GetInfo(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Information, args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal LogEvent GetFatal(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Critical, args);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal LogEvent GetDebug(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Debug, args);

    internal string GetMessage(EventType eventType, int operationId, string operationType, string operationDescription)
    {
        switch (eventType)
        {
            case EventType.UnsupportedOperation: 
                    return string.Format(DefaultCulture, _resourceHelper.GetMessage(ResourceType.UnsuportedOperation),
                            operationType, operationDescription, operationId);
        }
        return string.Empty;
    }

    internal string GetMessage(AlterQuery query,EventType eventType, TimeSpan? executionTime=null)
    {
        switch (eventType)
        {
            case EventType.QueryPerformed:
                {
                    var displayMillisecond = executionTime.HasValue ? 
                        Math.Max(executionTime.Value.TotalMilliseconds, 1).ToString(DefaultCulture) : DefaultExecutionTime;
                    return string.Format(DefaultCulture, _resourceHelper.GetMessage(ResourceType.DdlException),
                        string.Format(DefaultCulture, GetOperationDescription(query,1), query.Table.PhysicalName), 
                        displayMillisecond);
                }
            case EventType.DdlException:
                return string.Format(DefaultCulture, _resourceHelper.GetMessage(ResourceType.DdlException), 
                    GetOperationDescription(query,0), query.Table.PhysicalName);
        }
        return string.Empty;
    }

    #region private methods

    private string GetOperationDescription(AlterQuery query, int statusId) 
        => AlterOperationMapping.ContainsKey(query.Type) ? _resourceHelper.GetMessage(AlterOperationMapping[query.Type][statusId]) 
            : string.Empty;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private LogEvent GetInstance(LogType logType, LogLevel level, params object?[] args)
    {
        var stackTrace = new StackTrace(true);
        var threadId = Environment.CurrentManagedThreadId;
        var callingFrame = stackTrace.GetFrame(2);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        var methodInfo = callingFrame?.GetMethod();
#pragma warning restore IL2026 
        var method = methodInfo?.Name;
        var callSite = methodInfo?.ReflectedType?.FullName;
        var lineNumber = callingFrame?.GetFileLineNumber();
        var message = GetMessage(logType);
        var description = GetDescription(logType,args);
        return new LogEvent();
    }
    private string? GetMessage(LogType logType) => _resourceHelper.GetMessage(logType);
    private string? GetDescription(LogType logType, params object?[] args)
        => args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, 
                _resourceHelper.GetDescription(logType) ?? string.Empty, args) :
                _resourceHelper.GetDescription(logType);

    #endregion 

}
