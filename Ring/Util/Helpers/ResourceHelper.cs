using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Models;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Xml;

namespace Ring.Util.Helpers;

/// <summary>
///		This class serves as the resource management backbone for the Ring0 class library
/// </summary>
internal sealed class ResourceHelper
{
	private static readonly object SyncRoot = new();
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceCrLf = @"|||";
	private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
	private static readonly string TemplateResourceNameSpace = ResourceNameSpace+ @"Templates.";
	private static readonly string MessageDescSplitChar = "#$";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private const char ResourceEndOfLine = '\n';
	private static bool _resourcesLoaded;
	private static bool _schemaTemplateLoaded;
	private static bool _parameterLoaded;
	private static string?[] _logMessages = Array.Empty<string?>();
	private static string?[] _logDescriptions = Array.Empty<string?>();
	private static Dictionary<int, SchemaTemplate> _schemaTemplates = new();
	private static Dictionary<int, Parameter> _parameters = new();

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

	internal static SchemaTemplate? GetSchemaTemplate(DocumentType resourceType)
	{
        // Code size: 33 (0x21)
        if (!_schemaTemplateLoaded) LoadSchemaTemplates();
		var key = (int)resourceType;
		return _schemaTemplates.TryGetValue(key, out var template) ? template : null;
    }

#pragma warning disable CA1822, S2325 // Mark members as static
	internal string GetMessage(ResourceType resourceType)
		=> ((int)resourceType <= _logMessages.Length) ? _logMessages[(int)resourceType - 1] : null;
	internal string? GetMessage(LogType logType)
		=> ((int)logType <= _logMessages.Length) ? _logMessages[(int)logType - 1] : null;
	internal string? GetDescription(LogType logType) 
		=> ((int)logType <= _logDescriptions.Length) ? _logDescriptions[(int)logType - 1] : null;
#pragma warning restore S2325, CA1822 // Mark members as static

	internal static Parameter GetParameter(ParameterType parameterType)
	{
		// Code size: 78 (0x4e)
		var parameterTypeId = (int)parameterType;
		if (!_parameterLoaded) LoadParameters();
		if (_parameters.ContainsKey(parameterTypeId)) return _parameters[parameterTypeId];
		throw new ArgumentException(string.Format(DefaultCulture, GetErrorMessage(ResourceType.WrongParameterType), parameterType.ToString()));
	}

	internal static HashSet<string> GetReservedWords(DatabaseProvider databaseProvider)
	{
		// Code size: 266 (0x10a)
		switch (databaseProvider)
		{
			case DatabaseProvider.Oracle: return GetCompressedResource(ResourceNameSpace, ResourceType.OracleReservedKeyWord + CompressedResourceSuffix, true).ToHashSet();
			case DatabaseProvider.PostgreSql: return GetCompressedResource(ResourceNameSpace, ResourceType.PostgreSQLReservedKeyWord + CompressedResourceSuffix, true).ToHashSet();
			case DatabaseProvider.MySql: return GetCompressedResource(ResourceNameSpace, ResourceType.MySQLReservedKeyWord + CompressedResourceSuffix, true).ToHashSet();
			case DatabaseProvider.SqlServer: return GetCompressedResource(ResourceNameSpace, ResourceType.SQLServerReservedKeyWord + CompressedResourceSuffix, true).ToHashSet();
			case DatabaseProvider.SqlLite: return GetCompressedResource(ResourceNameSpace, ResourceType.SQLiteReservedKeyWord + CompressedResourceSuffix, true).ToHashSet();
		};
		return new HashSet<string>();
	}

	#region private methods

	private static string? GetValue(ReadOnlySpan<string?> values, int index) => index < values.Length ? values[index] : null;

	private static void LoadResources()
	{
		// Code size: 100 (0x64)
		lock (SyncRoot)
		{
			if (!_resourcesLoaded)
			{
				var resourceFile = ResourceType.LogMessage + CompressedResourceSuffix;
				(_logMessages, _logDescriptions) = GetLogResource(ResourceNameSpace, resourceFile);
				_resourcesLoaded = true;
			}
			
		}
	}

	private static void LoadParameters()
	{
        // Code size: 322 (0x142)
        lock (SyncRoot)
		{
			if (!_parameterLoaded)
			{
				var resourceFile = EntityType.Parameter.ToString() + CompressedResourceSuffix;
				var parameters = new ReadOnlySpan<string?>(GetCompressedResource(ResourceNameSpace, resourceFile, false));
				_parameters = new Dictionary<int, Parameter>(parameters.Length*2);

				foreach (var param in parameters)
				{
					if (param != null)
					{
						//eg. @version,Schema version,16,7, ==> 0=id, 1=name; 2=description; 3=fielType; 4=entityType; 5=defaultValue
						var arr = new ReadOnlySpan<string?>(param.Split(','));
						var id = int.Parse(arr[0] ?? string.Empty, CultureInfo.InvariantCulture);
						var paramType = id.ToParameterType();
						if (paramType != ParameterType.Undefined)
						{
							_parameters.Add(id, new Parameter((int)paramType, GetValue(arr, 1) ?? string.Empty, GetValue(arr, 2), paramType, GetValue(arr, 3).ToFieldType(),
								string.Empty, GetValue(arr, 5), 0, GetValue(arr, 4).ToEntityType(), true, true));
						} else throw new ArgumentException(); // force exception just during unitest run ! avoid to call LoadResource here (Recursive Exception Issue risk)

                    }
				}
			}
			_parameterLoaded = true;
		}
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
                var xmlStr = GetCompressedResource(TemplateResourceNameSpace, resourceFile, false, true)[0] ?? string.Empty;
				_schemaTemplates = new Dictionary<int, SchemaTemplate>
                {
                    { (int)DocumentType.XmlNative, GetSchemaTemplate(DocumentType.XmlNative, xmlStr) }
                };
            }
            _schemaTemplateLoaded = true;
		}
	}

	private static (string?[], string?[]) GetLogResource(string resourceNamespace, string fileName)
	{
		// Code size: 148 (0x94)
		string?[] resultMessage;
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
        var attributeValues = new Dictionary<string, string>() {{tagId , string.Empty}, {tagParent , string.Empty}, {tagValue, string.Empty}};
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

    private static string?[] GetCompressedResource(string resourceNamespace, string fileName, bool toUpper, bool noSplit=false)
	{
        // Code size: 183 (0xb7)
        var resource = resourceNamespace + fileName;
		var assembly = Assembly.GetExecutingAssembly();
        string?[] result = Array.Empty<string>();
		using var stream = assembly.GetManifestResourceStream(resource);
		if (stream == null) return result;
		using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
		using var reader = new StreamReader(decompressionStream);
		if (noSplit)
		{
			result = new string?[1];
			result[0] = toUpper ? reader.ReadToEnd().ToUpper(CultureInfo.InvariantCulture) : reader.ReadToEnd();
		}
		else result = toUpper ? reader.ReadToEnd().ToUpper(CultureInfo.InvariantCulture).Split(ResourceEndOfLine) : reader.ReadToEnd().Split(ResourceEndOfLine);
		return result;
	}


	#endregion

}
