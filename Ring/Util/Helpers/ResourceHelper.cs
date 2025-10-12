using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;

namespace Ring.Util.Helpers;

/// <summary>
///		This class serves as the resource management backbone for the Ring0 class library
/// </summary>
internal sealed class ResourceHelper
{
	private static readonly object SyncRoot = new();
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceEof = @"|||";
	private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
	private static readonly string MessageDescSplitChar = "#$";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private const char ResourceEndOfLine = '\n';
	private static bool _resourcesLoaded;
	private static bool _parameterLoaded;
	private static string?[] _logMessages = Array.Empty<string?>();
	private static string?[] _logDescriptions = Array.Empty<string?>();	
	private static Dictionary<int, Parameter> _parameters = new();

	internal ResourceHelper()
	{
		// Code size: 19 (0x13)
		if (!_resourcesLoaded) LoadResources();
	}

	internal static string GetErrorMessage(ResourceType resourceType) 
	{
		// Code size: 53 (0x35)
		if (!_resourcesLoaded) LoadResources();
		var index = (int)resourceType - 1;
		if (index < 0 || index >= _logMessages.Length) return string.Empty;
		return _logMessages[index] ?? string.Empty;
	}
		
#pragma warning disable CA1822, S2325 // Mark members as static

	internal string GetMessage(ResourceType resourceType) // Code size: 22 (0x16)
        => ((int)resourceType <= _logMessages.Length) ? _logMessages[(int)resourceType - 1] : null;
	internal string? GetMessage(LogType logType) // Code size: 22 (0x16)
        => ((int)logType <= _logMessages.Length) ? _logMessages[(int)logType - 1] : null;
	internal string? GetDescription(LogType logType) // Code size: 22 (0x16)
        => ((int)logType <= _logDescriptions.Length) ? _logDescriptions[(int)logType - 1] : null;

#pragma warning restore S2325, CA1822 // Mark members as static

	internal static Parameter GetParameter(ParameterType parameterType)
	{
		// Code size: 70 (0x46)
		var parameterTypeId = (int)parameterType;
		if (!_parameterLoaded) LoadParameters();
		if (_parameters.TryGetValue(parameterTypeId, out var parameter)) return parameter;
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
		// Code size: 282 (0x11a)
		lock (SyncRoot)
		{
			if (!_parameterLoaded)
			{
				var resourceFile = EntityType.Parameter.ToString() + CompressedResourceSuffix;
				var parameters = GetCompressedResource(ResourceNameSpace, resourceFile, false);
				_parameters = new Dictionary<int, Parameter>(parameters.Length*2);

				foreach (var param in new ReadOnlySpan<string?>(parameters))
				{
					if (string.IsNullOrEmpty(param)) continue;
					var parts = param.Split(',');
					if (parts.Length < 6) continue;  // Skip malformed entries

					if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var id)) continue;
					var paramType = id.ToParameterType();
					if (paramType == ParameterType.Undefined)  continue;
					_parameters.Add(id, new Parameter( id, parts[1] ?? string.Empty, parts[2], paramType, parts[3].ToFieldType(), string.Empty, parts[5], 0, parts[4].ToEntityType(), true, true));
				}
			}
			_parameterLoaded = true;
		}
	}

	private static (string?[], string?[]) GetLogResource(string resourceNamespace, string fileName)
	{
		// Code size: 223 (0xdf)
		var resultMessage = GetCompressedResource(resourceNamespace, fileName, false);
		if (resultMessage.Length == 0) return (resultMessage, Array.Empty<string?>());
		var resultDesc = new string?[resultMessage.Length];
		var strResourceEof = ResourceEndOfLine.ToString();
		for (var i = 0; i < resultMessage.Length; ++i)
		{
			var message = resultMessage[i];
			if (string.IsNullOrEmpty(message)) continue;
			var index = message.IndexOf(MessageDescSplitChar, StringComparison.Ordinal);
			if (index >= 0)
			{
				var mainText = message[..index];
				var descText = message[(index + MessageDescSplitChar.Length)..];
				resultMessage[i] = mainText.Replace(ResourceEof, strResourceEof, StringComparison.Ordinal);
				resultDesc[i] = descText.Replace(ResourceEof, strResourceEof, StringComparison.Ordinal);
			}
			else resultMessage[i] = message.Replace(ResourceEof, strResourceEof, StringComparison.Ordinal);
		}
		return (resultMessage, resultDesc);
	}

	private static string?[] GetCompressedResource(string resourceNamespace, string fileName, bool toUpper)
	{
		// Code size: 114 (0x72)
		var resource = resourceNamespace + fileName;
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream(resource);
		if (stream is null) return Array.Empty<string?>();

		using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
		using var reader = new StreamReader(decompressionStream);
		var content = reader.ReadToEnd();
		if (toUpper) content = content.ToUpperInvariant();
		return content.Split(ResourceEndOfLine);
	}

	#endregion

}
