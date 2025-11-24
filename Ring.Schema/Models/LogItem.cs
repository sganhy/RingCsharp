using Microsoft.Extensions.Logging;

namespace Ring.Schema.Models;

internal sealed class LogItem
{
	internal readonly int Id;
	internal readonly int lineNumber;
	internal readonly string Name;
	internal readonly string Description;
	internal readonly LogLevel Level;

	internal LogItem(int id, int lineNumber, string name, string description, LogLevel level)
    {
        Id = id;
        this.lineNumber = lineNumber;
        Name = name;
        Description = description;
        Level = level;
    }
}
