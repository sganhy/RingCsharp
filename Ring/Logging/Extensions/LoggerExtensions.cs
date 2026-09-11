using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Logging.Extensions;

/// <summary>
/// Extension methods for Logger that provide convenient logging methods
/// with automatic caller information and resource-based message formatting.
/// </summary>
internal static class LoggerExtensions
{
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	#region Trace level extensions

	/// <summary>
	/// Logs a trace message with optional format arguments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void LogTrace(this Logger logger, string message, params object?[] args)
		=> logger.Log(LogLevel.Trace, message, args);

	/// <summary>
	/// Logs a trace message using a resource type for localized messages.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogTrace(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null,
		[CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
		=> LogWithResource(LogLevel.Trace, logger, resourceType, lineNumber, memberName, param1, param2, null, null);

	#endregion

	#region Debug level extensions

	/// <summary>
	/// Logs a debug message using a resource type for localized messages.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogDebug(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null,
		[CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
		=> LogWithResource(LogLevel.Debug, logger, resourceType, lineNumber, memberName, param1, param2, null, null);

	/// <summary>
	/// Logs a debug message with caller information.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogDebugWithCaller(this Logger logger, string message,
		[CallerMemberName] string? memberName = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int lineNumber = 0)
	{
		if (!logger.IsEnabled(LogLevel.Debug)) return;

		var logEvent = new LogEvent(
			LogLevel.Debug,
			logger.CategoryName,
			message,
			description: null,
			exception: null,
			schemaId: null,
			jobId: null,
			method: memberName,
			lineNumber: lineNumber,
			callSite: filePath);

		logger.Log(logEvent);
	}

	#endregion

	#region Information level extensions

	/// <summary>
	/// Logs an information message using a resource type for localized messages.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogInformation(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null,
		[CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
		=> LogWithResource(LogLevel.Information, logger, resourceType, lineNumber, memberName, param1, param2, null, null);

	/// <summary>
	/// Logs an information message with caller information.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogInformationWithCaller(this Logger logger, string message,
		[CallerMemberName] string? memberName = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int lineNumber = 0)
	{
		if (!logger.IsEnabled(LogLevel.Information)) return;

		var logEvent = new LogEvent(
			LogLevel.Information,
			logger.CategoryName,
			message,
			description: null,
			exception: null,
			schemaId: null,
			jobId: null,
			method: memberName,
			lineNumber: lineNumber,
			callSite: filePath);

		logger.Log(logEvent);
	}

	#endregion

	#region Warning level extensions

	/// <summary>
	/// Logs a warning message using a resource type for localized messages.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogWarning(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null,
		[CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
		=> LogWithResource(LogLevel.Warning, logger, resourceType, lineNumber, memberName, param1, param2, null, null);

	/// <summary>
	/// Logs a warning message with caller information.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogWarningWithCaller(this Logger logger, string message,
		[CallerMemberName] string? memberName = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int lineNumber = 0)
	{
		if (!logger.IsEnabled(LogLevel.Warning)) return;

		var logEvent = new LogEvent(
			LogLevel.Warning,
			logger.CategoryName,
			message,
			description: null,
			exception: null,
			schemaId: null,
			jobId: null,
			method: memberName,
			lineNumber: lineNumber,
			callSite: filePath);

		logger.Log(logEvent);
	}

	#endregion

	#region Error level extensions

	/// <summary>
	/// Logs an error message using a resource type for localized messages.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogError(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null,
		[CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
		=> LogWithResource(LogLevel.Error, logger, resourceType, lineNumber, memberName, param1, param2, null, null);

	/// <summary>
	/// Logs an error with exception and message.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogError(this Logger logger, Exception exception, string message,
		[CallerMemberName] string? memberName = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int lineNumber = 0)
	{
		if (!logger.IsEnabled(LogLevel.Error)) return;

		var logEvent = new LogEvent(
			LogLevel.Error,
			logger.CategoryName,
			message,
			description: null,
			exception: exception,
			schemaId: null,
			jobId: null,
			method: memberName,
			lineNumber: lineNumber,
			callSite: filePath);

		logger.Log(logEvent);
	}

	/// <summary>
	/// Logs an error with exception and formatted message.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogError(this Logger logger, Exception exception, string message, params object?[] args)
	{
		if (!logger.IsEnabled(LogLevel.Error)) return;

		var formattedMessage = args.Length > 0 ? string.Format(DefaultCulture, message, args) : message;

		var logEvent = new LogEvent(
			LogLevel.Error,
			logger.CategoryName,
			formattedMessage,
			description: null,
			exception: exception);

		logger.Log(logEvent);
	}

	#endregion

	#region Critical level extensions

	/// <summary>
	/// Logs a critical message with optional format arguments.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogCritical(this Logger logger, string message, params object?[] args)
		=> logger.Log(LogLevel.Critical, message, args);

	/// <summary>
	/// Logs a critical message using a resource type for localized messages.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogCritical(this Logger logger, ResourceType resourceType, string? param1 = null, string? param2 = null,
		[CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
		=> LogWithResource(LogLevel.Critical, logger, resourceType, lineNumber, memberName, param1, param2, null, null);

	/// <summary>
	/// Logs a critical error with exception and message.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LogCritical(this Logger logger, Exception exception, string message,
		[CallerMemberName] string? memberName = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int lineNumber = 0)
	{
		if (!logger.IsEnabled(LogLevel.Critical)) return;

		var logEvent = new LogEvent(
			LogLevel.Critical,
			logger.CategoryName,
			message,
			description: null,
			exception: exception,
			schemaId: null,
			jobId: null,
			method: memberName,
			lineNumber: lineNumber,
			callSite: filePath);

		logger.Log(logEvent);
	}

	#endregion

	#region Scoped logging

	/// <summary>
	/// Creates a scoped log context that logs entry and exit of a method.
	/// </summary>
	internal static IDisposable BeginScope(this Logger logger, string scopeName,
		[CallerMemberName] string? memberName = null)
	{
		return new LogScope(logger, scopeName, memberName);
	}

	#endregion

	#region private methods

	private static void LogWithResource(LogLevel logLevel, Logger logger, ResourceType resourceType, int lineNumber,
		string? memberName, string? param1, string? param2, string? param3, string? param4)
	{
		if (!logger.IsEnabled(logLevel)) return;

		var message = ResourceHelper.GetMessage(resourceType, noLogs: true); // no logs here to avoid recursion
		var methodInfo = ResourceHelper.GetMethodInfo(resourceType) ?? memberName ?? string.Empty;
		var description = ResourceHelper.GetDescription(resourceType);

		// Format message with parameters
		if (param4 is not null)
			message = string.Format(DefaultCulture, message, param1, param2, param3, param4);
		else if (param3 is not null)
			message = string.Format(DefaultCulture, message, param1, param2, param3);
		else if (param2 is not null)
			message = string.Format(DefaultCulture, message, param1, param2);
		else if (param1 is not null)
			message = string.Format(DefaultCulture, message, param1);

		var logEvent = new LogEvent(
			logLevel,
			logger.CategoryName,
			message,
			description,
			exception: null,
			schemaId: null,
			jobId: null,
			method: methodInfo,
			lineNumber: lineNumber,
			callSite: null);

		logger.Log(logEvent);
	}

	#endregion

	#region nested types

	/// <summary>
	/// Represents a logging scope that logs entry and exit messages.
	/// </summary>
	private sealed class LogScope : IDisposable
	{
		private readonly Logger _logger;
		private readonly string _scopeName;
		private readonly string? _memberName;
		private readonly DateTime _startTime;
		private bool _disposed;

		internal LogScope(Logger logger, string scopeName, string? memberName)
		{
			_logger = logger;
			_scopeName = scopeName;
			_memberName = memberName;
			_startTime = DateTime.UtcNow;
			_disposed = false;

			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug("Entering scope: {0} in {1}", _scopeName, _memberName ?? "unknown");
			}
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;

			if (_logger.IsEnabled(LogLevel.Debug))
			{
				var elapsed = DateTime.UtcNow - _startTime;
				_logger.LogDebug("Exiting scope: {0} in {1} (elapsed: {2}ms)", _scopeName, _memberName ?? "unknown", elapsed.TotalMilliseconds.ToString("F2", DefaultCulture));
			}
		}
	}

	#endregion
}
