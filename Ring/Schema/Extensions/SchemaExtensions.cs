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

	/// <summary>
	/// 	Convert DbSchema model and its contained schema objects into a Meta array representation.
	/// </summary>
	internal static Meta[] ToMeta(this DbSchema schema)
	{
		// Code size: 366 (0x16e)
		// 1. Calculate the exact length needed for the destination array
		var totalLength = 1 + schema.Parameters.Length + schema.TableSpaces.Length +
						  schema.Lexicons.Length + schema.Sequences.Length;

		for (var i = 0; i < schema.TablesById.Length; ++i)
		{
			var table = schema.TablesById[i];
			totalLength += table.Fields.Length + table.Relations.Length + table.Indexes.Length + 1;
		}

		// 2. Allocate the exact destination array
		var result = new Meta[totalLength];
		var index = 0;

		// 3. Populate root Schema metadata
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, schema.Baseline);

		result[index++] = new Meta(
			schema.Id,
			(byte)EntityType.Schema,
			0, // ReferenceId
			(int)schema.Type,
			flags,
			schema.Name,
			schema.Description,
			null, // Value
			schema.Active
		);

		// 4. Populate Parameters
		for (var i = 0; i < schema.Parameters.Length; ++i)
		{
			result[index++] = schema.Parameters[i].ToMeta();
		}

		// 5. Populate TableSpaces
		for (var i = 0; i < schema.TableSpaces.Length; ++i)
		{
			//result[index++] = schema.TableSpaces[i].ToMeta();
		}

		// 6. Populate Lexicons
		for (var i = 0; i < schema.Lexicons.Length; ++i)
		{
			//result[index++] = schema.Lexicons[i].ToMeta();
		}

		// 7. Populate Sequences
		for (var i = 0; i < schema.Sequences.Length; ++i)
		{
			//result[index++] = schema.Sequences[i].ToMeta();
		}

		// 8. Populate Tables & children
		for (var i = 0; i < schema.TablesById.Length; ++i)
		{
			var tableMetas = schema.TablesById[i].ToMeta(schema.Id);
			Array.Copy(tableMetas, 0, result, index, tableMetas.Length);
			index += tableMetas.Length;
		}

		return result;
	}

}
