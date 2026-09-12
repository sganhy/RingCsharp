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
		// Code size: 345 (0x159)
		// Pre-calculate approximate capacity to avoid List resizes
		var initialCapacity = 1 + schema.Parameters.Length + schema.TableSpaces.Length +
							  schema.Lexicons.Length + schema.Sequences.Length;

		for (var i = 0; i < schema.TablesById.Length; ++i)
		{
			var table = schema.TablesById[i];
			initialCapacity += table.Fields.Length + table.Relations.Length + table.Indexes.Length + 1;
		}

		var result = new List<Meta>(initialCapacity);

		// 1. Convert Schema entity flags & root metadata
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, schema.Baseline);

		var schemaMeta = new Meta(
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
		result.Add(schemaMeta);

		// 2. Convert Parameters
		for (var i = 0; i < schema.Parameters.Length; ++i)
		{
			result.Add(schema.Parameters[i].ToMeta());
		}

		// 3. Convert TableSpaces
		for (var i = 0; i < schema.TableSpaces.Length; ++i)
		{
			//result.Add(schema.TableSpaces[i].ToMeta());
		}

		// 4. Convert Lexicons
		for (var i = 0; i < schema.Lexicons.Length; ++i)
		{
			//result.Add(schema.Lexicons[i].ToMeta());
		}

		// 5. Convert Sequences
		for (var i = 0; i < schema.Sequences.Length; ++i)
		{
			//result.Add(schema.Sequences[i].ToMeta());
		}

		// 6. Convert Tables (and their child Fields, Relations, Indexes)
		for (var i = 0; i < schema.TablesById.Length; ++i)
		{
			result.AddRange(schema.TablesById[i].ToMeta(schema.Id));
		}

		return result.ToArray();
	}

}
