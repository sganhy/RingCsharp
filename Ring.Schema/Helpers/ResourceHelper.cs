using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.IO.Compression;
using System.Reflection;
using System.Xml;

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
		// Code size: 124 (0x7c)
		lock (SyncRoot)
		{
			if (!_schemaTemplateLoaded)
			{
				var doc = new XmlDocument();
				var resourceFile = DocumentType.XmlNative + CompressedResourceSuffix;
				var xmlStr = GetCompressedResource(TemplateResourceNameSpace, resourceFile, false);
				_schemaTemplates = new Dictionary<int, SchemaTemplate>
				{
					{ (int)DocumentType.XmlNative, GetSchemaTemplate(DocumentType.XmlNative, xmlStr) }
				};
			}
			_schemaTemplateLoaded = true;
		}
	}

	private static SchemaTemplate GetSchemaTemplate(DocumentType documentType, string xmlString)
	{
		// Code size: 448 (0x1c0)
		var subResult = new List<SchemaTemplateItem>();
		var attributes = new List<SchemaTemplateAttribute>();
		var tagId = SchemaTemplateAttributeType.Id.ToString().ToUpperInvariant();
		var tagParent = SchemaTemplateAttributeType.Parent.ToString().ToUpperInvariant();
		var tagValue = SchemaTemplateAttributeType.Value.ToString().ToUpperInvariant();
		var tagAttribute = SchemaTemplateAttributeType.Attribute.ToString().ToUpperInvariant();
		var startTage = string.Empty;
		var parent = string.Empty;
		var entityType = EntityType.Undefined;
		var attributeValues = new Dictionary<string, string>(6) { { tagId, string.Empty }, { tagParent, string.Empty }, { tagValue, string.Empty } };
		using var stringReader = new StringReader(xmlString);
		using var reader = XmlReader.Create(stringReader);
		while (reader.Read())
		{
			attributeValues[tagId] = string.Empty;
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (string.Equals(tagAttribute, reader.Name, StringComparison.OrdinalIgnoreCase))
				{
					reader.LoadAttributes(attributeValues); // read attribute after node !!!
					var xmlSchemaAttributeType = ToXmlSchemaAttributeType(attributeValues[tagId]);
					var templateAttribute = new SchemaTemplateAttribute(xmlSchemaAttributeType, attributeValues[tagValue]);
					attributes.Add(templateAttribute);
				}
				else 
				{ 
					startTage = reader.Name;
					reader.LoadAttributes(attributeValues); // read attribute after node !!!
					entityType = ToEntityType(attributeValues[tagId]);
					if (entityType == EntityType.Undefined) continue;
					parent = attributeValues[tagParent];
					attributes.Clear();
				}
			}
			if (reader.NodeType == XmlNodeType.EndElement)
			{
				var item = new SchemaTemplateItem(entityType, startTage, parent, string.Empty, string.Empty, attributes.ToArray());
				subResult.Add(item);
			}
		}
		return new SchemaTemplate(documentType, subResult.ToArray());
	}

	private static EntityType ToEntityType(string attributeValue) => 
		int.TryParse(attributeValue, out int id) ? id.ToEntityType() : EntityType.Undefined; // Code size: 20 (0x14)
	private static SchemaTemplateAttributeType ToXmlSchemaAttributeType(string attributeValue) => 
		int.TryParse(attributeValue, out int id) ? id.ToXmlSchemaAttributeType() : SchemaTemplateAttributeType.Undefined; // Code size: 23 (0x17)

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
