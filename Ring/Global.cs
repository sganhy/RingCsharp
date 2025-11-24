using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring;

internal static class Global
{
	private const int MaxSchemaBucketSize = 4096;
	private const int MinSchemaBucketSize = 16;
	private static readonly object SyncRoot = new();
	private static int _currentMaxNumberOfSchema;
	private static DbSchema? _defaultSchema;
	private static int _maxSchemaId;
	private static bool _initialized;
	private static DbSchema?[] _schemas = Array.Empty<DbSchema>();
	private static (string,int)[] _schemaMappers = Array.Empty<(string, int)>(); // (schemaName, schemaId)
	private static int _schemaCount; // current number of schemas

	internal static void Init(IConfiguration configuration)
	{
		if (_initialized) return;
		_schemaCount = 0; 
		_currentMaxNumberOfSchema = configuration.MaxNumberOfSchema * 4;
		if (_currentMaxNumberOfSchema > MaxSchemaBucketSize) _currentMaxNumberOfSchema = MaxSchemaBucketSize;
		if (_currentMaxNumberOfSchema < MinSchemaBucketSize) _currentMaxNumberOfSchema = MinSchemaBucketSize;
		_schemas = new DbSchema[_currentMaxNumberOfSchema+1]; // @meta schema (Id=0)
		_initialized = true;
	}
	internal static void SetDefaultSchema(DbSchema schema)
	{
        // Code size: 43 (0x2b) - removed box statements - no virtual calls
        lock (SyncRoot)
		{
			if (!ReferenceEquals(schema, _defaultSchema)) _defaultSchema = schema; // assign schema if necessary
		}
	}
	internal static void LoadSchema(DbSchema schema)
	{
		// Code size: 257 (0x101)
		lock (SyncRoot)
		{
			_defaultSchema ??= schema;
			var currSchema = _schemas[schema.Id];
			
			// new schema ?? -  schema change its name, not managed yet!!!
			if (currSchema is null)
			{
				// prepare new mapper
				var mappingCount = _schemaCount + 1;
				var newMapping = new (string, int)[mappingCount];
				var index = 0;
				var lastSchemaId = _maxSchemaId + 1;

				// copy as fast as possible mapping
				for (var i = 0; i < lastSchemaId; ++i)
				{
					var sch = _schemas[i];
					if (sch is null) continue;
					newMapping[index] = (sch.Name, sch.Id);
					++index;
				}
				newMapping[index] = (schema.Name, schema.Id); // add new one to last position
				// sort mapper
				Array.Sort(newMapping, (x, y) => string.CompareOrdinal(x.Item1,y.Item1));

				// assign new mapper
				_maxSchemaId = schema.Id;
				_schemaMappers = newMapping;
			}
			_schemas[schema.Id] = schema;
			
			// new schema ??
			if (currSchema is null)  ++_schemaCount;
		}
	}
	internal static void Clear()
	{
		_schemaMappers = Array.Empty<(string, int)>();
		_schemaCount = 0;
		_schemas = new DbSchema[_currentMaxNumberOfSchema+1];
	}

	internal static DbSchema DefaultSchema => _defaultSchema ?? Meta.GetDefaultSchema(Meta.Create(string.Empty),DatabaseProvider.SqlLite); // Code size: 27 (0x1b) - DatabaseProvider cannot be undefined !!!!

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsSchemaDefault(DbSchema schema) => ReferenceEquals(schema,_defaultSchema); // Code size: 9 (0x9)
	internal static int MaxSchemaId() => MaxSchemaBucketSize; // Code size: 6 (0x6)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static DbSchema? GetSchema(int id)
	{
		// Code size: 8 (0x8)
		var schList = _schemas;
		return schList[id];
	}
	internal static DbSchema? GetSchema(string name)
	{
		// Code size: 97 (0x61)
		var span = new ReadOnlySpan<(string, int)>(_schemaMappers);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Item1);
			if (indexerCompare == 0) return GetSchema(span[indexerMiddle].Item2);
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Table? GetTable(string? schemaName, string tableName)
	{
		// Code size: 43 (0x2b) - DatabaseProvider cannot be undefined !!!!
		var schema = schemaName is not null ? GetSchema(schemaName) ?? Meta.GetDefaultSchema(Meta.Create(string.Empty), DatabaseProvider.SqlLite) : DefaultSchema;
		return schema.GetTable(tableName);
	}

}
