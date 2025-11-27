using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Globalization;
using System.Xml;

namespace Ring.Schema;

public sealed class DocumentBuilder
{
	public string FilePath { get; private set; }
	private readonly static long MaxFileSize = 104857600L;
	private int _schemaId = -1;
	private string? _creator;
	private DateTime? _creationTime;
	private DateTime? _updateTime;
	private DocumentType _type = DocumentType.Undefined;
	private long _jobId = -1L;
	private DatabaseProvider _provider = DatabaseProvider.Undefined;
	private string _schemaName = string.Empty;

	/// <summary>
	/// Ctor
	/// </summary>
	public DocumentBuilder(string filePath) => FilePath = filePath ?? string.Empty;

	public async ValueTask<Document> GetDocumentAsync(DocumentType documentType, CancellationToken cancellationToken = default)
	{
		Reset(); // reset values
		var validationResult = new ValidationResult();
		var metaArray = Array.Empty<Meta>();

		try
		{
			if (File.Exists(FilePath))
			{
				// check file size (max 100mb)
				var length = new FileInfo(FilePath).Length;
				if (length > MaxFileSize)
				{
					validationResult.AddItem(LogType.FileToolLarge, MaxFileSize.ToString(CultureInfo.InvariantCulture));
					return CreateDocument(metaArray, validationResult);
				}

				// load template
				var validator = documentType.GetValidator();
				var template = documentType.GetSchemaTemplate();
				var metaBuilder = documentType.GetMetaBuilder();

				if (template is not null)
				{
					var stats = await validator.GetMetaCountAsync(FilePath, cancellationToken).ConfigureAwait(false);
					// validate stats here
					if (stats.MetaCount > 0 && stats.ErrorCount == 0)
					{
						metaArray = await metaBuilder.GetMetaAsync(FilePath, stats.MetaCount, validator.ReferenceTables, cancellationToken).ConfigureAwait(false);
					}
				}
				else
				{
					// unsupported document type
				}
			}
			else validationResult.AddItem(LogType.FileNotFound, FilePath);
		}
		catch (XmlException ex)	{ validationResult.AddError(LogType.XmlException, ex.GetType().Name, ex.Message); }
		catch (OperationCanceledException ex) { validationResult.AddError(LogType.OperationCanceledException, ex.GetType().Name, ex.Message); }

		return CreateDocument(metaArray, validationResult);
	}

	#region private methods 

	private void Reset()
	{
		_schemaId = -1;
		_creator = null;
		_creationTime = null;
		_updateTime = null;
		_type = DocumentType.Undefined;
		_jobId = 0L;
		_provider = DatabaseProvider.Undefined;
		_schemaName = string.Empty;
	}

	private Document CreateDocument(Meta[] metaArray, ValidationResult validationResult) => 
		new(_schemaId, FilePath, _creator, _creationTime, _updateTime, metaArray, _type, _jobId, _provider, _schemaName, validationResult);

	#endregion

}
