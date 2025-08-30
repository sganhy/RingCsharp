using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders;

internal sealed class ParameterBuilder
{
#pragma warning disable S2325, CA1822 // Mark members as static
    internal Parameter GetParameter(ParameterType parameterType, string? value, int referenceId)
    {
        var name = parameterType.GetName();
        return new((int)parameterType, name, parameterType.GetDescription(), parameterType,
            parameterType.GetValueType(), value ?? string.Empty, parameterType.GetDefaultValue(), referenceId, EntityType.Schema, true, true);
    }
#pragma warning restore S2325, CA1822
}
