using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Parameter : BaseEntity
{
	internal readonly string Value;
	internal readonly string? DefaultValue;
	internal readonly FieldType ValueType;
	internal readonly ParameterType Type;
	internal readonly int ReferenceId;
	internal readonly EntityType ReferenceType;

	internal Parameter(int id, string name, string? description, ParameterType type, FieldType valueType,
		string value, string? defaultValue, int referenceId, EntityType referenceType, bool baseline, bool active)
		: base(id, name, description, baseline, active)
	{
		ReferenceId = referenceId;
		DefaultValue = defaultValue;
		Value = value;
		ValueType = valueType;
		Type = type;
		ReferenceType = referenceType;
	}

#if DEBUG
	public sealed override string ToString() => $"{Id} - {Name} ({Value})";
#endif

}