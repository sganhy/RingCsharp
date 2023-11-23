using Ring.Schema.Enums;

namespace Ring.Schema.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class ParameterTypeAttribute :  Attribute
{
    internal string Name { get; private set; }
    internal string Description { get; private set; }
    internal FieldType ParameterDataType { get; private set; }
    internal EntityType TargetEntity { get; private set; }
    internal string? DefaultValue { get; private set; }

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
