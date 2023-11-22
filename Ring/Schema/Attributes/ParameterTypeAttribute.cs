using Ring.Schema.Enums;

namespace Ring.Schema.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal class ParameterTypeAttribute :  Attribute
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public FieldType ParameterDataType { get; private set; }
    public EntityType TargetEntity { get; private set; }
    public string? DefaultValue { get; private set; }


    public ParameterTypeAttribute(string name, string description, FieldType dataType, EntityType targetEntity, string? defaultValue=null)
    {
        Name = name;
        Description = description;
        ParameterDataType = dataType;
        TargetEntity = targetEntity;
        DefaultValue = defaultValue;
    }

    public ParameterTypeAttribute(FieldType dataType, EntityType targetEntity)
    {
        Name = string.Empty;
        Description = string.Empty;
        ParameterDataType = dataType;
        TargetEntity = targetEntity;
    }


}
