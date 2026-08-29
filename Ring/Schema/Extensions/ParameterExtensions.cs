using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class ParameterExtensions
{
	private readonly static string DefaultConnPoolSize = "1";

	internal static Meta ToMeta(this Parameter parameter, int refId)
	{
		// Code size: 67 (0x43)
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, parameter.Baseline);
		var meta = new Meta((int)parameter.Type, (byte)EntityType.Parameter, refId, (int)parameter.ValueType, flags, parameter.Name, parameter.Description,	parameter.Value, parameter.Active);
		return meta;
	}

	/// <summary>
	/// 	Get Parameter by parameterType, case sensitive search ==> O(log n) complexity
	/// </summary>
	internal static Parameter? GetParameter(this Parameter[] parameters, ParameterType parameterType)
	{
		// Code size: 62 (0x3e)
		int indexerLeft = 0, indexerRight = parameters.Length - 1, indexerMiddle, indexerCompare;
		var parameterTypeId = (int)parameterType;

		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = (indexerLeft + indexerRight) >> 1;
			indexerCompare = parameterTypeId.CompareTo(parameters[indexerMiddle].Id);
			if (indexerCompare == 0) return parameters[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}

		return null;
	}

	internal static float GetVersion(this Parameter parameter)=> float.TryParse(parameter.Value, CultureInfo.InvariantCulture, out var version) ? version : 0f; // Code size: 28 (0x1c)

	internal static int GetMaxPoolSize(this Parameter[] parameters)
	{
		// Code size: 63 (0x3f)
		var param = GetParameter(parameters, ParameterType.MaxPoolSize);
		var paramTemplate = ResourceHelper.GetParameter(ParameterType.MaxPoolSize);
		return param is not null ? int.Parse(param.Value, CultureInfo.InvariantCulture) : int.Parse(DefaultConnPoolSize, CultureInfo.InvariantCulture);
	}

	internal static string GetDbConnectionString(this Parameter[] parameters) => GetParameter(parameters, ParameterType.DbConnectionString)?.Value ?? string.Empty; // Code size: 30 (0x1e)

	internal static int GetMinPoolSize(this Parameter[] parameters)
	{
		var param = GetParameter(parameters, ParameterType.MinPoolSize);
		var paramTemplate = ResourceHelper.GetParameter(ParameterType.MinPoolSize);
		return param is not null ? int.Parse(param.Value, CultureInfo.InvariantCulture) :
			int.Parse(DefaultConnPoolSize, CultureInfo.InvariantCulture);
	}

	internal static int Hash(this Parameter parameter)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddParameter(parameter);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Field instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this Parameter parameter, Parameter? other)
	{
		// Code size: 70 (0x46)
		if (!parameter.BaseEntityEquals(other)) return false;
		// other cannot be null here 
		return parameter.Type == other!.Type && parameter.ValueType == other.ValueType && parameter.Type == other.Type && parameter.ReferenceType == other.ReferenceType;
	}

}