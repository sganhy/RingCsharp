using Ring.Schema.Enums;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical field
/// </summary>
internal sealed class Field : BaseEntity
{
	internal readonly SearchableType SearchableType;
	internal readonly string? DefaultValue;
	internal readonly bool Multilingual;
	internal readonly bool NotNull;
	internal readonly int Size;
	internal readonly FieldType Type;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Field(int id, string name, string? description, FieldType type, int size, string? defaultValue,
		SearchableType searchableType, bool baseline, bool notNull, bool multilingual, bool active)
		: base(id, name, description, baseline, active)
	{
		Type = type;
		Size = size;
		DefaultValue = defaultValue;
		NotNull = notNull;
		SearchableType = searchableType;
		Multilingual = multilingual;
	}

#if DEBUG
	public sealed override string ToString() => $"{Id} - {Name} ({Type})";
#endif
}