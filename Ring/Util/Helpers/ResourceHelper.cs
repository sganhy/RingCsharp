using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Models;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;

namespace Ring.Util.Helpers;

internal sealed class ResourceHelper
{
	private static readonly object SyncRoot = new();
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceCrLf = @"|||";
	private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
	private static readonly string TemplateResourceNameSpace = ResourceNameSpace+ @"Templates.";
	private static readonly string MessageDescSplitChar = "#$";
	private const char ResourceEndOfLine = '\n';
	private static bool _resourcesLoaded;
	private static bool _schemaTemplateLoaded;
	private static string?[] _logMessages = Array.Empty<string?>();
	private static string?[] _logDescriptions = Array.Empty<string?>();
	private static XmlSchemaTemplate?[] _schemaTemplates = Array.Empty<XmlSchemaTemplate?>();

	internal ResourceHelper()
	{
		// Code size: 19 (0x13)
		if (!_resourcesLoaded) LoadResources();
	}

	internal static string GetErrorMessage(ResourceType resourceType) 
	{
		// Code size: 69 (0x45)
		if (!_resourcesLoaded) LoadResources();
		var message = string.Empty;
		if ((int)resourceType <= _logMessages.Length) message= _logMessages[(int)resourceType - 1] ?? string.Empty;
		return message.Replace(ResourceCrLf, ResourceEndOfLine.ToString());
	}

	internal static XmlSchemaTemplate? GetSchemaTemplate(XmlTemplateType resourceType)
	{
		// Code size: 20 (0x14)
		if (!_schemaTemplateLoaded) LoadSchemaTemplates();
		return _schemaTemplates[(int)resourceType];
	}

#pragma warning disable CA1822, S2325 // Mark members as static
	internal string GetMessage(ResourceType resourceType)
		=> ((int)resourceType <= _logMessages.Length) ? _logMessages[(int)resourceType - 1] : null;
	internal string? GetMessage(LogType logType)
		=> ((int)logType <= _logMessages.Length) ? _logMessages[(int)logType - 1] : null;
	internal string? GetDescription(LogType logType) 
		=> ((int)logType <= _logDescriptions.Length) ? _logDescriptions[(int)logType - 1] : null;
#pragma warning restore S2325, CA1822 // Mark members as static

	internal static HashSet<string> GetReservedWords(DatabaseProvider databaseProvider)
	{
		// Code size: 284 (0x11c)
		return databaseProvider switch
		{
			DatabaseProvider.Oracle => GetCompressedResource(ResourceNameSpace, ResourceType.OracleReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.PostgreSql => GetCompressedResource(ResourceNameSpace, ResourceType.PostgreSQLReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.MySql => GetCompressedResource(ResourceNameSpace, ResourceType.MySQLReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.SqlServer => GetCompressedResource(ResourceNameSpace, ResourceType.SQLServerReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.SqlLite => GetCompressedResource(ResourceNameSpace, ResourceType.SQLiteReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			_ => new HashSet<string>()
		};
	}

	#region private methods

	private static void LoadResources()
	{
		lock (SyncRoot)
		{
			if (!_resourcesLoaded)
			{
				var resourceFile = ResourceType.LogMessage + CompressedResourceSuffix;
				(_logMessages, _logDescriptions) = GetLogResource(ResourceNameSpace, resourceFile);
			}
			_resourcesLoaded = true;
		}
	}

	private static void LoadSchemaTemplates()
	{
		lock (SyncRoot)
		{
			if (!_schemaTemplateLoaded)
			{
				_schemaTemplates = new XmlSchemaTemplate[byte.MaxValue];
				var resourceFile = ResourceType.XmlSchemaTemplate.ToString() + XmlTemplateType.Native + CompressedResourceSuffix;
				var resources = new ReadOnlySpan<string?>(GetCompressedResource(TemplateResourceNameSpace, resourceFile, true));
				var items = new List<XmlSchemaTemplateItem>();
				// just native for the moment
				for (var i=0; i<resources.Length; ++i)
				{
					var entityType = i.ToEntityType();
					if (entityType == EntityType.Undefined) continue;
					if (string.IsNullOrWhiteSpace(resources[i])) continue;
					var elements = resources[i]?.Split(';');
					if (elements==null || elements.Length!=4) continue; // invalid line
					var templateItem = new XmlSchemaTemplateItem(entityType, elements[0], elements[1], ResourceType.Description.ToString(), elements[2], GetXmlAttributes(elements[3]));
					items.Add(templateItem);
				}
				_schemaTemplates[(int)XmlTemplateType.Native] = new XmlSchemaTemplate(XmlTemplateType.Native, items.ToArray());
			}
			_schemaTemplateLoaded = true;
		}
	}

	private static (string?[], string?[]) GetLogResource(string resourceNamespace, string fileName)
	{
		// Code size: 148 (0x94)
		var resultMessage = Array.Empty<string?>();
		var resultDesc = Array.Empty<string?>();
		resultMessage = GetCompressedResource(resourceNamespace, fileName, false);

		// build a description array
		if (resultMessage.Length > 0)
		{
			resultDesc = new string[resultMessage.Length];
			for (var i=0; i < resultMessage.Length; ++i)
			{
				var message = resultMessage[i];
				resultMessage[i] = null;
				resultDesc[i] = null;
				if (!string.IsNullOrEmpty(message))
				{
					var index = message.IndexOf(MessageDescSplitChar, StringComparison.Ordinal);
					if (index >= 0)
					{
						resultMessage[i] = message[..index];
						resultDesc[i] = message[(index + 1)..];
					}
					else
					{
						resultMessage[i] = message;
						resultDesc[i] = null;
					}
				}
			}
		}
		return (resultMessage, resultDesc);
	}

	private static string?[] GetCompressedResource(string resourceNamespace, string fileName, bool toUpper)
	{
		// Code size: 140 (0x8c)
		var resource = resourceNamespace + fileName;
		var assembly = Assembly.GetExecutingAssembly();
		var result = Array.Empty<string>();
		using var stream = assembly.GetManifestResourceStream(resource);
		if (stream == null) return result;
		using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
		using var reader = new StreamReader(decompressionStream);
		result = toUpper ? reader.ReadToEnd().ToUpper(CultureInfo.InvariantCulture).Split(ResourceEndOfLine) :
			reader.ReadToEnd().Split(ResourceEndOfLine);

		return result;
	}


	private static XmlSchemaAttribute[] GetXmlAttributes(string attributes)
	{
		// Code size: 114 (0x72)
		if (string.IsNullOrWhiteSpace(attributes)) return Array.Empty<XmlSchemaAttribute>();
		var span = new ReadOnlySpan<string>(attributes.Split(','));
		var xmlAttributes = new List<XmlSchemaAttribute>(span.Length);
		for (var i = 0; i < span.Length; ++i)
		{
			var attribute = span[i];
			var xmlSchemaAttributeType = i.ToXmlSchemaAttributeType();
			if (!string.IsNullOrWhiteSpace(attribute) && 
				xmlSchemaAttributeType != XmlSchemaAttributeType.Undefined) 
				xmlAttributes.Add(new XmlSchemaAttribute(xmlSchemaAttributeType, attribute));
		}
		return xmlAttributes.ToArray();
	}

	#endregion

}
