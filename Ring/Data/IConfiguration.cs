using Microsoft.Extensions.Logging;

namespace Ring.Data;

public interface IConfiguration
{
    string? ConnectionString { get; }
    ILoggerFactory LoggerFactory { get; }
    ILogger? SqlLogger { get; }

    /// <summary>
    /// Minimum number of Connections a pool will maintain at any given time. 
    /// </summary>
    int MinConnectionPoolSize { get; }

    /// <summary>
    /// Maximum number of Connections a pool will maintain at any given time.
    /// </summary>
    int MaxConnectionPoolSize { get; }

    string DefaultSchema { get; } 
    
    /// <summary>
    /// Default tablespace for constraint + index of meta tables.
    /// </summary>
    string? DefaultIndexStorage { get; }

    /// <summary>
    /// Default tablespace for meta tables.
    /// </summary>
    string? DefaultTableStorage { get; }

}
