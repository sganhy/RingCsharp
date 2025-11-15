using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Xml;

namespace Ring.Schema.Builders;

internal sealed class NativeMetaBuilder : BaseMetaBuilder, IMetaBuilder
{
	private const int FileStreamBufferSize = 8192;
	private const int CancellationCheckMask = 0xFF; // Check every 256 iterations
	private int[] _lines = [];

	internal NativeMetaBuilder() : this(GetTemplate(DocumentType.XmlNative)) { }
	internal NativeMetaBuilder(DocumentType documentType) : this(GetTemplate(documentType)) { } // reuse same logic with another document type
	private NativeMetaBuilder(SchemaTemplate template) : base(template, template.ToTagDictionary(StringComparer.Ordinal), template.Type) { }


	public ValueTask<Meta[]> GetMetaAsync(string filePath, int count, CancellationToken cancellationToken = default)
	{
		return GetMeta(filePath, TagDictionary, Template, count, cancellationToken);
	}

	private async static ValueTask<Meta[]> GetMeta(string filePath, Dictionary<string, SchemaTemplateItem> tagDico, SchemaTemplate template, int count, CancellationToken cancellationToken = default)
	{
		var readerSettings = new XmlReaderSettings
		{
			IgnoreWhitespace = true,
			CheckCharacters = false,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			Async = true,
			CloseInput = false // We manage disposal ourselves
		};
		
		// schema variables
		var schemaId = 0;
		var templateItem = template.GetTemplateItem(EntityType.Schema);
		var schemaNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		// table variables
		templateItem = template.GetTemplateItem(EntityType.Table);
		var tableNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var tableIdAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Id)?.Name;
		var tableReadonlyAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.ReadOnly);
		var tableBaselineAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.BaseLine);
		var tableCachedAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Cached);
		// field variables
		var field = Meta.GetDefaultField(DefaultMetaField, FieldType.Boolean);
		var primaryKeyFieldName = field.GetPrimaryKeyName();
		templateItem = template.GetTemplateItem(EntityType.Field);
		var fieldTypeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var fieldNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var fieldCaseSensitiveAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		var fieldSizeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Size);
		var fieldDefaultValueAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.DefaultValue);
		var fieldBaselineAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.BaseLine);
		var fieldNotNullAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.NotNull);
		var fieldMultiLangualeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Multilingual);
		// relation variables
		templateItem = template.GetTemplateItem(EntityType.Relation);
		var relationNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var buffer = new string?[template.MaxDepth + 2];
		var iterationCount = 0;
		var metaIndex = 0;
		var fieldIndex = 0;
		var currentTableId = -1;

		if (schemaNameAttribute is null || tableNameAttribute is null || fieldNameAttribute is null || relationNameAttribute is null || tableIdAttribute is null || fieldTypeAttribute is null 
			|| fieldCaseSensitiveAttribute is null || tableReadonlyAttribute is null || tableBaselineAttribute is null || tableCachedAttribute is null ||
			fieldSizeAttribute is null || fieldDefaultValueAttribute is null || fieldBaselineAttribute is null || fieldNotNullAttribute is null || fieldMultiLangualeAttribute is null)
		{
			// throw exception !!!
			return [];
		}
		buffer[0] = string.Empty;

		// allocate array
		var result = new Meta[count];
		var lines = new int[count];

		var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: FileStreamBufferSize, useAsync: true);
		await using (fs.ConfigureAwait(false))
		{
			using var xmlReader = XmlReader.Create(fs, readerSettings);

			while (await xmlReader.ReadAsync().ConfigureAwait(false))
			{
				// Check cancellation periodically, not every iteration for perf - Check every 256 iterations
				if ((++iterationCount & CancellationCheckMask) == 0) cancellationToken.ThrowIfCancellationRequested();
				if (xmlReader.NodeType != XmlNodeType.Element) continue;
				if (metaIndex >= count) break; 

				var currentDepth = xmlReader.Depth;
				var elementName = xmlReader.Name;
				if (currentDepth > template.MaxDepth) continue; // don't read below max depth (skipped)
				buffer[currentDepth + 1] = elementName;

				if (!tagDico.TryGetValue(elementName, out var item)) continue;
				var parent = buffer[currentDepth];
				if (!StringComparer.Ordinal.Equals(item.ParentTag, parent) && !AllParent.Equals(item.ParentTag, StringComparison.OrdinalIgnoreCase)) continue;
				
				// Process element
				switch (item.EntityType)
				{
					case EntityType.Schema: 
						result[metaIndex] =	new(0, SchemaId, 0, 0, 0L, GetAttributeValue(xmlReader, schemaNameAttribute), string.Empty, null, true);
						break;
					case EntityType.Table:
						{
							currentTableId = GetId(xmlReader, tableIdAttribute);
							var (readonlyTable, baseline, cachedTable) = xmlReader.GetTableInfo(tableReadonlyAttribute, tableBaselineAttribute, tableCachedAttribute);
							result[metaIndex] = ToTable(currentTableId, GetAttributeValue(xmlReader, tableNameAttribute), null, null, schemaId, TableType.Business, baseline, false, 
								readonlyTable, cachedTable);
							fieldIndex=1; 
						}
						break;
					case EntityType.Field:
						{
							var (fieldType, searchableType) = xmlReader.GetFieldInfo(fieldTypeAttribute, fieldCaseSensitiveAttribute);
							var (size, defaultValue, baseline, notNull, multiLangual) = 
								xmlReader.GetFieldInfo(fieldSizeAttribute, fieldDefaultValueAttribute, fieldBaselineAttribute, fieldNotNullAttribute, fieldMultiLangualeAttribute);
							var fieldName = GetAttributeValue(xmlReader, fieldNameAttribute);
							var fieldId = primaryKeyFieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) ? 0 : fieldIndex++;
							result[metaIndex] = ToField(fieldId, fieldName, null, fieldType, size, defaultValue, searchableType, currentTableId, baseline, notNull, multiLangual, true);
							if (searchableType != SearchableType.None)
							{
								++metaIndex;
								result[metaIndex] = ToSearchableColumn(metaIndex, fieldName, fieldType, size, defaultValue, searchableType, currentTableId, baseline, notNull);
							}
						}
						break;
					case EntityType.Relation: 
						result[metaIndex] = new(0, RelationId, currentTableId, 0, 0L, GetAttributeValue(xmlReader, relationNameAttribute), string.Empty, null, true);
						break;
					case EntityType.Index: break;
					case EntityType.Tablespace: break;
					case EntityType.Comment:
						{
							var comment = xmlReader.ReadString();
							if (!string.IsNullOrWhiteSpace(comment) && metaIndex > 0) 
								result[metaIndex - 1] = SetDescription(ref result[metaIndex - 1], comment);
						}
;						break;
				}
				if (item.EntityType != EntityType.Undefined && item.EntityType != EntityType.Comment) ++metaIndex;
			}
		}
		return result;
	}

}
