namespace Ring.Logging;

internal sealed class Logger : ILogger
{
	private readonly string _categoryName;
	private readonly LogLevel _minLevel;
	private readonly LogSubscriptionService _subscriptionService;

	internal Logger(string categoryName, LogLevel minLevel, LogSubscriptionService subscriptionService)
	{
		_categoryName = categoryName;
		_minLevel = minLevel;
		_subscriptionService = subscriptionService;
	}


	public void Log(LogLevel logLevel, string message)
		=> Log(logLevel, null, message);

	public void Log(LogLevel logLevel, Exception? exception, string message)
	{
	
		//_subscriptionService.Publish(logEvent);
	}

	public void Log(LogLevel logLevel, string message, params object?[] args)
	{
		var frmMessage = string.Format(message, args);
		/*
		if (!IsEnabled(logLevel)) return;

		var logEvent = new LogEvent(logLevel, _categoryName, message, args: args);
		_subscriptionService.Publish(logEvent);
		*/
	}

}
