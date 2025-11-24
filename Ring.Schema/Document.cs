using Ring.Schema.Enums;
using Ring.Util.Models;

namespace Ring.Schema;

/// <summary>
///		Result of document parsing (sources: xml, json, ...)
/// </summary>
public sealed class Document
{
	public int SchemaId { get; private set; }
	public string FilePath { get; private set; }
	public string? Creator { get; private set; }
	public DateTime? CreationTime { get; private set; }
	public DateTime? UpdateTime { get; private set; }
	public long JobId { get; private set; }
	public string SchemaName { get; private set; }
	internal readonly ValidationResult ValidationResult;
	internal readonly Meta[] Result;
	internal readonly DocumentType Type;
	internal readonly DatabaseProvider Provider;
	internal readonly List<Log> Logs;

	internal Document(int schemaId, string filePath, string? creator, DateTime? creationTime, DateTime? updateTime, Meta[] result, DocumentType type,
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
		Logs = [];
		ValidationResult = new();
	}
}
