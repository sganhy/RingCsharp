using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class ParameterExtensions
{
	private readonly static string DefaultConnPoolSize = "1";

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

	/// <summary>
	/// 	Get Parameter by parameterType, case sensitive search ==> O(log n) complexity
	/// </summary>
	internal static Parameter? GetParameter(this Parameter[] parameters, ParameterType parameterType)
	{
		// Code size: 64(0x40)
		int indexerLeft = 0, indexerRight = parameters.Length - 1, indexerMiddle, indexerCompare;
		var parameterTypeId = (int)parameterType;

		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;	// indexerMiddle <-- indexerMiddle /2 
			indexerCompare = parameterTypeId.CompareTo(parameters[indexerMiddle].Id);
			if (indexerCompare == 0) return parameters[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}

		return null;
	}

	internal static int GetMaxPoolSize(this Parameter[] parameters)
	{
		// Code size: 63 (0x3f)
		var param = GetParameter(parameters, ParameterType.MaxPoolSize);
		var paramTemplate = ResourceHelper.GetParameter(ParameterType.MaxPoolSize);
		return param is not null ? int.Parse(param.Value, CultureInfo.InvariantCulture) :
			int.Parse(paramTemplate.DefaultValue ?? DefaultConnPoolSize, CultureInfo.InvariantCulture);
	}

	internal static string GetDbConnectionString(this Parameter[] parameters) => GetParameter(parameters, ParameterType.DbConnectionString)?.Value ?? string.Empty; // Code size: 30 (0x1e)

	internal static int GetMinPoolSize(this Parameter[] parameters)
	{
		var param = GetParameter(parameters, ParameterType.MinPoolSize);
		var paramTemplate = ResourceHelper.GetParameter(ParameterType.MinPoolSize);
		return param is not null ? int.Parse(param.Value, CultureInfo.InvariantCulture) :
			int.Parse(paramTemplate.DefaultValue ?? DefaultConnPoolSize, CultureInfo.InvariantCulture);
	}


}