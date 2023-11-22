using Ring.Schema.Enums;
using Ring.Util.Models;

namespace Ring.Schema.Models;

/// <summary>
/// Result of document parsing (sources: xml, json, ...)
/// </summary>
internal sealed class Document
{
    internal readonly int SchemaId;
    internal readonly string FilePath;
    internal readonly string? Creator;
    internal readonly DateTime? CreationTime;
    internal readonly DateTime? UpdateTime;
    internal readonly Meta[] Result;
    internal readonly DocumentType Type;
    internal readonly long JobId;
    internal readonly DatabaseProvider Provider;
    internal readonly string SchemaName;
    internal readonly List<Log> Logs;

    public Document(int schemaId, string filePath, string? creator, DateTime? creationTime, DateTime? updateTime, Meta[] result, DocumentType type, 
        long jobId, DatabaseProvider provider, string schemaName)
    {
        SchemaId = schemaId;
        FilePath = filePath;
        Creator = creator;
        CreationTime = creationTime;
        UpdateTime = updateTime;
        Result = result;
        Type = type;
        JobId = jobId;
        Provider = provider;
        SchemaName = schemaName;
        Logs = new List<Log>();
    }
}
