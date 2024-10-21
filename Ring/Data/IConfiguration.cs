using Microsoft.Extensions.Logging;

namespace Ring.Data;

public interface IConfiguration
{
    string? ConnectionString { get; }
    ILogger Logger { get; }
    ILogger? SqlLogger { get; }
}
