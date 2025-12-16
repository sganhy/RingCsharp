namespace Ring.Logging;

internal sealed class LoggerFactory : ILoggerFactory
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

	public ILogger<T> CreateLogger<T>()
	{
		var categoryName = typeof(T).FullName ?? typeof(T).Name;
		//var logger = CreateLogger(categoryName);
		return new Logger<T>();
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

	public static LoggerFactory Create(Action<LoggerFactory>? configure = null)
	{
		var factory = new LoggerFactory();
		configure?.Invoke(factory);
		return factory;
	}
}
