using Microsoft.VisualBasic;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using Ring.Util.Models;
using System.Reflection.Metadata;
using Document = Ring.Schema.Models.Document;
using System.Xml;

namespace Ring.Schema.Builders;

internal sealed class DocumentBuilder
{
    internal string FilePath { get; set; }
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

    internal ValueTask<int> GetMetaCountAsync(XmlSchemaTemplate template, CancellationToken cancellationToken = default)
    {
        // Code size: 64 (0x40)
        var readerSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            CheckCharacters = false,
            IgnoreComments = true,
            Async = true
        };

        return Core();

        async ValueTask<int> Core()
        {
            var result = 0;
            try
            {
                using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var xmlReader = XmlReader.Create(fs, readerSettings);

                while (await xmlReader.ReadAsync().ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {

                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return 0;
            }

            return result;
        }
    }

    internal async Task<Document> GetDocumentAsync(DocumentType documentType, CancellationToken cancellationToken = default)
    {
        Reset(); // reset values
        if (File.Exists(FilePath))
        {
            // load template
            var template = ResourceHelper.GetSchemaTemplate(documentType);
            if (template != null)
            {
                var metaCount = await GetMetaCountAsync(template, cancellationToken).ConfigureAwait(false);
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
        _result = Array.Empty<Meta>();
        _type = DocumentType.Undefined;
        _jobId = 0L;
        _provider = DatabaseProvider.Undefined;
        _schemaName = string.Empty;
        _logs.Clear();
    }

    #endregion 

}
