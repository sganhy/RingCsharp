using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Database = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;
using ResourceType = Ring.Util.Enums.ResourceType;

namespace Ring.Data;

internal sealed class BulkAlter : IEquatable<BulkAlter>, IDisposable
{
	private const int Buckets = 256;
	private const int MinPooledListSize = 16;
	private bool _disposed;
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private PooledList<AlterQuery> _queries; // cannot set _queries as readonly!
	private readonly Database _schema;
	private readonly Dictionary<EntityType, Dictionary<string, TableSpace>> _tablespaces; // <entityType, <tableName or default>, TableSpaceInfo>	

	internal BulkAlter(Database schema) : this(schema, new PooledList<AlterQuery>(MinPooledListSize)) {}
	internal BulkAlter(Database schema, PooledList<AlterQuery> queries)
	{
		// Code size: 46 (0x2e)
		_disposed = false;
		_queries = queries;
		_schema = schema;
		_tablespaces = GetTableSpaceDictionary(schema);
	}

	internal void CreateTable(Table table, bool createComment = true)
	{
		// Code size: 223 (0xdf)
		AppendDdlCommand(AlterQueryType.CreateTable, table);
		// create constraints 
		foreach (var constraint in table.Constraints.AsSpan()) AppendDdlCommand(AlterQueryType.CreateTable, constraint, table);
		// create comments
		if (createComment)
		{
			if (table.Description is not null) AppendDdlCommand(AlterQueryType.CreateTableComment, table);
			foreach (var column in table.Columns.AsSpan())
			{
				var description = table.GetDescription(column);
				if (description is not null) AppendDdlCommand(AlterQueryType.CreateColumnComment, table, column);
			}
		}
		// create indexes
		foreach (var index in table.Indexes) AppendDdlCommand(AlterQueryType.CreateIndex, table, index);
	}

	internal PooledList<AlterQuery> Queries => _queries;
	internal Database Schema => _schema;
	internal bool Disposed => _disposed;

	internal void CreateIndex(string tableName, string indexName)
	{
		// Code size: 51 (0x33)
		var table = _schema.GetTable(tableName);
		if (table is null) ThrowInvalidObjectType(tableName);
		// TODO throw exception if null 
		var index = table.GetIndex(indexName);
		if (index is null) ThrowInvalidIndexName(tableName, indexName);
		AppendDdlCommand(AlterQueryType.CreateIndex, table, index);
	}


	internal void AlterTableAdd(string tableName, string columnName)
	{
		// columnName: logical column name
		// Code size: 51 (0x33)
		var table = _schema.GetTable(tableName);
		if (table is null) ThrowInvalidObjectType(tableName);
		var column = table.GetColumn(columnName);
		if (column is null) ThrowInvalidFieldName(tableName, columnName);
		AppendDdlCommand(AlterQueryType.AlterTableAddColumn, table, column);
	}

	internal void Apply()
	{
		// Code size: 59 (0x3b)
		if (_queries.Count == 0) return;
#pragma warning disable CA2000 // Dispose objects before losing scope
		var connection = _schema.Connections.Get();
#pragma warning restore CA2000
		try
		{
			Apply(connection);
		}
		finally
		{
			// return connection immediatly
			_schema.Connections.Put(connection);
		}
	}

	/// <summary>
	/// Apply changes to the database without transaction, the caller must handle transaction management if needed.
	/// </summary>
	internal void Apply(IConnection connection)
	{
		// Code size: 157 (0x9d)
		Sort(); // sort by Type

		var encoding = connection.ClientEncoding;
		var builder = _schema.DdlBuilder;

		foreach (var query in _queries)
		{
			var sql = query.ToSql(builder);
			if (sql is not null)
			{
				var bytes = encoding.GetBytes(sql);
				var error = connection.Execute(query, bytes);
				int oi = 0;
				++oi;
			}
			else
				ThrowUnsuportedAlterQueryType(query.Type);
			// add sql log if subscription
		}
	}

	internal void Sort()
	{
		// Code size: 350 (0x15e) - Linear Time Complexity O(n) + stability guarantee
		var queries = _queries.AsSpan();
		if (queries.Length < 2) return;

		Span<int> count = stackalloc int[Buckets];

		// Pass 1: count occurrences per Type and check if already sorted
		var maxType = 0;
		var previous = int.MinValue;
		var sorted = true;
		for (var i = 0; i < queries.Length; i++)
		{
			var type = (int)queries[i].Type;
			if ((uint)type >= Buckets)
			{
				queries.Sort(); // Fallback for out-of-bounds types
				return;
			}
			count[type]++;
			if (type > maxType) maxType = type;
			if (type < previous) sorted = false;
			previous = type;
		}
		if (sorted) return; // nothing to move

		// Pass 2: compute starting indices (prefix sums) for each bucket
		Span<int> start = stackalloc int[Buckets];
		var sum = 0;
		for (var t = 0; t <= maxType; t++)
		{
			start[t] = sum;
			sum += count[t];
		}

		// Pass 3: stably place elements into a temporary pooled buffer
		var tempBuffer = ArrayPool<AlterQuery>.Shared.Rent(queries.Length);
		Span<AlterQuery> temp = tempBuffer.AsSpan(0, queries.Length);

		// Iterate forward through the original queries to guarantee stability
		for (var i = 0; i < queries.Length; i++)
		{
			var query = queries[i];
			var type = (int)query.Type;
			temp[start[type]++] = query;
		}

		// Copy the sorted, stable sequence back to the original list
		temp.CopyTo(queries);
		ArrayPool<AlterQuery>.Shared.Return(tempBuffer);
	}
	public override int GetHashCode() => this.Hash(); // Code size: 7 (0x7)
	public static bool operator ==(BulkAlter left, BulkAlter right) => left.Equals(right);
	public static bool operator !=(BulkAlter left, BulkAlter right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is BulkAlter bulkAlter && Equals(bulkAlter);
	public bool Equals(BulkAlter? other) => other is not null && _schema.Id == other._schema.Id && _queries.Count == other._queries.Count && this.Hash() == other.Hash(); // Code size: 68 (0x44)

	public void Dispose()
	{
		// Code size: 28 (0x1c)
		if (_disposed) return;
		_disposed = true;
		_queries.Dispose();
	}

	#region private methods

	private void AppendDdlCommand(AlterQueryType type, Constraint constraint, Table table)
	{
		// Code size: 127 (0x7f)
		if (type == AlterQueryType.CreateTable)
			switch (constraint.Type)
			{
				case ConstraintType.PrimaryKey:
					_queries.Add(new AlterQuery(table.Id, table, constraint.Type.ToAlterQueryType(), -1, constraint, null, GetTableSpace(table, EntityType.Constraint)));
					break;
				case ConstraintType.NotNull:
				case ConstraintType.Check:
				case ConstraintType.Default:
					_queries.Add(new AlterQuery(table.Id, table, constraint.Type.ToAlterQueryType(), -1, constraint, null, null));
					break;
			}
	}

	private void AppendDdlCommand(AlterQueryType type, Table table, in Column? column = null)
	{
		// Code size: 158 (0x9e)
		switch (type)
		{

			case AlterQueryType.CreateTable:
				_queries.Add(new AlterQuery(table.Id, table, type, -1, null, null, GetTableSpace(table, EntityType.Table)));
				break;
			case AlterQueryType.CreateTableComment:
				_queries.Add(new AlterQuery(table.Id, table, type, -1, null, null, null));
				break;
			case AlterQueryType.CreateColumnComment:
				_queries.Add(new AlterQuery(table.Id, table, type, column is null ? -1 : table.GetColumnIndex(column.Value), null, null, null));
				break;
		}
	}

	private void AppendDdlCommand(AlterQueryType type, Table table, Index? index)
	{
		switch (type)
		{
			case AlterQueryType.CreateIndex:
				_queries.Add(new AlterQuery(table.Id, table, type, -1, null, index, GetTableSpace(table, EntityType.Index)));
				break;
		}
	}

	private TableSpace? GetTableSpace(Table table, EntityType entityType)
	{
		if (_tablespaces.TryGetValue(entityType, out Dictionary<string, TableSpace>? subDico))
		{
			var key = table.Name;
			if (subDico.TryGetValue(key, out TableSpace? value)) return value;
			// find default tablespace: not connected to a specific table
			key = string.Empty;
			if (subDico.TryGetValue(key, out TableSpace? value1)) return value1;
		}
		return null;
	}

	private static Dictionary<EntityType, Dictionary<string, TableSpace>> GetTableSpaceDictionary(Database schema)
	{
		// Code size: 230 (0xe6)
		var result = new Dictionary<EntityType, Dictionary<string, TableSpace>>()
		{
			{ EntityType.Index, new Dictionary<string, TableSpace>()},
			{ EntityType.Table, new Dictionary<string, TableSpace>()},
			{ EntityType.Constraint, new Dictionary<string, TableSpace>()}
		};

		// constraint is consider as index for the moment, can be modified in the future
		foreach (var tablespace in new ReadOnlySpan<TableSpace>(schema.TableSpaces))
		{
			// if TableName.Length == 0 then it's a default tablespace 
			if (tablespace.TableName.Length == 0)
			{
				AddTableSpace(result, tablespace, string.Empty);
			}
			else
			{
				foreach (var table in new ReadOnlySpan<string>(tablespace.TableName))
					AddTableSpace(result, tablespace, table);
			}
		}

		// no tablespace for constraints use the index one
		if (!result[EntityType.Constraint].ContainsKey(string.Empty) &&
			result[EntityType.Index].TryGetValue(string.Empty, out TableSpace? value))
		{
			result[EntityType.Constraint].Add(string.Empty, value);
		}
		return result;
	}

	private static void AddTableSpace(Dictionary<EntityType, Dictionary<string, TableSpace>> dico, TableSpace tablespace,
		string tableName)
	{
		if (tablespace.Index && !dico[EntityType.Index].ContainsKey(tableName))
			dico[EntityType.Index].Add(tableName, tablespace);
		if (tablespace.Table && !dico[EntityType.Table].ContainsKey(tableName))
			dico[EntityType.Table].Add(tableName, tablespace);
		if (tablespace.Constraint && !dico[EntityType.Constraint].ContainsKey(tableName))
			dico[EntityType.Constraint].Add(tableName, tablespace);
	}
	
	// error handling
	[DoesNotReturn]
	private static void ThrowInvalidObjectType(string objectType) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkAlterInvalidTableName), objectType));

	[DoesNotReturn]
	private static void ThrowUnsuportedAlterQueryType(AlterQueryType type) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkAlterUnsuportedAlterQueryType), type));

	//TODO - create a specific message for invalid index name
	[DoesNotReturn]
	private static void ThrowInvalidIndexName(string objectType, string indexName) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkAlterInvalidTableName), objectType));

	[DoesNotReturn]
	private static void ThrowInvalidFieldName(string objectType, string fieldName) =>
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkAlterInvalidFieldName), fieldName, objectType));
			
	#endregion

}