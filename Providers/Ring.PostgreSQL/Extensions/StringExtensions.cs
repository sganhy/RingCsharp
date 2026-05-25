using Ring.Data.Models;
using Ring.Schema.Enums;

namespace Ring.PostgreSQL.Extensions;

internal static class StringExtensions
{
    //TODO unit test !!!!!!!!!!
    internal static string DateTimeValue(this string value)
    {
        // Code size: 100 (0x64)
        // eg. '2024-12-14T23:48:52.171Z' replace to '2024-12-14 23:48:52.171'
        var span = value.AsSpan();
        var timeZoneIndicator = span[^1] == 'Z' ? 1 : 0;
        var count = span.Length - timeZoneIndicator;
        var result = new char[count];
        for (var i = 0; i < count; ++i)
            result[i] = span[i] == 'T' ? ' ' : span[i];
        return new string(result);
    }

	internal static ConnectionParameters ToConnectionParameters(this string value)
	{
        var host = string.Empty; 
		var port = 5432; // default port!
		var userName = string.Empty;
		var password = string.Empty;
		var timeOut = 30000; // 30 seconds
        var result = new ConnectionParameters(DatabaseProvider.PostgreSql, host, port, userName, password, timeOut);
		return result;
	}

	#region private methods

    #endregion 

}
