using Ring.Schema.Enums;
using Ring.Util.Models;

namespace Ring.Schema;

/// <summary>
///		This class provides a parsed document along with its associated validation report.
/// </summary>
public sealed class Document
{
	public int SchemaId { get; private set; }
	public string SchemaName { get; private set; }
	public string FilePath { get; private set; }
	public string? Creator { get; private set; }
	public DateTime? CreationTime { get; private set; }
	public DateTime? UpdateTime { get; private set; }
	public long JobId { get; private set; }
	public ValidationResult ValidationResult { get; private set; }
	public DocumentType Type { get; private set; }
	public int ResultCount => Result.Length;
	//
	internal readonly Meta[] Result;
	internal readonly DatabaseProvider Provider;
	//internal readonly List<Log> Logs;

	internal Document(int schemaId, string filePath, string? creator, DateTime? creationTime, DateTime? updateTime, Meta[] result, DocumentType type, long jobId, DatabaseProvider provider, string schemaName, ValidationResult validationResult)
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
		//Logs = [];
		ValidationResult = validationResult ?? new();
	}
}
