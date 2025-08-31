using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Helpers;

namespace Ring.Schema.Builders;

internal sealed class ParameterBuilder
{
#pragma warning disable S2325, CA1822 // Mark members as static
    internal Parameter GetParameter(ParameterType parameterType, string? value, int referenceId)
    {
        var paramTemplate = ResourceHelper.GetParameter(parameterType);
        return new((int)parameterType, paramTemplate.Name, paramTemplate.Description, parameterType,
            paramTemplate.ValueType, value ?? string.Empty, paramTemplate.DefaultValue, referenceId, EntityType.Schema, true, true);
    }
#pragma warning restore S2325, CA1822
}
