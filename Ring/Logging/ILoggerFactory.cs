namespace Ring.Logging;

internal interface ILoggerFactory
{
	ILogger<T> CreateLogger<T>();
}
