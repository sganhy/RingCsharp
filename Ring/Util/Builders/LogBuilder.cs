using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using Ring.Util.Models;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Util.Builders;

internal sealed class LogBuilder
{
    private readonly ResourceHelper _resourceHelper = new ();
    internal long? JobId { set; get; }
    internal int SchemaId { set; get; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal Log GetError(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Error, args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal Log GetWarning(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Warning, args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal Log GetInfo(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Info, args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal Log GetFatal(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Fatal, args);
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal Log GetDebug(LogType logType, params object?[] args) => GetInstance(logType, LogLevel.Debug, args);

    #region private methods
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Log GetInstance(LogType logType, LogLevel level, params object?[] args)
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
        return new((int)logType, DateTime.UtcNow, level, SchemaId, threadId, callSite.Truncate(255), 
                        JobId, method.Truncate(80), lineNumber, message.Truncate(255), description);
    }
    private string? GetMessage(LogType logType) => _resourceHelper.GetMessage(logType);
    private string? GetDescription(LogType logType, params object?[] args)
        => args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, 
                _resourceHelper.GetDescription(logType) ?? string.Empty, args) :
                _resourceHelper.GetDescription(logType);

    #endregion 

}
