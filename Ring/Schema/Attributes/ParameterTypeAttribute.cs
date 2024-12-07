using Ring.Schema.Enums;

namespace Ring.Schema.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class ParameterTypeAttribute :  Attribute
{
	internal string Name { get; }
	internal string Description { get; }
	internal FieldType ParameterDataType { get; }
	internal EntityType TargetEntity { get; }
	internal string? DefaultValue { get; }

	internal ParameterTypeAttribute(string name, string description, FieldType dataType, EntityType targetEntity, string? defaultValue=null)
	{
		Name = name;
		Description = description;
		ParameterDataType = dataType;
		TargetEntity = targetEntity;
		DefaultValue = defaultValue;
	}

	internal ParameterTypeAttribute(FieldType dataType, EntityType targetEntity)
	{
		Name = string.Empty;
		Description = string.Empty;
		ParameterDataType = dataType;
		TargetEntity = targetEntity;
	}

}
