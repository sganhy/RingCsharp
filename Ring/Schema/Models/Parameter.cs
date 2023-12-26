using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Parameter : BaseEntity
{
	internal readonly string Value;
	internal readonly string? DefaultValue;
	internal readonly FieldType ValueType;
	internal readonly ParameterType Type;
	internal readonly int ReferenceId;
	internal readonly long Hash;

	internal Parameter(int id, string name, string? description, ParameterType type, FieldType valueType,
		string value, string? defaultValue, int referenceId, bool baseline, bool active) 
		: base(id, name, description, active, baseline)
	{
		Hash = ParameterExtensions.GetParameterHash(null, type, referenceId);
		ReferenceId = referenceId;
		DefaultValue = defaultValue;
		Value = value;
		ValueType = valueType;
		Type = type;
	}
}
