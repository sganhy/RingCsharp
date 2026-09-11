using System.Runtime.CompilerServices;

namespace Ring.Logging;

/// <summary>
/// Logger implementation that publishes log events to subscribers via the LogSubscriptionService.
/// Each logger instance is associated with a category (typically the type name) and a minimum log level.
/// Captures the caller's class and namespace at creation time for AOT-friendly logging.
/// </summary>
internal sealed class Logger : ILogger
{
	private readonly string _categoryName;
	private readonly LogLevel _minLevel;
	private readonly LogSubscriptionService _subscriptionService;
	private readonly string _callerClass;
	private readonly string _callerNamespace;

	internal Logger(string categoryName, LogLevel minLevel, LogSubscriptionService subscriptionService)
	{
		_categoryName = categoryName;
		_minLevel = minLevel;
		_subscriptionService = subscriptionService;
		_callerClass = string.Empty;
		_callerNamespace = string.Empty;
	}

	/// <summary>
	/// Creates a logger with explicit caller class and namespace (from typeof(T) at compile time).
	/// </summary>
	internal Logger(string categoryName, LogLevel minLevel, LogSubscriptionService subscriptionService, string callerClass, string callerNamespace)
	{
		_categoryName = categoryName;
		_minLevel = minLevel;
		_subscriptionService = subscriptionService;
		_callerClass = callerClass;
		_callerNamespace = callerNamespace;
	}

	/// <summary>
	/// Gets the category name for this logger.
	/// </summary>
	internal string CategoryName => _categoryName;

	/// <summary>
	/// Gets the minimum log level for this logger.
	/// </summary>
	internal LogLevel MinLevel => _minLevel;

	/// <summary>
	/// Gets the caller's class name (captured at logger creation time).
	/// </summary>
	internal string CallerClass => _callerClass;

	/// <summary>
	/// Gets the caller's namespace (captured at logger creation time).
	/// </summary>
	internal string CallerNamespace => _callerNamespace;

	/// <summary>
	/// Returns true if there are any subscribers. Use to skip log computation when no one is listening.
	/// </summary>
	internal bool HasSubscribers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _subscriptionService.HasSubscribers;
	}

	/// <summary>
	/// Checks if the specified log level is enabled for this logger.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel && logLevel != LogLevel.None;

	/// <summary>
	/// Logs a message at the specified level with method info and line number.
	/// </summary>
	internal void Log(LogLevel logLevel, string methodName, string message, string? description, int lineNumber)
	{
		if (!IsEnabled(logLevel)) return;

		var logEvent = new LogEvent(
			logLevel,
			_categoryName,
			message,
			description,
			exception: null,
			schemaId: null,
			jobId: null,
			method: methodName,
			lineNumber: lineNumber,
			callSite: null);

		_subscriptionService.Publish(logEvent, _categoryName);
	}

	/// <summary>
	/// Logs a message at the specified level with an optional exception.
	/// </summary>
	public void Log(LogLevel logLevel, string message, Exception? exception = null)
	{
		if (!IsEnabled(logLevel)) return;

		var logEvent = new LogEvent(
			logLevel,
			_categoryName,
			message,
			description: null,
			exception: exception);

		_subscriptionService.Publish(logEvent, _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at the specified level.
	/// </summary>
	public void Log(LogLevel logLevel, string message, params object?[] args)
	{
		if (!IsEnabled(logLevel)) return;

		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;

		var logEvent = new LogEvent(
			logLevel,
			_categoryName,
			formattedMessage);

		_subscriptionService.Publish(logEvent, _categoryName);
	}

	/// <summary>
	/// Logs a pre-built log event.
	/// </summary>
	internal void Log(LogEvent logEvent)
	{
		if (!IsEnabled(logEvent.Level)) return;
		_subscriptionService.Publish(logEvent, _categoryName);
	}

	/// <summary>
	/// Logs a message at Trace level.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void LogTrace(string message)
	{
		if (!IsEnabled(LogLevel.Trace)) return;
		_subscriptionService.Publish(LogEvent.Trace(_categoryName, message), _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at Trace level.
	/// </summary>
	internal void LogTrace(string message, params object?[] args)
	{
		if (!IsEnabled(LogLevel.Trace)) return;
		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
		_subscriptionService.Publish(LogEvent.Trace(_categoryName, formattedMessage), _categoryName);
	}

	/// <summary>
	/// Logs a message at Debug level.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void LogDebug(string message)
	{
		if (!IsEnabled(LogLevel.Debug)) return;
		_subscriptionService.Publish(LogEvent.Debug(_categoryName, message), _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at Debug level.
	/// </summary>
	internal void LogDebug(string message, params object?[] args)
	{
		if (!IsEnabled(LogLevel.Debug)) return;
		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
		_subscriptionService.Publish(LogEvent.Debug(_categoryName, formattedMessage), _categoryName);
	}

	/// <summary>
	/// Logs a message at Information level.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void LogInformation(string message)
	{
		if (!IsEnabled(LogLevel.Information)) return;
		_subscriptionService.Publish(LogEvent.Information(_categoryName, message), _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at Information level.
	/// </summary>
	internal void LogInformation(string message, params object?[] args)
	{
		if (!IsEnabled(LogLevel.Information)) return;
		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
		_subscriptionService.Publish(LogEvent.Information(_categoryName, formattedMessage), _categoryName);
	}

	/// <summary>
	/// Logs a message at Warning level.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void LogWarning(string message)
	{
		if (!IsEnabled(LogLevel.Warning)) return;
		_subscriptionService.Publish(LogEvent.Warning(_categoryName, message), _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at Warning level.
	/// </summary>
	internal void LogWarning(string message, params object?[] args)
	{
		if (!IsEnabled(LogLevel.Warning)) return;
		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
		_subscriptionService.Publish(LogEvent.Warning(_categoryName, formattedMessage), _categoryName);
	}

	/// <summary>
	/// Logs a message at Error level.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void LogError(string message)
	{
		if (!IsEnabled(LogLevel.Error)) return;
		_subscriptionService.Publish(LogEvent.Error(_categoryName, message), _categoryName);
	}

	/// <summary>
	/// Logs a message with exception at Error level.
	/// </summary>
	internal void LogError(Exception exception, string message)
	{
		if (!IsEnabled(LogLevel.Error)) return;
		_subscriptionService.Publish(LogEvent.Error(_categoryName, message, exception), _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at Error level.
	/// </summary>
	internal void LogError(string message, params object?[] args)
	{
		if (!IsEnabled(LogLevel.Error)) return;
		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
		_subscriptionService.Publish(LogEvent.Error(_categoryName, formattedMessage), _categoryName);
	}

	/// <summary>
	/// Logs a message at Critical level.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void LogCritical(string message)
	{
		if (!IsEnabled(LogLevel.Critical)) return;
		_subscriptionService.Publish(LogEvent.Critical(_categoryName, message), _categoryName);
	}

	/// <summary>
	/// Logs a message with exception at Critical level.
	/// </summary>
	internal void LogCritical(Exception exception, string message)
	{
		if (!IsEnabled(LogLevel.Critical)) return;
		_subscriptionService.Publish(LogEvent.Critical(_categoryName, message, exception), _categoryName);
	}

	/// <summary>
	/// Logs a formatted message at Critical level.
	/// </summary>
	internal void LogCritical(string message, params object?[] args)
	{
		if (!IsEnabled(LogLevel.Critical)) return;
		var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
		_subscriptionService.Publish(LogEvent.Critical(_categoryName, formattedMessage), _categoryName);
	}
}
