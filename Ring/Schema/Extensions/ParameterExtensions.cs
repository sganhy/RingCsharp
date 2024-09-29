using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class ParameterExtensions
{
    private readonly static string DefaultConnPoolSize = "1";

    /// <summary>
    /// Get Parameter by parameterType, case sensitive search ==> O(log n) complexity
    /// </summary>
    internal static Parameter? GetParameter(this Parameter[] parameters, long hash)
    {
        int indexerLeft = 0, indexerRigth = parameters.Length - 1, indexerMiddle, indexerCompare;

        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = hash.CompareTo(parameters[indexerMiddle].Hash);
            if (indexerCompare == 0) return parameters[indexerMiddle];
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }

        return null;
    }

    internal static Parameter? GetParameter(this Parameter[] parameters, ParameterType type, int referenceId) =>
        GetParameter(parameters, GetParameterHash(null, type, referenceId));

    internal static long GetParameterHash(this Parameter? _, ParameterType type, int referenceId) => (((long)type) << 32) + referenceId;

    internal static int GetMinPoolSize(this Parameter[] parameters, int schemaId)
    {
        var param = GetParameter(parameters, ParameterType.MinPoolSize, schemaId);
        return param != null ? int.Parse(param.Value, CultureInfo.InvariantCulture) :
            int.Parse(ParameterTypeExtensions.GetDefaultValue(ParameterType.MinPoolSize) ?? DefaultConnPoolSize,
                CultureInfo.InvariantCulture);
    }
    internal static string GetDbConnectionString(this Parameter[] parameters, int schemaId)
        => GetParameter(parameters, ParameterType.DbConnectionString, schemaId)?.Value ?? string.Empty;

    internal static int GetMaxPoolSize(this Parameter[] parameters, int schemaId)
    {
        var param = GetParameter(parameters, ParameterType.MaxPoolSize, schemaId);
        return param != null ? int.Parse(param.Value, CultureInfo.InvariantCulture) :
            int.Parse(ParameterTypeExtensions.GetDefaultValue(ParameterType.MinPoolSize) ??
            DefaultConnPoolSize, CultureInfo.InvariantCulture);
    }

    internal static Meta ToMeta(this Parameter parameter)
    {
        var flags = 0L;
        flags = Meta.SetEntityBaseline(flags, parameter.Baseline);
        var dataType = 0;
        dataType = Meta.SetParameterValueType(dataType, parameter.ValueType);
        var meta = new Meta((int)parameter.Type, (byte)EntityType.Parameter, parameter.ReferenceId, dataType, flags, parameter.Name, parameter.Description,
            parameter.Value, parameter.Active);
        return meta;
    }

}