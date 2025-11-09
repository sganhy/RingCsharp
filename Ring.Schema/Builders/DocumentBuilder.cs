using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Models;
using System.Xml;
using Document = Ring.Schema.Models.Document;
using ResourceHelper = Ring.Schema.Helpers.ResourceHelper;

namespace Ring.Schema.Builders;

public sealed class DocumentBuilder
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

    internal int TableCount { get; private set; }
    internal int FieldCount { get; private set; }
    internal int RelationCount { get; private set; }
    internal int IndexCount { get; private set; }

    /// <summary>
    /// Ctor
    /// </summary>
    public DocumentBuilder(string filePath) => FilePath = filePath ?? string.Empty;

    internal int GetMetaCount(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, CancellationToken cancellationToken = default)
    {
        // Code size: 375 (0x177)
        var readerSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            CheckCharacters = false,
            IgnoreComments = true,
            Async = false,  // Synchronous
        };

        TableCount = 0;
        FieldCount = 0;
        RelationCount = 0;
        IndexCount = 0;
        var fieldItem = template.GetTemplateItem(EntityType.Field);
        var fieldTypeAttribute = fieldItem.GetAttribute(SchemaTemplateAttributeType.Type);
        var fieldSearchableAttribute = fieldItem.GetAttribute(SchemaTemplateAttributeType.CaseSensitive).Name;
        var result = 0;
        var buffer = new string?[template.MaxDepth + 2];
        buffer[0] = string.Empty;

        using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var xmlReader = XmlReader.Create(fs, readerSettings);

        while (xmlReader.Read())  // Synchronous read
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                var currentDepth = xmlReader.Depth;
                if (currentDepth > template.MaxDepth) continue;

                buffer[currentDepth + 1] = xmlReader.Name;

                if (tagDico.TryGetValue(xmlReader.Name, out var item))
                {
                    var parent = buffer[currentDepth];
                    if (StringComparer.Ordinal.Equals(item.ParentTag, parent))
                    {
                        ++result;
                        switch (item.EntityType)
                        {
                            case EntityType.Table: ++TableCount; break;
                            case EntityType.Field: 
                                ++FieldCount;
                                (var fieldType, var searchableType) = GetFieldInfo(template, xmlReader, fieldTypeAttribute.Name, fieldSearchableAttribute); 
                                break;
                            case EntityType.Relation: ++RelationCount; break;
                            case EntityType.Index: ++IndexCount; break;
                        }
                    }
                }
            }
        }

        return result;
    }

    public async Task<Document> GetDocumentAsync(DocumentType documentType, CancellationToken cancellationToken = default)
    {
        Reset(); // reset values
        if (File.Exists(FilePath))
        {
            // load template
            var template = ResourceHelper.GetSchemaTemplate(documentType);
            if (template is not null)
            {
                var tagDico = template.ToTagDictionary(StringComparer.Ordinal);
                var metaCount = GetMetaCount(template, tagDico, cancellationToken);
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

    private static (FieldType, SearchableType) GetFieldInfo(SchemaTemplate template, XmlReader reader, string attributeType, string attributeSearchable)
    {
        var fieldType = FieldType.Undefined;
        var searchableType = SearchableType.None;
        //var fieldType = template.GetFieldType("long");
		var attInd = 0;
		while (attInd < reader.AttributeCount)
		{
            reader.MoveToNextAttribute();
            if (string.Equals(attributeType, reader.Name, StringComparison.OrdinalIgnoreCase))
            {
                                                
            }
            attInd++;
		}
		return (fieldType, searchableType);
    }

    #endregion 

}
