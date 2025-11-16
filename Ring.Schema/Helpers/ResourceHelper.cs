using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Xml;
using XmlReaderExtensions = Ring.Schema.Extensions.XmlReaderExtensions;


namespace Ring.Schema.Helpers;

internal sealed class ResourceHelper
{
	private static readonly object SyncRoot = new();
	private static bool _schemaTemplateLoaded;
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceNameSpace = @"Ring.Schema.Resources.";
	private static readonly string TemplateResourceNameSpace = ResourceNameSpace + @"Templates.";
	private static Dictionary<int, SchemaTemplate> _schemaTemplates = new();


	internal static SchemaTemplate? GetSchemaTemplate(DocumentType resourceType)
	{
		// Code size: 33 (0x21)
		if (!_schemaTemplateLoaded) LoadSchemaTemplates();
		var key = (int)resourceType;
		return _schemaTemplates.TryGetValue(key, out var template) ? template : null;
	}

	#region private methods

	private static void LoadSchemaTemplates()
	{
		// Code size: 169 (0xa9)
		lock (SyncRoot)
		{
			if (!_schemaTemplateLoaded)
			{
				var resourceNativeFile = DocumentType.XmlNative + CompressedResourceSuffix;
				var resourceClfyFile = DocumentType.XmlClfy + CompressedResourceSuffix;
				var xmlNativeStr = GetCompressedResource(TemplateResourceNameSpace, resourceNativeFile, false);
				var xmlClfyStr = GetCompressedResource(TemplateResourceNameSpace, resourceClfyFile, false);

				_schemaTemplates = new Dictionary<int, SchemaTemplate>
				{
					{ (int)DocumentType.XmlNative, GetSchemaTemplate(DocumentType.XmlNative, resourceNativeFile,  xmlNativeStr) },
					{ (int)DocumentType.XmlClfy, GetSchemaTemplate(DocumentType.XmlClfy, resourceClfyFile,  xmlClfyStr) }
				};
			}
			_schemaTemplateLoaded = true;
		}
	}

	private static SchemaTemplate GetSchemaTemplate(DocumentType documentType, string resourceFile, string xmlString)
	{
		// Code size: 671 (0x29f)
		var subResult = new List<SchemaTemplateItem>();
		var attributes = new List<SchemaTemplateAttribute>();
		var tagId = SchemaTemplateAttributeType.Id.ToString().ToUpperInvariant();
		var tagParent = SchemaTemplateAttributeType.Parent.ToString().ToUpperInvariant();
		var tagValue = SchemaTemplateAttributeType.Value.ToString().ToUpperInvariant();
		var tagVal = SchemaTemplateAttributeType.Val.ToString().ToUpperInvariant();
		var tagDepth = SchemaTemplateAttributeType.Depth.ToString().ToUpperInvariant();
		var tagAttribute = SchemaTemplateAttributeType.Attribute.ToString().ToUpperInvariant();
		var tagTemplate = SchemaTemplateAttributeType.Template.ToString().ToUpperInvariant();
		var startTage = string.Empty;
		var parent = string.Empty;
		var depth = 0;
		var entityType = EntityType.Undefined;
		List<SchemaTemplateAttributeValue> attributeValuesLst = [];
		var attributeValues = new Dictionary<string, string>(8) { { tagId, string.Empty }, { tagParent, string.Empty }, { tagValue, string.Empty } , { tagDepth , string.Empty } };
		using var stringReader = new StringReader(xmlString);
		using var reader = XmlReader.Create(stringReader);

		while (reader.Read())
		{
			attributeValues[tagId] = string.Empty;
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (string.Equals(tagAttribute, reader.Name, StringComparison.OrdinalIgnoreCase))
				{
					// manage attribute
					reader.LoadAttributes(attributeValues); // read attribute after node !!!
					attributeValuesLst.Clear();
					var xmlSchemaAttributeType = ToXmlSchemaAttributeType(attributeValues[tagId]);
					attributes.Add(new SchemaTemplateAttribute(attributeValues[tagValue], xmlSchemaAttributeType, []));
				}
				else if (string.Equals(tagVal, reader.Name, StringComparison.OrdinalIgnoreCase))
				{
					// manage attribute value (domaine list) 
					var id = XmlReaderExtensions.GetAttributeValue(reader, tagId);
					var value = XmlReaderExtensions.GetAttributeValue(reader, tagValue)?.ToUpperInvariant();
					if (id != null && value != null) attributeValuesLst.Add(new SchemaTemplateAttributeValue(int.Parse(id, CultureInfo.InvariantCulture), value));
				}
				else
				{
					startTage = reader.Name;
					reader.LoadAttributes(attributeValues); // read attribute after node !!!
					entityType = ToEntityType(attributeValues[tagId]);
					if (entityType == EntityType.Undefined) continue;
					parent = attributeValues[tagParent];
					depth = int.Parse(attributeValues[tagDepth], CultureInfo.InvariantCulture);
					attributes.Clear();
				}
			}
			if (reader.NodeType == XmlNodeType.EndElement)
			{
				if (string.Equals(tagAttribute, reader.Name, StringComparison.OrdinalIgnoreCase))
				{
					var getLastItem = attributes.Last();
					var attributeValuesArr = attributeValuesLst.ToArray();
					attributeValuesArr.AsSpan().Sort((x, y) => string.CompareOrdinal(x.Value, y.Value));
					getLastItem = getLastItem.SetValues(attributeValuesArr);
					attributes[^1] = getLastItem;
				}
				else if (!string.Equals(tagTemplate, reader.Name, StringComparison.OrdinalIgnoreCase)) 
				{
					var attributeArr = attributes.ToArray();
					attributeArr.AsSpan().Sort((x, y) => x.TypeId.CompareTo(y.TypeId));
					var item = new SchemaTemplateItem(entityType, startTage, parent, string.Empty, string.Empty, depth, attributeArr);
					parent = string.Empty;
					subResult.Add(item);
				}
			}
		}
		var templateItems = subResult.ToArray();
		templateItems.AsSpan().Sort((x, y) => x.EntityTypeId.CompareTo(y.EntityTypeId));
		return new SchemaTemplate(resourceFile, documentType, templateItems, GetMaxDepth(templateItems));
	}

	private static EntityType ToEntityType(string attributeValue) => 
		int.TryParse(attributeValue, out int id) ? id.ToEntityType() : EntityType.Undefined; // Code size: 20 (0x14)
	private static SchemaTemplateAttributeType ToXmlSchemaAttributeType(string attributeValue) => 
		int.TryParse(attributeValue, out int id) ? id.ToXmlSchemaAttributeType() : SchemaTemplateAttributeType.Undefined; // Code size: 23 (0x17)

	private static int GetMaxDepth(SchemaTemplateItem[] items) 
	{
		// Code size: 55 (0x37)
		var result = 0;
		var span = new ReadOnlySpan<SchemaTemplateItem>(items);
		foreach (var item in span) if (item.Depth > result) result = item.Depth;
		return result;
	}

	private static string GetCompressedResource(string resourceNamespace, string fileName, bool toUpper)
	{
		// Code size: 106 (0x6a)
		var resource = resourceNamespace + fileName;
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream(resource);
		if (stream is null) return string.Empty;
		using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
		using var reader = new StreamReader(decompressionStream);
		var content = reader.ReadToEnd();
		return toUpper ? content.ToUpperInvariant() : content;
	}

	#endregion

}
