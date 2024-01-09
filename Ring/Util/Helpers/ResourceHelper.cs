using Ring.Schema.Enums;
using Ring.Util.Enums;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;

namespace Ring.Util.Helpers;

internal sealed class ResourceHelper
{
    private static readonly object _syncRoot = new();
    private static readonly string ResourceSuffix = @".txt";
    private static readonly string CompressedRessourceSuffix = @".gz";
    private static readonly string ResourceCrLf = @"|||";
    private static readonly string ResourceNameSpace = @"Ring.Util.Resources.";
    private static readonly char ResourceEndOfLine = '\n';
    private static readonly char MessageDescSplitChar = '#';
    private static bool _ressourcesLoaded;
    private static string?[] _logMessages = Array.Empty<string?>();
    private static string?[] _logDescriptions = Array.Empty<string?>();

    internal ResourceHelper()
    {
        if (!_ressourcesLoaded) LoadRessources();
    }

    internal static string GetErrorMessage(ResourceType resourceType) {
        if (!_ressourcesLoaded) LoadRessources();
        string message = string.Empty;
        if ((int)resourceType <= _logMessages.Length) message= _logMessages[(int)resourceType - 1] ?? string.Empty;
        return message.Replace(ResourceCrLf, ResourceEndOfLine.ToString());
    }

#pragma warning disable CA1822

    internal string? GetMessage(LogType logType) 
        => ((int)logType <= _logMessages.Length) ? _logMessages[(int)logType - 1] : string.Empty;
    internal string? GetDescription(LogType logType)
        => ((int)logType <= _logDescriptions.Length) ? _logDescriptions[(int)logType - 1] : string.Empty;

    internal static string[] GetReservedWords(DatabaseProvider databaseProvider)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (databaseProvider)
        {
            case DatabaseProvider.Oracle:
                return GetCompressedResource(ResourceType.OracleReservedKeyWord.ToString() + CompressedRessourceSuffix, true, true);
            case DatabaseProvider.PostgreSql:
                return GetCompressedResource(ResourceType.PostgreSQLReservedKeyWord.ToString() + CompressedRessourceSuffix, true, true);
            case DatabaseProvider.MySql:
                return GetCompressedResource(ResourceType.MySQLReservedKeyWord.ToString() + CompressedRessourceSuffix, true, true);
            case DatabaseProvider.SqlServer:
                return GetCompressedResource(ResourceType.SQLServerReservedKeyWord.ToString() + CompressedRessourceSuffix, true, true);
            case DatabaseProvider.SqlLite:
                return GetCompressedResource(ResourceType.SQLiteReservedKeyWord.ToString() + CompressedRessourceSuffix, true, true);
        }
#pragma warning restore IDE0066
        return Array.Empty<string>();
    }

#pragma warning restore CA1822

    #region private methods

    private static void LoadRessources()
    {
        lock (_syncRoot)
        {
            if (!_ressourcesLoaded)
            {
                var ressourceFile = ResourceType.LogMessage.ToString() + ResourceSuffix;
                (_logMessages, _logDescriptions) = GetLogResource(ressourceFile);
            }
            _ressourcesLoaded = true;
        }
    }

    private static (string?[], string?[]) GetLogResource(string fileName)
    {
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
        // build description array
        if (resultMessage.Length > 0)
        {
            resultDesc = new string[resultMessage.Length];
            for (var i = 0; i < resultMessage.Length; ++i)
            {
                if (string.IsNullOrEmpty(resultMessage[i]))
                {
                    resultMessage[i] = null;
                    resultDesc[i] = null;
                }
                else 
                {
                    var message = resultMessage[i];
                    if (!string.IsNullOrEmpty(message))
                    {
                        var index = message.IndexOf(MessageDescSplitChar);
                        if (index >= 0)
                        {
                            resultMessage[i] = message.Substring(0, index);
                            resultDesc[i] = message.Substring(index + 1);
                            continue;
                        }
                    }
                    resultDesc[i] = message;
                }
            }
        }
        return (resultMessage, resultDesc);
    }

    private static string[] GetCompressedResource(string fileName, bool sortResult, bool toUpper)
    {
        var resource = ResourceNameSpace + fileName;
        var assembly = Assembly.GetExecutingAssembly();
        var result = Array.Empty<string>();
        using (var stream = assembly.GetManifestResourceStream(resource))
        {
            if (stream == null) return result;
            using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressionStream);
            result = toUpper ? 
                reader.ReadToEnd().ToUpper(CultureInfo.InvariantCulture).Split(ResourceEndOfLine) :
                reader.ReadToEnd().Split(ResourceEndOfLine);
        }
        // sort result 
        if (sortResult) Array.Sort(result);
        return result;
    }

    #endregion 

}
