namespace Ring.Logging.Extensions;

internal static class LoggerExtensions
{
	internal static void LogTrace(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Trace, message, args);
	internal static void LogDebug(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Debug, message, args);
	internal static void LogInformation(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Information, message, args);
	internal static void LogWarning(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Warning, message, args);
	internal static void LogWarning(this Logger logger, Exception exception, string message) => logger.Log(LogLevel.Warning, exception, message);
	internal static void LogError(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Error, message, args);
	internal static void LogError(this Logger logger, Exception exception, string message) => logger.Log(LogLevel.Error, exception, message);
	internal static void LogCritical(this Logger logger, string message, params object?[] args) => logger.Log(LogLevel.Critical, message, args);
}
