using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Extensions;
using Ring.Schema.Models;
using System.Globalization;
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
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private static Dictionary<int, SchemaTemplate> _schemaTemplates = new();

    internal static SchemaTemplate? GetSchemaTemplate(DocumentType resourceType)
    {

        // Code size: 33 (0x21)
        if (!_schemaTemplateLoaded) LoadSchemaTemplates();
        var key = (int)resourceType;
        return _schemaTemplates.TryGetValue(key, out var template) ? template : null;
    }

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
        // Code size: 263 (0x107)
        var subResult = new List<SchemaTemplateItem>();
        var tagId = SchemaTemplateAttributeType.Id.ToString().ToUpper(DefaultCulture);
        var tagParent = SchemaTemplateAttributeType.Parent.ToString().ToUpper(DefaultCulture);
        var tagValue = SchemaTemplateAttributeType.Value.ToString().ToUpper(DefaultCulture);
        var startTage = string.Empty;
        var parent = string.Empty;
        var entityType = EntityType.Undefined;
        var attributeValues = new Dictionary<string, string>() { { tagId, string.Empty }, { tagParent, string.Empty }, { tagValue, string.Empty } };
        using var stringReader = new StringReader(xmlString);
        using var reader = XmlReader.Create(stringReader);
        while (reader.Read())
        {
            attributeValues[tagId] = string.Empty;
            if (reader.NodeType == XmlNodeType.Element)
            {
                startTage = reader.Name;
                reader.LoadAttributes(attributeValues);
                entityType = ToEntityType(attributeValues[tagId]);
                if (entityType == EntityType.Undefined) continue;
                parent = attributeValues[tagParent];
                var dept = reader.Depth;

            }
            if (reader.NodeType == XmlNodeType.EndElement)
            {
                var item = new SchemaTemplateItem(entityType, startTage, parent, string.Empty, string.Empty, new SchemaTemplateAttribute[0]);
                subResult.Add(item);
            }
        }
        return new SchemaTemplate(documentType, subResult.ToArray());
    }

    private static EntityType ToEntityType(string attributeValue) => int.TryParse(attributeValue, out int id) ? id.ToEntityType() : EntityType.Undefined; // Code size: 20 (0x14)

    private static string GetCompressedResource(string resourceNamespace, string fileName, bool toUpper)
    {
        // Code size: 120 (0x78)
        var resource = resourceNamespace + fileName;
        var assembly = Assembly.GetExecutingAssembly();
        var result = string.Empty;
        using var stream = assembly.GetManifestResourceStream(resource);
        if (stream == null) return result;
        using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
        using var reader = new StreamReader(decompressionStream);
        result = toUpper ? reader.ReadToEnd().ToUpper(CultureInfo.InvariantCulture) : reader.ReadToEnd();
        return result;
    }
}
