using Ring.Logging;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Util.Helpers;

/// <summary>
///		This class serves as the resource management backbone for the Ring0 class library
/// </summary>
internal sealed class ResourceHelper
{
	private static readonly object SyncRoot = new();
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string ResourceEof = @"|||";
	private const char ResourceEndOfLine = '\n';
	private static bool _resourcesLoaded;
	private static bool _parameterLoaded;
	private static bool _methodInfoLoaded;
	private static Dictionary<int, string> _logMessages = new();
	private static Dictionary<int, string> _logDescriptions = new();
	private static Dictionary<int, string> _methodInfos = new();
	private static Dictionary<int, Parameter> _parameters = new();
	private static readonly Logger _logger = Global.LoggerFactory.CreateLogger<ResourceHelper>();

	static ResourceHelper()
	{
		RuntimeHelpers.RunClassConstructor(typeof(Global).TypeHandle);
		LoadResources(); // load _logMessages & _logDescriptions
		LoadParameters(); // _parameters
		LoadMethodInfos(); // _methodInfos
	}

	internal static string GetMessage(ResourceType resourceType, bool noLogs=false)
	{
		// Code size: 25 (0x19)
		var resourceTypeId = (int)resourceType;
		if (_logMessages.TryGetValue(resourceTypeId, out var message)) return message;
		return string.Empty;
	}
	internal static string? GetDescription(ResourceType resourceType)
	{
		// Code size: 21 (0x15)
		var resourceTypeId = (int)resourceType;
		if (_logDescriptions.TryGetValue(resourceTypeId, out var description)) return description;
		return null;
	}
	internal static string? GetMethodInfo(ResourceType resourceType)
	{
		// Code size: 21 (0x15)
		var resourceTypeId = (int)resourceType;
		if (_methodInfos.TryGetValue(resourceTypeId, out var methodInfo)) return methodInfo;
		return null;
	}
	internal static Parameter GetParameter(ParameterType parameterType)
	{
		// Code size: 59 (0x3b)
		var parameterTypeId = (int)parameterType;
		if (_parameters.TryGetValue(parameterTypeId, out var parameter)) return parameter;
		throw new ArgumentException(string.Format(DefaultCulture, GetMessage(ResourceType.WrongParameterType), parameterType.ToString()));
	}
	internal static HashSet<string> GetReservedWords(DatabaseProvider databaseProvider)
	{
		// Code size: 332 (0x14c)
		var resourceFile = string.Empty;
		switch (databaseProvider)
		{
			case DatabaseProvider.Oracle: resourceFile =  ResourceType.OracleReservedKeyWord.ToString(); break; 
			case DatabaseProvider.PostgreSql: resourceFile = ResourceType.PostgreSQLReservedKeyWord.ToString(); break;
			case DatabaseProvider.MySql: resourceFile = ResourceType.MySQLReservedKeyWord.ToString(); break;
			case DatabaseProvider.SqlServer: resourceFile = ResourceType.SQLServerReservedKeyWord.ToString(); break;
			case DatabaseProvider.SqlLite: resourceFile = ResourceType.SQLiteReservedKeyWord.ToString(); break;
		};
		using var csv = new CsvHelper(ResourceNameSpace, resourceFile + CompressedResourceSuffix, 1);
		var reservedWords = new List<string?>();
		foreach (var reservedWord in csv) reservedWords.Add(reservedWord[0]);
		var result = new HashSet<string>(reservedWords.Count * 2, StringComparer.OrdinalIgnoreCase);
		foreach (var reservedWord in reservedWords) if (reservedWord is not null && !result.Contains(reservedWord)) result.Add(reservedWord);
		return result;
	}

	#region private methods

	private static void LoadResources()
	{
		// Code size: 474 (0x1da)
		lock (SyncRoot)
		{
			if (!_resourcesLoaded)
			{
				using var csv = new CsvHelper(ResourceNameSpace, ResourceType.LogMessage + CompressedResourceSuffix, 3);
				var strResourceEof = ResourceEndOfLine.ToString();
				var messages = new List<(int, string)>();
				var descriptions = new List<(int,string)>();
				foreach (var resource in csv)
				{
					if (!int.TryParse(resource[0]?.Trim()??string.Empty, NumberStyles.None, CultureInfo.InvariantCulture, out var id)) continue;
					if (resource[1] is not null) messages.Add((id, resource[1]?? string.Empty));
					if (resource[2] is not null) descriptions.Add((id, resource[2] ?? string.Empty));
				}
				var msgResult = new Dictionary<int, string>(messages.Count*2);
				var descResult = new Dictionary<int, string>(descriptions.Count*2);
				
				foreach (var message in messages) msgResult.TryAdd(message.Item1, message.Item2.Replace(ResourceEof, strResourceEof, StringComparison.Ordinal));
				foreach (var description in descriptions) descResult.TryAdd(description.Item1, description.Item2.Replace(ResourceEof, strResourceEof, StringComparison.Ordinal));

				_logMessages = msgResult;
				_logDescriptions = descResult;
				_resourcesLoaded = true;
			}
		}
	}

	private static void LoadParameters()
	{
		// Code size: 332 (0x14c)
		lock (SyncRoot)
		{
			if (!_parameterLoaded)
			{
				var parameters = new List<Parameter>();
				using var csv = new CsvHelper(ResourceNameSpace, EntityType.Parameter.ToString() + CompressedResourceSuffix, 7);
				foreach (var param in csv)
				{
					if (!int.TryParse(param[0], NumberStyles.None, CultureInfo.InvariantCulture, out var id)) continue;
					var paramType = id.ToParameterType();
					if (paramType == ParameterType.Undefined)  continue;
					parameters.Add(new Parameter(id, param[1] ?? string.Empty, param[2], paramType, param[3].ToFieldType(), param[6] ?? string.Empty, param[5], 0, param[4].ToEntityType(), true, true));
				}
				_parameters = new Dictionary<int, Parameter>(parameters.Count * 2);
				foreach (var param in parameters) _parameters.TryAdd((int)param.Type, param);
			}
			_parameterLoaded = true;
		}
	}

	private static void LoadMethodInfos()
	{
		// Code size: 274 (0x112)
		lock (SyncRoot)
		{
			if (!_methodInfoLoaded)
			{
				var methodInfos = new List<(int,string?)>();
				using var csv = new CsvHelper(ResourceNameSpace, ResourceType.MethodInfo.ToString() + CompressedResourceSuffix, 2);
				foreach (var methodInfo in csv)
				{
					if (!int.TryParse(methodInfo[0], NumberStyles.None, CultureInfo.InvariantCulture, out var id)) continue;
					methodInfos.Add((id, methodInfo[1]));
				}
				_methodInfos = new Dictionary<int, string>(methodInfos.Count * 2);
				foreach (var method in methodInfos) _methodInfos.TryAdd(method.Item1, method.Item2 ?? string.Empty);
			}
			_methodInfoLoaded = true;
		}
	}

	#endregion

}
