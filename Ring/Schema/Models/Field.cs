using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical field
/// </summary>
internal sealed class Field : BaseEntity, IEquatable<Field>
{
	// 63 bytes → rounded to 64 bytes. - 1 bytes padding
	internal readonly string? DefaultValue;
	internal readonly string? EffectiveDefaultValue; // default value to use when creating a new record. If not specified and field is mandatory, then use the default value for the field type.
	internal readonly int Size;
	internal readonly FieldType Type;
	internal readonly SearchableType SearchableType;
	internal readonly bool Multilingual;
	internal readonly bool NotNull;
	internal readonly bool AllowTruncation; // Truncate data on insert, update if it exceeds size.

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Field(int id, string name, string? description, FieldType type, int size, string? defaultValue, string? effectiveDefaultValue, SearchableType searchableType,
		bool baseline, bool notNull, bool multilingual, bool allowTruncation, bool active) : base(id, name, description, baseline, active)
	{
		Type = type;
		Size = size;
		DefaultValue = defaultValue;
		EffectiveDefaultValue = effectiveDefaultValue;
		NotNull = notNull;
		SearchableType = searchableType;
		Multilingual = multilingual;
		AllowTruncation = allowTruncation;
	}

	public static bool operator ==(Field? left, Field? right) => left.IsEquivalentTo(right); // Code size: 8 (0x8)
	public static bool operator !=(Field? left, Field? right) => !left.IsEquivalentTo(right); // Code size: 11 (0xb)
	public override bool Equals(object? obj) => obj is Field field && Equals(field); // Code size: 20 (0x14)
	public bool Equals(Field? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {Name} ({Type})";
#endif
}