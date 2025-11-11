using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Ring.Schema.Validators;

internal sealed class NativeDocumentValidator : BaseDocumentValidator, IDocumentValidator
{

	internal NativeDocumentValidator() : base (
		DocumentType.XmlNative.GetSchemaTemplate() ?? DefaultTemplate, 
		DocumentType.XmlNative.GetSchemaTemplate().ToTagDictionary(StringComparer.Ordinal), 
		DocumentType.XmlNative)
	{ 
	}

	public ValueTask<DocumentStats> GetMetaCountAsync(string FilePath, CancellationToken cancellationToken = default)
	{ 
		return GetMetaCountAsync(FilePath, TagDictionary, true, Template, cancellationToken);
	}

	/// <summary>
	///	 Compute the number of meta, before allocation + light validation of xml structure
	/// </summary>
	private async ValueTask<DocumentStats> GetMetaCountAsync(string FilePath, Dictionary<string, SchemaTemplateItem> tagDico, bool hasTimeZoneOffsetColumn, SchemaTemplate template, CancellationToken cancellationToken = default)
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

		// initialize
		ResetStats();

		var fieldItem = template.GetTemplateItem(EntityType.Field);
		var fieldTypeAttribute = fieldItem?.GetAttribute(SchemaTemplateAttributeType.Type);
		var fieldCaseSensitiveAttribute = fieldItem?.GetAttribute(SchemaTemplateAttributeType.CaseSensitive);
		var result = 0;
		var buffer = new string?[template.MaxDepth + 2];

		if (fieldTypeAttribute is null || fieldCaseSensitiveAttribute is null)
		{
			// throw exception !!!
			return new DocumentStats( SchemaCount, TableCount, FieldCount, UndefinedFieldTypeCount, RelationCount, IndexCount, WrongParentCount, TableSpaceCount, LineCount);
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
		return new DocumentStats(SchemaCount, TableCount, FieldCount, UndefinedFieldTypeCount, RelationCount, IndexCount, WrongParentCount, TableSpaceCount, LineCount);
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
}
