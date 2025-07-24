using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Extensions;

internal static class SchemaExtensions
{
	internal static Parameter? GetParameter(this DbSchema schema, ParameterType parameterType)
		=> ParameterExtensions.GetParameter(schema.Parameters, parameterType, schema.Id);

	/// <summary>
	/// 	Get table object by name (case sensitive) --> O(log n)
	/// </summary>
	internal static Sequence? GetSequence(this DbSchema schema, string name)
	{
		var span = new ReadOnlySpan<Sequence>(schema.Sequences);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
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
		// Code size: 91 (0x5b)
		var span = new ReadOnlySpan<Table>(schema.TablesById);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = id - span[indexerMiddle].Id;
			if (indexerCompare == 0L) return span[indexerMiddle];
			if (indexerCompare > 0L) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	/// <summary>
	/// 	Get table object by name (case sensitive) --> O(log n)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Table? GetTable(this DbSchema schema, string name)
	{
		// Code size: 92 (0x5c)
		var span = new ReadOnlySpan<Table>(schema.TablesByName);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name); 
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

}
