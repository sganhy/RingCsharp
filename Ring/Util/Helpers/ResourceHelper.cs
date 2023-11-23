using Ring.Schema.Enums;
using Ring.Util.Enums;
using System.IO.Compression;
using System.Reflection;

namespace Ring.Util.Helpers;

internal sealed class ResourceHelper
{
    private static readonly object _syncRoot = new();
    private static readonly string RessourceSuffix = @".txt";
    private static readonly string CompressedRessourceSuffix = @".gz";
    private static readonly string RessourceNameSpace = @"Ring.Util.Resources.";
    private static readonly char RessourceEndOfLine = '\n';
    private static readonly char MessageDescSplitChar = '#';
    private static bool _ressourcesLoaded;
    private static string?[] _logMessages = Array.Empty<string?>();
    private static string?[] _logDescriptions = Array.Empty<string?>();

    internal ResourceHelper()
    {
        if (!_ressourcesLoaded) LoadRessources();
    }

#pragma warning disable CA1822

    internal string? GetMessage(LogType logType) => ((int)logType <= _logMessages.Length) ? _logMessages[(int)logType - 1] : string.Empty;
    internal string? GetDescription(LogType logType)
        => ((int)logType <= _logDescriptions.Length) ? _logDescriptions[(int)logType - 1] : string.Empty;

    internal string[] GetReservedWords(DatabaseProvider databaseProvider)
    {
        switch (databaseProvider)
        {
            case DatabaseProvider.PostgreSql:
                return GetCompressedResource(ResourceType.PostgreSQLReservedKeyWord.ToString() + CompressedRessourceSuffix);
            case DatabaseProvider.MySql:
                return GetCompressedResource(ResourceType.MySQLReservedKeyWord.ToString() + CompressedRessourceSuffix);
            case DatabaseProvider.SqlServer:
                return GetCompressedResource(ResourceType.SQLServerReservedKeyWord.ToString() + CompressedRessourceSuffix);
            case DatabaseProvider.SqlLite:
                return GetCompressedResource(ResourceType.SQLiteReservedKeyWord.ToString() + CompressedRessourceSuffix);
        }
        return Array.Empty<string>();
    }

#pragma warning restore CA1822


    private static void LoadRessources()
    {
        lock (_syncRoot)
        {
            if (!_ressourcesLoaded)
            {
                var ressourceFile = ResourceType.LogMessage.ToString() + RessourceSuffix;
                (_logMessages, _logDescriptions) = GetLogResource(ressourceFile);
            }
            _ressourcesLoaded = true;
        }
    }

    private static (string?[], string?[]) GetLogResource(string fileName)
    {
        var resultMessage = Array.Empty<string?>();
        var resultDesc = Array.Empty<string?>();
        var resource = RessourceNameSpace + fileName;
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resource))
        {
            if (stream == null) return (resultMessage, resultDesc);
            using var reader = new StreamReader(stream);
            resultMessage = reader.ReadToEnd().Split(RessourceEndOfLine);
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

    private static string[] GetCompressedResource(string fileName)
    {
        var resource = RessourceNameSpace + fileName;
        var assembly = Assembly.GetExecutingAssembly();
        var result = Array.Empty<string>();
        using (var stream = assembly.GetManifestResourceStream(resource))
        {
            if (stream == null) return result;
            using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressionStream);
            result = reader.ReadToEnd().Split(RessourceEndOfLine);
        }
        // sort result 
        Array.Sort(result);
        return result;
    }


}
