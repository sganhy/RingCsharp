using Microsoft.Extensions.Logging;

namespace Ring.Data;

public interface IConfiguration
{
    string? ConnectionString { get; }
    ILoggerFactory LoggerFactory { get; }
    ILogger? SqlLogger { get; }
}
