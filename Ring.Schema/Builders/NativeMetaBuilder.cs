using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
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

	private async ValueTask<Meta[]> GetMetaAsync(string filePath, Dictionary<string, SchemaTemplateItem> tagDico, Dictionary<string, int> referenceTables, SchemaTemplate template, int count, CancellationToken cancellationToken = default)
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
		var field = Meta.GetDefaultField(DefaultMetaField, FieldType.Boolean);
		var primaryKeyFieldName = field.GetPrimaryKeyName();
		var buffer = new string?[template.MaxDepth + 2];
		var iterationCount = 0;
		var metaIndex = 0;
		var columnIndex = 0;
		var indexIndex = 0;
		var currentTableId = -1;

		if (LoadTemplateErrorCount > 0)
		{
			// log here !!!
			return Array.Empty<Meta>();
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
				if (metaIndex >= count)
				{
					// log here !!! - wrong template defition
					return Array.Empty<Meta>();
				}

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
						{
							var name = GetSchemaInfo(xmlReader);
							result[metaIndex] = ToSchema(name);
							++metaIndex;
						}
						break;
					case EntityType.Table:
						{
							(currentTableId, var name, var readonlyTable, var baseline, var cachedTable) = GetTableInfo(xmlReader);
							result[metaIndex] = ToTable(currentTableId, name, null, null, schemaId, TableType.Business, baseline, false, readonlyTable, cachedTable);
							columnIndex = indexIndex = 1;
							++metaIndex;
						}
						break;
					case EntityType.Field:
						{
							var (name, fieldType, searchableType,  size, defaultValue, baseline, notNull, multiLangual) = GetFieldInfo(xmlReader);
							var fieldId = primaryKeyFieldName.Equals(name, StringComparison.OrdinalIgnoreCase) ? 0 : columnIndex++;
							result[metaIndex] = ToField(fieldId, name, null, fieldType, size, defaultValue, searchableType, currentTableId, baseline, notNull, multiLangual, true);
							++metaIndex;
							if (searchableType != SearchableType.None)
							{
								result[metaIndex] = ToSearchableColumn(columnIndex, name, fieldType, size, defaultValue, searchableType, currentTableId, baseline, notNull);
								++columnIndex;
								++metaIndex;
							}
							// add time zone column if needed
						}
						break;
					case EntityType.Relation:
						{
							var (name, type, toTable, inverseRelation, baseline, notNull, constraint) = GetRelationInfo(xmlReader);
							var toTableCriteria = toTable?.ToUpperInvariant().Trim() ?? string.Empty;
							if (!referenceTables.TryGetValue(toTableCriteria, out var toTableId)) toTableId= -1;
							result[metaIndex] = ToRelation(columnIndex, name, type, toTableId, currentTableId, inverseRelation, baseline, notNull, constraint);
							++columnIndex;
							++metaIndex;
						}
						break;
					case EntityType.Index:
						{
							var (name, columnList, unique, bitmap, baseline) = await GetIndexInfoAsync(xmlReader, tagDico).ConfigureAwait(false);
							result[metaIndex] = ToIndex(indexIndex, name, columnList, currentTableId, unique, bitmap, baseline);
							++indexIndex;
							++metaIndex;
						}
						break;
					case EntityType.Tablespace:
						{
							var (name, file, table, index) = GetTableSpaceInfo(xmlReader);
							result[metaIndex] = ToTablespace(name, file, table, index);
							++metaIndex;
						}
						break;
					case EntityType.Comment:
						{
							var comment = xmlReader.ReadString();
							if (!string.IsNullOrWhiteSpace(comment) && metaIndex > 0)
							{
								Meta meta = result[metaIndex - 1];
								if (meta.ObjectType != SearchableColumnId) result[metaIndex - 1] = SetDescription(ref meta, comment);
								else if (metaIndex > 1) result[metaIndex - 2] = SetDescription(ref result[metaIndex - 2], comment);
							}
						}
;						break;
				}
			}
		}
		return result;
	}

}
