using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Models;
using System.Runtime.CompilerServices;
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

	internal int SchemaCount { get; private set; }
	internal int TableCount { get; private set; }
	internal int FieldCount { get; private set; }
	internal int UndefinedFieldTypeCount { get; private set; }
	internal int RelationCount { get; private set; }
	internal int IndexCount { get; private set; }
	internal int WrongParentCount { get; private set; }
	internal int TableSpaceCount { get; private set; }
	internal int LineCount { get; private set; }

	/// <summary>
	/// Ctor
	/// </summary>
	public DocumentBuilder(string filePath) => FilePath = filePath ?? string.Empty;

	/// <summary>
	///	 Compute the number of meta, before allocation + light validation of xml structure
	/// </summary>
	internal async ValueTask<int> GetMetaCountAsync(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, bool hasTimeZoneOffsetColumn, CancellationToken cancellationToken = default)
	{
		// Code size: 687 (0x2af)
		var readerSettings = new XmlReaderSettings
		{
			IgnoreWhitespace = true,
			CheckCharacters = false,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			Async = true,
			CloseInput = false // We manage disposal ourselves
		};
		
		// initialize
		SchemaCount = 0;
		TableCount = 0;
		FieldCount = 0;
		UndefinedFieldTypeCount = 0;
		RelationCount = 0;
		IndexCount = 0;
		WrongParentCount = 0;
		TableSpaceCount = 0;
		LineCount = 0;

		var fieldItem = template.GetTemplateItem(EntityType.Field);
		var fieldTypeAttribute = fieldItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var fieldCaseSensitiveAttribute = fieldItem?.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		var result = 0;
		var buffer = new string?[template.MaxDepth + 2];
		if (fieldTypeAttribute is null || fieldCaseSensitiveAttribute is null)
		{
			// throw exception !!!
			return 0;
		}
		buffer[0] = string.Empty;

		var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192, useAsync: true);
		try
		{
			using var xmlReader = XmlReader.Create(fs, readerSettings);

			while (await xmlReader.ReadAsync().ConfigureAwait(false))
			{
				// Check cancellation periodically, not every iteration for perf
				// Check every 256 iterations
				if ((result & 0xFF) == 0) cancellationToken.ThrowIfCancellationRequested();
				if (xmlReader.NodeType != XmlNodeType.Element) continue;

				var currentDepth = xmlReader.Depth;
				if (currentDepth > template.MaxDepth) continue;
				buffer[currentDepth + 1] = xmlReader.Name;

				if (tagDico.TryGetValue(xmlReader.Name, out var item))
				{
					var parent = buffer[currentDepth];
					if (StringComparer.Ordinal.Equals(item.ParentTag, parent))
					{
						switch (item.EntityType)
						{
							case EntityType.Schema: ++SchemaCount; break;
							case EntityType.Table: ++TableCount; break;
							case EntityType.Field:
								++FieldCount;
								(var fieldType, var searchableType) = GetFieldInfo(xmlReader, fieldTypeAttribute, fieldCaseSensitiveAttribute);
								if (searchableType != SearchableType.None) ++result; // extra column for searchable
								if (fieldType == FieldType.Undefined) ++UndefinedFieldTypeCount;
								if (fieldType == FieldType.DateTimeOffset && hasTimeZoneOffsetColumn) ++result; // extra field of date offset
								break;
							case EntityType.Relation: ++RelationCount; break;
							case EntityType.Index: ++IndexCount; break;
							case EntityType.Tablespace: ++TableSpaceCount; break;
						}
					}
					else ++WrongParentCount;
				}
			}
			result += TableCount + FieldCount + RelationCount + IndexCount + TableSpaceCount + SchemaCount;
			LineCount = (xmlReader as IXmlLineInfo)?.LineNumber ?? 0;
		}
		finally
		{
			await fs.DisposeAsync().ConfigureAwait(false);
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
				var metaCount = await GetMetaCountAsync(template, tagDico, true, cancellationToken).ConfigureAwait(false);

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (FieldType, SearchableType) GetFieldInfo(XmlReader reader, SchemaTemplateAttribute attributeType, SchemaTemplateAttribute attributeSearchable)
	{
		// Code size: 106 (0x6a)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var fieldType = FieldType.Undefined;
		var searchableType = SearchableType.None;
		var attributeTypeName = attributeType.Name;
		var attributeSearchableName = attributeSearchable.Name;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				if (string.Equals(attributeTypeName, reader.Name, comparison))
					fieldType = attributeType.GetFieldType(reader.Value);
				if (string.Equals(attributeSearchableName, reader.Name, comparison))
					searchableType = attributeSearchable.GetSearchableType(reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (fieldType, searchableType);
	}

	#endregion 

}
