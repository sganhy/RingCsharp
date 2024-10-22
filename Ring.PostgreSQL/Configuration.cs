using Microsoft.Extensions.Logging;
using Ring.Data;

namespace Ring.PostgreSQL;

public sealed class Configuration : IConfiguration
{
    public string? ConnectionString { get ; set; }
    public ILoggerFactory LoggerFactory { get; set; } = new DefaultLoggerFactory();
    public ILogger? SqlLogger { get; set; }
}
