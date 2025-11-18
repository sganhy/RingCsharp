using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Xml;
using Ring.Util.Extensions;

namespace Ring.Schema.Validators;

internal sealed class NativeDocumentValidator : BaseDocumentValidator, IDocumentValidator
{
	private const int FileStreamBufferSize = 8192;
	private const int CancellationCheckMask = 0xFF; // Check every 256 iterations
	private Dictionary<string, int> _tableDictionary = [];

	internal NativeDocumentValidator() : this(GetTemplate(DocumentType.XmlNative)) {}
	internal NativeDocumentValidator(DocumentType documentType) : this(GetTemplate(documentType)) { }
	private NativeDocumentValidator(SchemaTemplate template) : base(template, template.ToTagDictionary(StringComparer.Ordinal), template.Type)	{}

	public Dictionary<string, int> ReferenceTables => _tableDictionary;

	public ValueTask<DocumentStats> GetMetaCountAsync(string filePath, CancellationToken cancellationToken = default) 
		=> GetMetaCountAsync(filePath, TagDictionary, true, Template, cancellationToken);

	/// <summary>
	///	 Compute the number of meta, before allocation + light validation of xml structure
	/// </summary>
	private async ValueTask<DocumentStats> GetMetaCountAsync(string filePath, Dictionary<string, SchemaTemplateItem> tagDico, bool hasTimeZoneOffsetColumn, SchemaTemplate template, 
		CancellationToken cancellationToken = default)
    {
		
		var readerSettings = new XmlReaderSettings
		{
			IgnoreWhitespace = true,
			CheckCharacters = false,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			Async = true,
		};

		// initialize
		ResetStats();
		_tableDictionary = [];
		var fieldItem = template.GetTemplateItem(EntityType.Field);
		var fieldTypeAttribute = fieldItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var fieldCaseSensitiveAttribute = fieldItem?.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		var tableItem = template.GetTemplateItem(EntityType.Table);
		var tableIdAttribute = tableItem?.GetAttribute(SchemaTemplateAttributeType.Id);
		var tableNameAttribute = tableItem?.GetAttribute(SchemaTemplateAttributeType.Name);
		var extraFieldCount = 0;
		var buffer = new string?[template.MaxDepth + 2];
		var iterationCount = 0;
		var metaCount = 0;
		int id;
		string name;

		if (fieldTypeAttribute is null || fieldCaseSensitiveAttribute is null || tableIdAttribute is null || tableNameAttribute is null)
		{
			// throw exception !!!
			return new DocumentStats(SchemaCount, TableCount, FieldCount, UndefinedFieldTypeCount, RelationCount, IndexCount, WrongParentCount, TableSpaceCount, LineCount, metaCount);
		}

		buffer[0] = string.Empty;

		var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: FileStreamBufferSize, useAsync: true);
		await using (fs.ConfigureAwait(false))
		{
			using var xmlReader = XmlReader.Create(fs, readerSettings);

			while (await xmlReader.ReadAsync().ConfigureAwait(false))
			{
				// Check cancellation periodically, not every iteration for perf
				// Check every 256 iterations
				if ((++iterationCount & CancellationCheckMask) == 0) cancellationToken.ThrowIfCancellationRequested();
				if (xmlReader.NodeType != XmlNodeType.Element) continue;

				var elementName = xmlReader.Name;
				var currentDepth = xmlReader.Depth;
				if (currentDepth > template.MaxDepth) continue;
				buffer[currentDepth + 1] = elementName;

				if (tagDico.TryGetValue(elementName, out var item))
				{
					var parent = buffer[currentDepth];
					if (StringComparer.Ordinal.Equals(item.ParentTag, parent))
					{
						switch (item.EntityType)
						{
							case EntityType.Schema: ++SchemaCount; break;
							case EntityType.Table: 
								++TableCount;
								id = xmlReader.GetId(tableIdAttribute.Name);
								name = xmlReader.GetAttributeValue(tableNameAttribute.Name).ToUpperInvariant();
								if (!_tableDictionary.ContainsKey(name)) _tableDictionary.Add(name, id);
								break;
							case EntityType.Field:
								++FieldCount;
								var (fieldType, searchableType) = GetFieldInfo(xmlReader, fieldTypeAttribute, fieldCaseSensitiveAttribute);
								if (searchableType != SearchableType.None) ++extraFieldCount; // extra column for searchable
								if (fieldType == FieldType.Undefined) ++UndefinedFieldTypeCount;
								if (fieldType == FieldType.DateTimeOffset && hasTimeZoneOffsetColumn) ++extraFieldCount; // extra field of date offset
								break;
							case EntityType.Relation: ++RelationCount; break;
							case EntityType.Index: ++IndexCount; break;
							case EntityType.Tablespace: ++TableSpaceCount; break;
						}
					}
					else ++WrongParentCount;
				}
			}
			LineCount = (xmlReader as IXmlLineInfo)?.LineNumber ?? 0;
			metaCount = extraFieldCount + TableCount + FieldCount + RelationCount + IndexCount + TableSpaceCount + SchemaCount;
		}

		return new DocumentStats(SchemaCount, TableCount, FieldCount, UndefinedFieldTypeCount, RelationCount, IndexCount, WrongParentCount, TableSpaceCount, LineCount, metaCount);
	}

}
