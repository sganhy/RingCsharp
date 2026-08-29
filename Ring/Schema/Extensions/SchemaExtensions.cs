using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Extensions;

internal static class SchemaExtensions
{

	/// <summary>
	/// 	Get table object by name (case sensitive) --> O(log n)
	/// </summary>
	internal static Sequence? GetSequence(this DbSchema schema, string name)
	{
		// Code size: 90 (0x5a)
		var span = new ReadOnlySpan<Sequence>(schema.Sequences);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	/// <summary>
	/// 	Get table object by Id --> O(log n)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Table? GetTable(this DbSchema schema, int id)
	{
		// Code size: 89 (0x59) - no virtual calls
		var span = new ReadOnlySpan<Table>(schema.TablesById);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = id - span[indexerMiddle].Id;
			if (indexerCompare == 0L) return span[indexerMiddle];
			if (indexerCompare > 0L) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	/// <summary>
	/// 	Get parameter by ParameterType --> O(log n)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Parameter? GetParameter(this DbSchema schema, ParameterType parameterType) => schema.Parameters.GetParameter(parameterType); // Code size: 13 (0xd)

	/// <summary>
	/// 	Get table object by name (case sensitive) --> O(log n)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Table? GetTable(this DbSchema schema, string name)
	{
		// Code size: 90 (0x5a) - no virtual calls
		var span = new ReadOnlySpan<Table>(schema.TablesByName);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name); 
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

}
