using Microsoft.Extensions.Logging;

namespace Ring.Data;

internal sealed class DefaultLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) {}

    public ILogger CreateLogger(string categoryName)
    {
        return null;
    }

    public void Dispose() {}
}
