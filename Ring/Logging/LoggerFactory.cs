namespace Ring.Logging;

internal sealed class LoggerFactory 
{
	private readonly LogSubscriptionService _subscriptionService;
	private readonly LogLevel _minLevel;

	public LoggerFactory(LogLevel minLevel = LogLevel.Information)
	{
		_minLevel = minLevel;
		_subscriptionService = new LogSubscriptionService();
	}

	
	public LoggerFactory AddFilter(string categoryName, LogLevel level)
	{
		
		return this;
	}

	internal Logger CreateLogger<T>()
	{
		var categoryName = typeof(T).FullName ?? typeof(T).Name;
		return CreateLogger(categoryName, _minLevel, _subscriptionService);
	}

	private LogLevel GetLevelForCategory(string categoryName)
	{
		/*
		if (_categoryLevels.TryGetValue(categoryName, out var level))
			return level;

		foreach (var kvp in _categoryLevels)
		{
			if (categoryName.StartsWith(kvp.Key + ".") || categoryName.StartsWith(kvp.Key + "+"))
				return kvp.Value;
		}
		*/
		return _minLevel;
	}

	private static Logger CreateLogger(string categoryName, LogLevel minLevel, LogSubscriptionService subscriptionService)
	{
		return new Logger(categoryName, minLevel, subscriptionService);
	}

}
