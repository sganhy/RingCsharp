using Ring.Schema.Attributes;
using Ring.Schema.Enums;
using Ring.Util.Extensions;

namespace Ring.Schema.Extensions;

internal static class ParameterTypeExtensions
{
    internal static string? GetDefaultValue(this ParameterType parameterType) =>
        parameterType.GetCustomAttribute<ParameterTypeAttribute>()?.DefaultValue;
    internal static FieldType GetValueType(this ParameterType parameterType) =>
        parameterType.GetCustomAttribute<ParameterTypeAttribute>()?.ParameterDataType ?? FieldType.Undefined;
    internal static string GetName(this ParameterType parameterType) =>
        parameterType.GetCustomAttribute<ParameterTypeAttribute>()?.Name ?? string.Empty;
    internal static string? GetDescription(this ParameterType parameterType) =>
        parameterType.GetCustomAttribute<ParameterTypeAttribute>()?.Description ?? string.Empty;
}
