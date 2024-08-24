using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Models;

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
    private readonly List<Log> _logs = new List<Log>();
    private readonly LogBuilder _logBuilder = new LogBuilder();

    /// <summary>
    /// Ctor
    /// </summary>
    public DocumentBuilder(string filePath) => FilePath = filePath ?? string.Empty;
        
    internal Document GetDocument()
    {
        Reset(); // reset values
        if (File.Exists(FilePath))
        {

        }
        else _logs.Add(_logBuilder.GetError(LogType.FileNotFound, FilePath));
        var result=new Document(_schemaId, FilePath, _creator, _creationTime, _updateTime, _result, _type, _jobId, _provider, _schemaName);
        result.Logs.AddRange(_logs);
        return result;
    }

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
}
