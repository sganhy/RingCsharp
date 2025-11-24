using Microsoft.Extensions.Logging;

namespace Ring.Schema.Models;

internal readonly struct ValidationItem
{
	internal readonly long Id;
	internal readonly long LineNumber;
	internal readonly DateTime ValidationTime;
	internal readonly string Name;
	internal readonly string Description;
	internal readonly LogLevel Level;

	/// <summary>
	///     Ctor
	/// </summary>
	internal ValidationItem(long id, long lineNumber, string name, string description, LogLevel level)
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
