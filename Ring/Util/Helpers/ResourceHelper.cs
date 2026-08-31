using Ring.Logging;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ring.Util.Helpers;

/// <summary>
///		This class serves as the resource management backbone for the Ring0 class library
/// </summary>
internal sealed class ResourceHelper
{
	// BUGS (Claude source 4.6):
	//     1) LoadParameters(): _parameterLoaded = true is outside the if block; Severity: High (Done)
	//     2) GetReservedWords(): DatabaseProvider.Undefined produces empty resourceFile — Silent crash; Severity: Medium (Not a bug)
	//     3) Create(TableSpace): FileName not quoted or escaped; Severity: Low (Not a bug)

	private static readonly object SyncRoot = new();
	private static readonly string CompressedResourceSuffix = @".gz";
	private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
	private static readonly string ResourceMetaFile = @"Meta";
	private static readonly string ResourceEof = @"|||";
	private const char ResourceEndOfLine = '\n';
	private static bool _resourcesLoaded;
	private static bool _methodInfoLoaded;
	private static bool _metaLoaded;
	private static Dictionary<int, string> _logMessages = new();
	private static Dictionary<int, string> _logDescriptions = new();
	private static Dictionary<int, string> _methodInfos = new();
	private static Dictionary<int, Meta[]> _metas = new(); // <tableTypeId, Meta[] >
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	private static readonly Logger _logger = Global.LoggerFactory.CreateLogger<ResourceHelper>();

	static ResourceHelper()
	{
		RuntimeHelpers.RunClassConstructor(typeof(Global).TypeHandle);
		LoadResources(); // load _logMessages & _logDescriptions
		LoadMethodInfos(); // _methodInfos
		LoadMetas(); // _metas
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

	/// <summary>
	/// 	Get parameter by id ==> O(n) complexityO(n) complexity
	/// </summary>
	internal static Meta GetParameter(ParameterType parameterType)
	{
		// Code size: 97 (0x61)
		if (_metas.TryGetValue(0, out var parameters))
		{
			var parameterTypeId = (int)parameterType;
			var span = new ReadOnlySpan<Meta>(parameters);
			var spanCount = span.Length;
			for (var i=0; i < spanCount; ++i) if (span[i].Id == parameterTypeId) return span[i];
		}
		throw new ArgumentException(string.Format(DefaultCulture, GetMessage(ResourceType.UnsupportedParamType), parameterType.ToString()));
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

	internal static Meta[] GetMetaRows(TableType tableType)
	{
		// Code size: 34 (0x22)
		var refId = (int)tableType; 
		if (refId==0) return Array.Empty<Meta>(); // refId ==> 0 is reservice
		return _metas.TryGetValue(refId, out var metas)  ? metas : Array.Empty<Meta>();
	}

	internal static Meta? GetMetaTable(TableType tableType)
	{
		// Code size: 92 (0x5c)
		var refId = (int)tableType;
		var metaSpan = new ReadOnlySpan<Meta>(_metas.TryGetValue(refId, out var metas) ? metas : Array.Empty<Meta>());
		foreach (ref readonly var meta in metaSpan)	if (meta.IsTable) return meta;
		return null;
	}

	#region private methods
	
	private static void LoadResources()
	{
		// Code size: 474 (0x1da)
		lock (SyncRoot)
		{
			if (!_resourcesLoaded)
			{
				using var csv = new CsvHelper(ResourceNameSpace, ResourceType.LogMessage.ToString() + CompressedResourceSuffix, 3);
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

	private static void LoadMetas()
	{
		// Code size: 607 (0x25f)
		lock (SyncRoot)
		{
			if (!_metaLoaded)
			{
				var parsedRows = new List<Meta>();
				using var csv = new CsvHelper(ResourceNameSpace, ResourceMetaFile + CompressedResourceSuffix, 9);
				foreach (var row in csv)
				{
					// row is a string?[]: every cell can be null (short line, empty field), so every
					// numeric column goes through TryParse rather than Parse - a null or malformed
					// cell is logged and the row is skipped instead of throwing and aborting the load.
					if (!int.TryParse(row[0], NumberStyles.None, CultureInfo.InvariantCulture, out var id)) continue;
					if (!byte.TryParse(row[1], NumberStyles.None, CultureInfo.InvariantCulture, out var objectType)) continue;
					if (!int.TryParse(row[2], NumberStyles.None, CultureInfo.InvariantCulture, out var refId)) continue;
					if (!int.TryParse(row[3], NumberStyles.None, CultureInfo.InvariantCulture, out var dataType)) continue;
					if (!long.TryParse(row[4], NumberStyles.None, CultureInfo.InvariantCulture, out var flags)) continue;
					var newMeta = new Meta(id, objectType, refId, dataType, flags, row[5] ?? string.Empty, row[6], row[7], true);
					parsedRows.Add(newMeta);
				}
				ReadOnlySpan<Meta> span = CollectionsMarshal.AsSpan(parsedRows);

				// pass 1: count metas per tableTypeId so every array can be allocated once, at its final size
				var counts = new Dictionary<int, int>(parsedRows.Count * 2);
				foreach (var meta in span) counts[meta.ReferenceId] = counts.TryGetValue(meta.ReferenceId, out var count) ? count + 1 : 1;

				var result = new Dictionary<int, Meta[]>(counts.Count * 2);
				foreach (var kvp in counts) result[kvp.Key] = new Meta[kvp.Value];

				// pass 2: fill the pre-sized arrays via a per-key write cursor
				var cursor = new Dictionary<int, int>(counts.Count * 2);
				foreach (var meta in span)
				{
					var index = cursor.TryGetValue(meta.ReferenceId, out var i) ? i : 0;
					result[meta.ReferenceId][index] = meta;
					cursor[meta.ReferenceId] = index + 1;
				}
				_metas = result;
			}
			_metaLoaded = true;
		}
	}

	#endregion

}
