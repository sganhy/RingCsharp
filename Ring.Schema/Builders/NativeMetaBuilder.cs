using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Xml;

namespace Ring.Schema.Builders;

internal sealed class NativeMetaBuilder : BaseMetaBuilder, IMetaBuilder
{
	internal NativeMetaBuilder() : base(
		DocumentType.XmlNative.GetSchemaTemplate() ?? DefaultTemplate,
		DocumentType.XmlNative.GetSchemaTemplate().ToTagDictionary(StringComparer.Ordinal),
		DocumentType.XmlNative)
	{
	}

	public ValueTask<Meta[]> GetMeta(string FilePath, int count, CancellationToken cancellationToken = default)
	{
		return GetMeta(FilePath, TagDictionary, Template, count, cancellationToken);
	}

	private async static ValueTask<Meta[]> GetMeta(string FilePath, Dictionary<string, SchemaTemplateItem> tagDico, SchemaTemplate template, int count, CancellationToken cancellationToken = default)
	{
		// allocate array
		var result = new Meta[count];
		var readerSettings = new XmlReaderSettings
		{
			IgnoreWhitespace = true,
			CheckCharacters = false,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			Async = true,
			CloseInput = false // We manage disposal ourselves
		};

		var schemaId = 0;
		var templateItem = template.GetTemplateItem(EntityType.Schema);
		var schemaNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		templateItem = template.GetTemplateItem(EntityType.Table);
		var tableNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var tableIdAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Id)?.Name;
		templateItem = template.GetTemplateItem(EntityType.Field);
		var fieldTypeAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var fieldNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var fieldCaseSensitiveAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		templateItem = template.GetTemplateItem(EntityType.Relation);
		var relationNameAttribute = templateItem?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var buffer = new string?[template.MaxDepth + 2];
		var iteration = 0;
		var metaIndex = 0;
		var currentTableId = -1;

		if (schemaNameAttribute is null || tableNameAttribute is null || fieldNameAttribute is null || relationNameAttribute is null || tableIdAttribute is null || fieldTypeAttribute is null 
			|| fieldCaseSensitiveAttribute is null)
		{
			// throw exception !!!
			return [];
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
				if ((iteration & 0xFF) == 0) cancellationToken.ThrowIfCancellationRequested();
				if (xmlReader.NodeType != XmlNodeType.Element) continue;

				var currentDepth = xmlReader.Depth;
				if (currentDepth > template.MaxDepth) continue; // don't read below max depth (skipped)
				buffer[currentDepth + 1] = xmlReader.Name;

				if (tagDico.TryGetValue(xmlReader.Name, out var item))
				{
					var parent = buffer[currentDepth];
					if (StringComparer.Ordinal.Equals(item.ParentTag, parent) || AllParent.Equals(item.ParentTag, StringComparison.OrdinalIgnoreCase))
					{
						switch (item.EntityType)
						{
							case EntityType.Schema: 
								result[metaIndex] =	new(0, SchemaId, 0, 0, 0L, GetAttributeValue(xmlReader, schemaNameAttribute), string.Empty, null, true);
								break;
							case EntityType.Table:
								currentTableId = GetId(xmlReader, tableIdAttribute);
								result[metaIndex] = ToTable(currentTableId, GetAttributeValue(xmlReader, tableNameAttribute), null, null, schemaId, TableType.Business, true, false, true, true);
								break;
							case EntityType.Field:
								{
									(var fieldType, var searchableType) = xmlReader.GetFieldInfo(fieldTypeAttribute, fieldCaseSensitiveAttribute);
									result[metaIndex] = ToField(0, GetAttributeValue(xmlReader, fieldNameAttribute), null, fieldType, 0, null, searchableType, 
										currentTableId, true, true,true, true);
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
;								break;
						}
						if (item.EntityType != EntityType.Undefined && item.EntityType != EntityType.Comment) ++metaIndex;
					}
				}
				++iteration;
			}
		}
		finally
		{
			await fs.DisposeAsync().ConfigureAwait(false);
		}
		return result;
	}

}
