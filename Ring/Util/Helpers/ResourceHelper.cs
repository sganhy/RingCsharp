using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;

namespace Ring.Util.Helpers;

internal sealed class ResourceHelper
{
	private static readonly object SyncRoot = new();
	private static readonly string ResourceSuffix = @".txt";
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceCrLf = @"|||";
	private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
	private static readonly string MessageDescSplitChar = "#$";
	private const char ResourceEndOfLine = '\n';
	private static bool _resourcesLoaded;
	private static string?[] _logMessages = Array.Empty<string?>();
	private static string?[] _logDescriptions = Array.Empty<string?>();

	internal ResourceHelper()
	{
		// Code size: 69 (0x45)
		if (!_resourcesLoaded) LoadResources();
	}

	internal static string GetErrorMessage(ResourceType resourceType) 
	{
		// Code size: 97 (0x61)
		lock (SyncRoot)
		{
			if (!_resourcesLoaded) LoadResources();
		}
		var message = string.Empty;
		if ((int)resourceType <= _logMessages.Length) message= _logMessages[(int)resourceType - 1] ?? string.Empty;
		return message.Replace(ResourceCrLf, ResourceEndOfLine.ToString());
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
		// Code size: 241 (0xf1)
		return databaseProvider switch
		{
			DatabaseProvider.Oracle => GetCompressedResource(ResourceType.OracleReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.PostgreSql => GetCompressedResource(ResourceType.PostgreSQLReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.MySql => GetCompressedResource(ResourceType.MySQLReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.SqlServer => GetCompressedResource(ResourceType.SQLServerReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
			DatabaseProvider.SqlLite => GetCompressedResource(ResourceType.SQLiteReservedKeyWord + CompressedResourceSuffix, true).ToHashSet(),
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
				var resourceFile = ResourceType.LogMessage + ResourceSuffix;
				(_logMessages, _logDescriptions) = GetLogResource(resourceFile);
			}
			_resourcesLoaded = true;
		}
	}

	private static (string?[], string?[]) GetLogResource(string fileName)
	{
		// Code size: 251 (0xfb)
		var resultMessage = Array.Empty<string?>();
		var resultDesc = Array.Empty<string?>();
		var resource = ResourceNameSpace + fileName;
		var assembly = Assembly.GetExecutingAssembly();
		using (var stream = assembly.GetManifestResourceStream(resource))
		{
			if (stream == null) return (resultMessage, resultDesc);
			using var reader = new StreamReader(stream);
			resultMessage = reader.ReadToEnd().Split(ResourceEndOfLine);
		}
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

	private static string?[] GetCompressedResource(string fileName, bool toUpper)
	{
		// Code size: 140 (0x8c)
		var resource = ResourceNameSpace + fileName;
		var assembly = Assembly.GetExecutingAssembly();
		var result = Array.Empty<string>();
		using var stream = assembly.GetManifestResourceStream(resource);
		if (stream == null) return result;
		using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
		using var reader = new StreamReader(decompressionStream);
		result = toUpper ? 
			reader.ReadToEnd().ToUpper(CultureInfo.InvariantCulture).Split(ResourceEndOfLine) :
			reader.ReadToEnd().Split(ResourceEndOfLine);

		return result;
	}

	#endregion 

}
