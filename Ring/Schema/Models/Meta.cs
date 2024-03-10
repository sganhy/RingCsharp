namespace Ring.Schema.Models;

/// <summary>
/// Cannot be a struct due to extensions
///		removed SchemaId field
/// </summary>
internal sealed class Meta
{
	internal int Id;
	internal byte ObjectType;
	internal int ReferenceId;
	internal int DataType;
	internal long Flags;
	internal string Name;			// name of entity
	internal string? Description;	// late loading 
	internal string? Value;
	internal bool Active = true;

	public Meta() => Name = string.Empty; // this constructor should be public due to XUnit lib usage
	internal Meta(string name) => Name = name;

#if DEBUG
	public override string ToString() => Name ?? string.Empty;
#endif

}