using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical field
/// </summary>
internal sealed class Field : BaseEntity, IEquatable<Field>
{
    // 60 bytes with padding
    internal readonly string? DefaultValue;
    internal readonly int Size;
	internal readonly FieldType Type;
	internal readonly SearchableType SearchableType;
	internal readonly bool Multilingual;
	internal readonly bool NotNull;
	internal readonly bool AllowTruncation;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Field(int id, string name, string? description, FieldType type, int size, string? defaultValue, SearchableType searchableType,
		bool baseline, bool notNull, bool multilingual, bool allowTruncation, bool active) : base(id, name, description, baseline, active)
	{
		Type = type;
		Size = size;
		DefaultValue = defaultValue;
		NotNull = notNull;
		SearchableType = searchableType;
		Multilingual = multilingual;
		AllowTruncation = allowTruncation;
	}

	public static bool operator ==(Field left, Field right) => left.Equals(right);
	public static bool operator !=(Field left, Field right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Field field && Equals(field);
	public bool Equals(Field? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {Name} ({Type})";
#endif
}