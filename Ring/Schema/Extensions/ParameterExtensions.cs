using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class ParameterExtensions
{
	private const int DefaultConnPoolSize = 1;

	private readonly static string CurrentTimestamp = "[CURRENT_TIMESTAMP]";

	internal static Meta ToMeta(this Parameter parameter)
	{
		// Code size: 128 (0x80)
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, parameter.Baseline);
		var value = parameter.ValueType == FieldType.DateTime && string.Equals(parameter.Value,CurrentTimestamp, StringComparison.OrdinalIgnoreCase) ? 
			DateTime.UtcNow.ToString(parameter.ValueType, true, null)	: parameter.Value;
		var meta = new Meta((int)parameter.Type, (byte)EntityType.Parameter, parameter.ReferenceId, (int)parameter.ValueType, flags, parameter.Name, parameter.Description, value, parameter.Active);
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

	internal static string GetDbConnectionString(this Parameter[] parameters) => GetParameter(parameters, ParameterType.DbConnectionString)?.Value ?? string.Empty; // Code size: 30 (0x1e)

	internal static int GetMaxPoolSize(this Parameter[] parameters)
	{
		// Code size: 39 (0x27) - no virtual calls!
		var param = GetParameter(parameters, ParameterType.MaxPoolSize);
		return param is not null && param.Value is not null ? int.Parse(param.Value, CultureInfo.InvariantCulture) : DefaultConnPoolSize;
	}

	internal static int GetMinPoolSize(this Parameter[] parameters)
	{
		// Code size: 39 (0x27) - no virtual calls!
		var param = GetParameter(parameters, ParameterType.MinPoolSize);
		return param is not null && param.Value is not null ? int.Parse(param.Value, CultureInfo.InvariantCulture) : DefaultConnPoolSize;
	}

	internal static int Hash(this Parameter parameter)
	{
		// Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddParameter(parameter);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Field instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this Parameter? parameter, Parameter? other)
	{
		// Code size: 100 (0x64) - no virtual calls!
		if (ReferenceEquals(parameter, other)) return true;
		if (parameter is null || other is null) return false;
		return parameter.BaseEntityEquals(other) && parameter.Type == other.Type && parameter.ValueType == other.ValueType && parameter.ReferenceType == other.ReferenceType
			&& parameter.ReferenceId == other.ReferenceId && string.Equals(parameter.Value, other.Value, StringComparison.Ordinal);
	}

}