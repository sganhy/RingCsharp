using Ring.Schema.Enums;
using Ring.Util.Helpers;

namespace Ring.Schema.Extensions;

internal static class ParameterTypeExtensions
{
	internal static string? GetDefaultValue(this ParameterType parameterType) => ResourceHelper.GetParameter(parameterType).DefaultValue;
	internal static FieldType GetValueType(this ParameterType parameterType) => ResourceHelper.GetParameter(parameterType).ValueType;
	internal static string GetName(this ParameterType parameterType) => ResourceHelper.GetParameter(parameterType).Name;
	internal static string? GetDescription(this ParameterType parameterType) => ResourceHelper.GetParameter(parameterType).Description;
}
