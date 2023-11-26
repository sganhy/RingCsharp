using Ring.Schema.Attributes;
using Ring.Schema.Enums;
using Ring.Util.Extensions;

namespace Ring.Schema.Extensions;

internal static class ParameterTypeExtensions
{
    internal static string? GetDefaultValue(this ParameterType parameterType) =>
        parameterType.GetCustomAttribute<ParameterTypeAttribute>()?.DefaultValue;

}
