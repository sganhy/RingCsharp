using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Parameter : BaseEntity, IEquatable<Parameter>
{
	// 20 → rounded to 24 bytes. - 4 bytes padding
	internal readonly string Value;
	internal readonly FieldType ValueType;
	internal readonly ParameterType Type;
	internal readonly EntityType ReferenceType;

	internal Parameter(int id, string name, string? description, ParameterType type, FieldType valueType, string value, EntityType referenceType, bool baseline, bool active)
		: base(id, name, description, baseline, active)
	{
		Value = value;
		ValueType = valueType;
		Type = type;
		ReferenceType = referenceType;
	}

	public static bool operator ==(Parameter left, Parameter right) => left.Equals(right);
	public static bool operator !=(Parameter left, Parameter right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Parameter parameter && Equals(parameter);
	public bool Equals(Parameter? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public sealed override string ToString() => $"{Id} - {Name} - {Value} ({DefaultValue})";
#endif

}