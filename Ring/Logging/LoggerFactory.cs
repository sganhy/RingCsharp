using System.Collections.Concurrent;

namespace Ring.Logging;

/// <summary>
/// Factory for creating Logger instances and managing subscriptions.
/// </summary>
internal sealed class LoggerFactory : IDisposable
{
	private readonly LogSubscriptionService _subscriptionService;
	private readonly ConcurrentDictionary<string, Logger> _loggers;
	private readonly ConcurrentDictionary<string, LogLevel> _categoryLevels;
	private readonly LogLevel _defaultMinLevel;
	private int _disposed;

	public LoggerFactory(LogLevel minLevel = LogLevel.Information)
	{
		_defaultMinLevel = minLevel;
		_subscriptionService = new LogSubscriptionService();
		_loggers = new ConcurrentDictionary<string, Logger>();
		_categoryLevels = new ConcurrentDictionary<string, LogLevel>();
		_disposed = 0;
	}

	internal LogSubscriptionService SubscriptionService => _subscriptionService;
	internal int LoggerCount => _loggers.Count;
	internal int SubscriptionCount => _subscriptionService.SubscriptionCount;

	internal Logger CreateLogger<T>()
	{
		var categoryName = typeof(T).FullName ?? typeof(T).Name;
		return GetOrCreateLogger(categoryName);
	}

	internal Logger CreateLogger(string categoryName) => GetOrCreateLogger(categoryName);

	/// <summary>
	/// Initializes the static Loggers container with this factory's subscription service.
	/// </summary>
	internal LoggerFactory InitializeLoggers()
	{
		Loggers.Initialize(_subscriptionService, _defaultMinLevel);
		return this;
	}

	private Logger GetOrCreateLogger(string categoryName)
	{
		return _loggers.GetOrAdd(categoryName, name =>
			new Logger(name, GetLevelForCategory(name), _subscriptionService));
	}

	public LoggerFactory AddFilter(string categoryName, LogLevel level)
	{
		_categoryLevels[categoryName] = level;
		return this;
	}

	public LoggerFactory AddFilter<T>(LogLevel level)
	{
		var categoryName = typeof(T).FullName ?? typeof(T).Name;
		return AddFilter(categoryName, level);
	}

	public LoggerFactory SetMinimumLevel(LogLevel level)
	{
		_categoryLevels[string.Empty] = level;
		return this;
	}

	private LogLevel GetLevelForCategory(string categoryName)
	{
		if (_categoryLevels.TryGetValue(categoryName, out var level)) return level;

		foreach (var kvp in _categoryLevels)
		{
			if (string.IsNullOrEmpty(kvp.Key)) continue;
			if (categoryName.StartsWith(kvp.Key + ".", StringComparison.Ordinal) ||
				categoryName.StartsWith(kvp.Key + "+", StringComparison.Ordinal))
				return kvp.Value;
		}

		return _categoryLevels.TryGetValue(string.Empty, out var globalLevel) ? globalLevel : _defaultMinLevel;
	}

	public IDisposable Subscribe(ILogSubscriber subscriber) => _subscriptionService.Subscribe(subscriber);

	public IDisposable Subscribe(Action<LogEvent> handler, LogLevel minLevel = LogLevel.Trace, string? categoryFilter = null)
		=> _subscriptionService.Subscribe(handler, minLevel, categoryFilter);

	public IDisposable AddConsoleSubscriber(LogLevel minLevel = LogLevel.Information)
		=> Subscribe(new ConsoleLogSubscriber(minLevel));

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
		_subscriptionService.Dispose();
		_loggers.Clear();
		_categoryLevels.Clear();
	}

	private sealed class ConsoleLogSubscriber : ILogSubscriber
	{
		public LogLevel MinLevel { get; }
		public string? CategoryFilter => null;

		internal ConsoleLogSubscriber(LogLevel minLevel) => MinLevel = minLevel;

		public void OnLogEvent(LogEvent logEvent)
		{
			var color = logEvent.Level switch
			{
				LogLevel.Trace => ConsoleColor.Gray,
				LogLevel.Debug => ConsoleColor.DarkGray,
				LogLevel.Information => ConsoleColor.White,
				LogLevel.Warning => ConsoleColor.Yellow,
				LogLevel.Error => ConsoleColor.Red,
				LogLevel.Critical => ConsoleColor.DarkRed,
				_ => ConsoleColor.White
			};

			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(logEvent.ToString());
			Console.ForegroundColor = originalColor;
		}
	}
}
