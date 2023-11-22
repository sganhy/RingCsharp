using Ring.Util.Enums;

namespace Ring.Util.Models;

internal sealed class Log
{
    internal readonly int Id;
    internal readonly DateTime EntryTime;
    internal readonly LogLevel Level;
    internal readonly int SchemaId;
    internal readonly int? ThreadId;
    internal readonly string? CallSite;
    internal readonly long? JobId;
    internal readonly string? Method;
    internal readonly int? LineNumber;
    internal readonly string? Message;
    internal readonly string? Description;

    internal Log(int id, DateTime entryTime, LogLevel level, int schemaId, int? threadId, string? callSite, long? jobId, string? method, int? lineNumber, string? message, string? description)
    {
        Id = id;
        EntryTime = entryTime;
        Level = level;
        SchemaId = schemaId;
        ThreadId = threadId;
        CallSite = callSite;
        JobId = jobId;
        Method = method;
        LineNumber = lineNumber;
        Message = message;
        Description = description;
    }
}
