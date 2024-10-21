using Microsoft.Extensions.Logging;
using Ring.Data;

namespace Ring.PostgreSQL;

public sealed class Configuration : IConfiguration
{
    public string? ConnectionString { get ; set; }
    public ILogger Logger { get; set; } = new DefaultLogger();
    public ILogger? SqlLogger { get; set; }
}
