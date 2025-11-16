using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Data;
using System.Xml;

namespace Ring.Schema.Builders;

internal sealed class NativeMetaBuilder : BaseMetaBuilder, IMetaBuilder
{
	private const int FileStreamBufferSize = 8192;
	private const int CancellationCheckMask = 0xFF; // Check every 256 iterations

	internal NativeMetaBuilder() : this(GetTemplate(DocumentType.XmlNative)) { }
	internal NativeMetaBuilder(DocumentType documentType) : this(GetTemplate(documentType)) { } // reuse same logic with another document type
	private NativeMetaBuilder(SchemaTemplate template) : base(template, template.ToTagDictionary(StringComparer.Ordinal), template.Type) { }


	public ValueTask<Meta[]> GetMetaAsync(string filePath, int count, Dictionary<string, int> referenceTable, CancellationToken cancellationToken = default)
	{
		return GetMetaAsync(filePath, TagDictionary, referenceTable, Template, count, cancellationToken);
	}

	private async static ValueTask<Meta[]> GetMetaAsync(string filePath, Dictionary<string, SchemaTemplateItem> tagDico, Dictionary<string, int> referenceTables, SchemaTemplate template, 
		int count, CancellationToken cancellationToken = default)
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
		var tableIdAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Id)?.Name;
		var tableNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var tableReadonlyAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.ReadOnly) ?? DefaultTemplateAttribute;
		var tableBaselineAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.BaseLine);
		var tableCachedAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Cached) ?? DefaultTemplateAttribute;
		// field variables
		var field = Meta.GetDefaultField(DefaultMetaField, FieldType.Boolean);
		var primaryKeyFieldName = field.GetPrimaryKeyName();
		templateItem = template.GetTemplateItem(EntityType.Field);
		var fieldTypeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var fieldNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var fieldCaseSensitiveAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		var fieldSizeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Size);
		var fieldDefaultValueAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.DefaultValue) ?? DefaultTemplateAttribute;
		var fieldBaselineAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.BaseLine);
		var fieldNotNullAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.NotNull);
		var fieldMultiLangualeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Multilingual);
		// relation variables
		templateItem = template.GetTemplateItem(EntityType.Relation);
		var relationNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var relationTypeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var relationToTableAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.To);
		var relationInverseAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.InverseRelation);
		var relationBaselineAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.BaseLine);
		var relationNotNullAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.NotNull);
		var relationConstraintAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Constraint);
		// index variables
		templateItem = template.GetTemplateItem(EntityType.Index);
		var indexNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;

		var buffer = new string?[template.MaxDepth + 2];
		var iterationCount = 0;
		var metaIndex = 0;
		var columnIndex = 0;
		var indexIndex = 0;
		var currentTableId = -1;

		if (schemaNameAttribute is null || tableNameAttribute is null || fieldNameAttribute is null || relationNameAttribute is null || tableIdAttribute is null || fieldTypeAttribute is null ||
			fieldCaseSensitiveAttribute is null || tableBaselineAttribute is null || fieldSizeAttribute is null || fieldBaselineAttribute is null || fieldMultiLangualeAttribute is null || 
			fieldNotNullAttribute is null || relationTypeAttribute is null || relationToTableAttribute is null || relationInverseAttribute is null || relationBaselineAttribute is null ||
			relationNotNullAttribute is null || relationConstraintAttribute is null || indexNameAttribute is null)
		{
			// throw exception !!!
			return [];
		}
		buffer[0] = string.Empty;

		// allocate array
		var result = new Meta[count];

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
						result[metaIndex] =	new(0, SchemaId, 0, 0, 0L, xmlReader.GetAttributeValue(schemaNameAttribute), string.Empty, null, true);
						break;
					case EntityType.Table:
						{
							currentTableId = xmlReader.GetId(tableIdAttribute);
							var (readonlyTable, baseline, cachedTable) = xmlReader.GetTableInfo(tableReadonlyAttribute, tableBaselineAttribute, tableCachedAttribute);
							result[metaIndex] = ToTable(currentTableId, xmlReader.GetAttributeValue(tableNameAttribute), null, null, schemaId, TableType.Business, baseline, false, 
								readonlyTable, cachedTable);
							columnIndex=1;
							indexIndex=1;
						}
						break;
					case EntityType.Field:
						{
							var (fieldType, searchableType) = xmlReader.GetFieldInfo(fieldTypeAttribute, fieldCaseSensitiveAttribute);
							var (size, defaultValue, baseline, notNull, multiLangual) = 
								xmlReader.GetFieldInfo(fieldSizeAttribute, fieldDefaultValueAttribute, fieldBaselineAttribute, fieldNotNullAttribute, fieldMultiLangualeAttribute);
							var fieldName = xmlReader.GetAttributeValue(fieldNameAttribute);
							var fieldId = primaryKeyFieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) ? 0 : columnIndex++;
							result[metaIndex] = ToField(fieldId, fieldName, null, fieldType, size, defaultValue, searchableType, currentTableId, baseline, notNull, multiLangual, true);
							if (searchableType != SearchableType.None)
							{
								++metaIndex;
								result[metaIndex] = ToSearchableColumn(columnIndex, fieldName, fieldType, size, defaultValue, searchableType, currentTableId, baseline, notNull);
								++columnIndex;
							}
							// add time zone column if needed
						}
						break;
					case EntityType.Relation:
						{
							var relationName = xmlReader.GetAttributeValue(relationNameAttribute);
							var (type, toTable, inverseRelation, baseline, notNull, constraint) = 
								xmlReader.GetRelationInfo(relationTypeAttribute, relationToTableAttribute, relationInverseAttribute, relationBaselineAttribute, 
								relationNotNullAttribute, relationConstraintAttribute);
							var toTableCriteria = toTable?.ToUpperInvariant().Trim() ?? string.Empty;
							if (!referenceTables.TryGetValue(toTableCriteria, out var toTableId)) toTableId= -1;
							result[metaIndex] = ToRelation(columnIndex, relationName, type, toTableId, currentTableId, inverseRelation, baseline, notNull, constraint);
							++columnIndex;
						}
						break;
					case EntityType.Index:
						{
							var indexName = xmlReader.GetAttributeValue(indexNameAttribute);
							var (columnList, unique, bitmap, baseline) = xmlReader.GetIndexInfo();
							result[metaIndex] = ToIndex(indexIndex, indexName, columnList, currentTableId, unique, bitmap, baseline);
							++indexIndex;
						}
						break;
					case EntityType.Tablespace: break;
					case EntityType.Comment:
						{
							var comment = xmlReader.ReadString();
							if (!string.IsNullOrWhiteSpace(comment) && metaIndex > 0)
							{
								ref Meta meta = ref result[metaIndex - 1];
								if (meta.ObjectType != SearchableColumnId) result[metaIndex - 1] = SetDescription(ref meta, comment);
								else if (metaIndex > 1) result[metaIndex - 2] = SetDescription(ref result[metaIndex - 2], comment);
							}
						}
;						break;
				}
				if (item.EntityType != EntityType.Undefined && item.EntityType != EntityType.Comment) ++metaIndex;
			}
		}
		return result;
	}

}
