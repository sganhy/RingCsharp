namespace Ring.Logging;

/// <summary>
/// Static container for all specialized loggers.
/// Each logger targets a specific concern (SQL, Dump, Errors, etc.).
/// Initialize once at startup via Loggers.Initialize(subscriptionService).
/// </summary>
internal static class Loggers
{
	private static LogSubscriptionService? _subscriptionService;
	private static LogLevel _defaultLevel = LogLevel.Information;

	/// <summary>Logger for SQL statements (SELECT, INSERT, UPDATE, DELETE, DDL).</summary>
	internal static Logger Sql { get; private set; } = null!;

	/// <summary>Logger for data dump operations.</summary>
	internal static Logger Dump { get; private set; } = null!;

	/// <summary>Logger for errors and exceptions.</summary>
	internal static Logger Error { get; private set; } = null!;

	/// <summary>Logger for log table operations (@log writes).</summary>
	internal static Logger LogTable { get; private set; } = null!;

	/// <summary>Logger for schema operations (create, alter, drop).</summary>
	internal static Logger Schema { get; private set; } = null!;

	/// <summary>Logger for connection pool events.</summary>
	internal static Logger Pool { get; private set; } = null!;

	/// <summary>Logger for cache operations.</summary>
	internal static Logger Cache { get; private set; } = null!;

	/// <summary>Logger for job/background task operations.</summary>
	internal static Logger Job { get; private set; } = null!;

	/// <summary>Logger for performance/timing metrics.</summary>
	internal static Logger Perf { get; private set; } = null!;

	/// <summary>General purpose logger for miscellaneous events.</summary>
	internal static Logger General { get; private set; } = null!;

	/// <summary>
	/// Initializes all loggers. Call once at application startup.
	/// </summary>
	internal static void Initialize(LogSubscriptionService subscriptionService, LogLevel defaultLevel = LogLevel.Information)
	{
		_subscriptionService = subscriptionService;
		_defaultLevel = defaultLevel;

		Sql = new Logger("Ring.Sql", defaultLevel, subscriptionService);
		Dump = new Logger("Ring.Dump", defaultLevel, subscriptionService);
		Error = new Logger("Ring.Error", LogLevel.Error, subscriptionService); // Error logger defaults to Error level
		LogTable = new Logger("Ring.LogTable", defaultLevel, subscriptionService);
		Schema = new Logger("Ring.Schema", defaultLevel, subscriptionService);
		Pool = new Logger("Ring.Pool", defaultLevel, subscriptionService);
		Cache = new Logger("Ring.Cache", defaultLevel, subscriptionService);
		Job = new Logger("Ring.Job", defaultLevel, subscriptionService);
		Perf = new Logger("Ring.Perf", defaultLevel, subscriptionService);
		General = new Logger("Ring.General", defaultLevel, subscriptionService);
	}

	/// <summary>
	/// Initializes loggers from a LoggerFactory instance.
	/// </summary>
	internal static void Initialize(LoggerFactory factory)
	{
		Initialize(factory.SubscriptionService, LogLevel.Information);
	}

	/// <summary>
	/// Creates a logger with the specified category, capturing caller type info at compile time.
	/// Usage: private static readonly Logger _logger = Loggers.Create&lt;BulkAlter&gt;("Ring.Sql");
	/// </summary>
	internal static Logger Create<TCaller>(string categoryName, LogLevel? minLevel = null)
	{
		if (_subscriptionService is null)
			throw new InvalidOperationException("Loggers not initialized. Call Loggers.Initialize() first.");

		var callerType = typeof(TCaller);
		return new Logger(
			categoryName,
			minLevel ?? _defaultLevel,
			_subscriptionService,
			callerType.Name,
			callerType.Namespace ?? string.Empty);
	}

	/// <summary>
	/// Creates a logger using the caller type's full name as category.
	/// Usage: private static readonly Logger _logger = Loggers.Create&lt;BulkAlter&gt;();
	/// </summary>
	internal static Logger Create<TCaller>(LogLevel? minLevel = null)
	{
		if (_subscriptionService is null)
			throw new InvalidOperationException("Loggers not initialized. Call Loggers.Initialize() first.");

		var callerType = typeof(TCaller);
		var categoryName = callerType.FullName ?? callerType.Name;
		return new Logger(
			categoryName,
			minLevel ?? _defaultLevel,
			_subscriptionService,
			callerType.Name,
			callerType.Namespace ?? string.Empty);
	}

	/// <summary>
	/// Checks if loggers have been initialized.
	/// </summary>
	internal static bool IsInitialized => _subscriptionService is not null;
}
