using Microsoft.Extensions.Logging;

namespace Ring.Schema;

public sealed class ValidationItem
{
	public readonly long Id;
	public readonly long LineNumber;
	public readonly DateTime ValidationTime;
	public readonly string Name;
	public readonly string Description;
	public readonly LogLevel Level;

	/// <summary>
	///     Ctor
	/// </summary>
	public ValidationItem(long id, long lineNumber, string name, string description, LogLevel level)
	{
		Id = id;
		LineNumber = lineNumber;
		Name = name;
		Description = description;
		Level = level;
		ValidationTime = DateTime.Now;
	}

#if DEBUG
	public override string ToString() => "name= " + Name + "; desc= " + Description + "; line= " + LineNumber;
#endif
}
