using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Models;

namespace Ring.Schema;

public sealed class DocumentBuilder
{
	public string FilePath { get; private set; }
	private int _schemaId = -1;
	private string? _creator;
	private DateTime? _creationTime;
	private DateTime? _updateTime;
	private Meta[] _result = Array.Empty<Meta>();
	private DocumentType _type = DocumentType.Undefined;
	private long _jobId = -1L;
	private DatabaseProvider _provider = DatabaseProvider.Undefined;
	private string _schemaName = string.Empty;
	private readonly List<Log> _logs = new();
	private readonly LogBuilder _logBuilder = new();

	/// <summary>
	/// Ctor
	/// </summary>
	public DocumentBuilder(string filePath) => FilePath = filePath ?? string.Empty;


	public async ValueTask<Document> GetDocumentAsync(DocumentType documentType, CancellationToken cancellationToken = default)
	{
		Reset(); // reset values
		if (File.Exists(FilePath))
		{
			// load template
			var validator = documentType.GetValidator();
			var template = documentType.GetSchemaTemplate();
			var metaBuilder = documentType.GetMetaBuilder();

			if (template is not null)
			{
				var stats = await validator.GetMetaCountAsync(FilePath, cancellationToken).ConfigureAwait(false);
				// validate stats here
				if (stats.MetaCount > 0)
				{
					var metaArray = await metaBuilder.GetMeta(FilePath, stats.MetaCount, cancellationToken).ConfigureAwait(false);
				}
			}
			else 
			{
				// unsupported document type
			}
		}
		else _logs.Add(_logBuilder.GetError(LogType.FileNotFound, FilePath));
		Document result=new(_schemaId, FilePath, _creator, _creationTime, _updateTime, _result, _type, _jobId, _provider, _schemaName);
		result.Logs.AddRange(_logs);
		return result;
	}

	#region private methods 

	private void Reset()
	{
		_schemaId = -1;
		_creator = null;
		_creationTime = null;
		_updateTime = null;
		_result = [];
		_type = DocumentType.Undefined;
		_jobId = 0L;
		_provider = DatabaseProvider.Undefined;
		_schemaName = string.Empty;
		_logs.Clear();
	}

	

	#endregion 

}
