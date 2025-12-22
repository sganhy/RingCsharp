using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Logging.Extensions;

internal static class LoggerExtensions
{
	internal static void LogTrace(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Trace, message, args);

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogDebug(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null, [CallerLineNumber] int lineNumber = 0)
		=> Log(LogLevel.Debug, logger, resourceType, lineNumber, param1, param2, null, null);

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogInformation(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null, [CallerLineNumber] int lineNumber = 0)
		=> Log(LogLevel.Information, logger, resourceType, lineNumber, param1, param2, null, null);

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogWarning(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null, [CallerLineNumber] int lineNumber = 0)
		=> Log(LogLevel.Warning, logger, resourceType, lineNumber, param1, param2, null, null);

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogError(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null, [CallerLineNumber] int lineNumber = 0)
		=> Log(LogLevel.Error, logger, resourceType, lineNumber, param1, param2, null, null);

	internal static void LogError(this Logger logger, Exception exception, string message)
	{ }
	internal static void LogCritical(this Logger logger, string message, params object?[] args)
	{ }


	#region private methods 

	private static void Log(LogLevel loglevel, Logger logger, ResourceType resourceType, int lineNumber, string? param1, string? param2, string? param3, string? param4)
	{
		var message = ResourceHelper.GetMessage(resourceType, true); // no logs here to avoid recursion calls
		var methodInfo = ResourceHelper.GetMethodInfo(resourceType) ?? string.Empty;
		var description = ResourceHelper.GetDescription(resourceType);

		if (param4 is not null) message = string.Format(CultureInfo.InvariantCulture, message, param1, param2, param3, param4);
		else if (param3 is not null) message = string.Format(CultureInfo.InvariantCulture, message, param1, param2, param3);
		else if (param2 is not null) message = string.Format(CultureInfo.InvariantCulture, message, param1, param2);
		else if (param1 is not null) message = string.Format(CultureInfo.InvariantCulture, message, param1);

		logger.Log(loglevel, methodInfo, message, description, lineNumber);
	}

	#endregion
}
