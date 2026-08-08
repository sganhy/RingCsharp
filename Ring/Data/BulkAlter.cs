using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Database = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;
using ResourceType = Ring.Util.Enums.ResourceType;

namespace Ring.Data;

internal sealed class BulkAlter : IEquatable<BulkAlter>
{
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private SpanList<AlterQuery> _queries; // cannot set _queries as readonly!
	private readonly Database _schema;
	private readonly Dictionary<EntityType, Dictionary<string, TableSpace>> _tablespaces; // <entityType, <tableName or default>, TableSpaceInfo>	

	internal BulkAlter(Database schema)
	{
		// Code size: 33 (0x21)
		_queries = new SpanList<AlterQuery>(16); // min bucket size = 16
		_schema = schema;
		_tablespaces = GetTableSpaceDictionary(schema);
	}

	internal void CreateTable(Table table)
	{
		// Code size: 118 (0x76)
		AppendDdlCommand(AlterQueryType.CreateTable, table);
		// create constraints 
		foreach(var constraint in _schema.DdlBuilder.GetConstraints(table).AsSpan()) AppendDdlCommand(AlterQueryType.CreateTable, constraint);
		// create indexes
		foreach (var index in table.Indexes) AppendDdlCommand(AlterQueryType.CreateIndex, table, index);
	}

	internal SpanList<AlterQuery> Queries => _queries;

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

    internal void Apply(IConnection connection)
	{
		// Code size: 157 (0x9d)
		// sort by Type
		_queries.Sort(static delegate (AlterQuery q1,AlterQuery q2)
		{
			if (q1.Type == q2.Type) return q1.Id.CompareTo(q2.Id);
			return q1.Type.CompareTo(q2.Type);
		});

		var encoding = connection.ClientEncoding;
		var builder = _schema.DdlBuilder;

		foreach (var query in _queries) 
		{
			var sql = query.ToSql(builder);
			if (sql is not null)
			{
				var byteCount = encoding.GetByteCount(sql);
				var error = connection.Execute(query, sql, byteCount);
				int oi = 0;
				++oi;
			}
			else ThrowUnsuportedAlterQueryType(query.Type);
			// add sql log if subscription
		}
	}

	public override int GetHashCode() => this.Hash();	
	public static bool operator ==(BulkAlter left, BulkAlter right) => left.Equals(right);
	public static bool operator !=(BulkAlter left, BulkAlter right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is BulkAlter bulkAlter && Equals(bulkAlter);
	public bool Equals(BulkAlter? other) => other is not null 
		&& _schema.Id == other._schema.Id 
		&& _queries.Count == other._queries.Count 
		&& this.Hash() == other.Hash(); // Code size: 68 (0x44)

	#region private methods

	private void AppendDdlCommand(AlterQueryType type, Constraint constraint)
	{
		// Code size: 182 (0xb6)
		var table = constraint.ToTable;
		if (type == AlterQueryType.CreateTable)
			switch (constraint.Type)
			{
				case ConstraintType.PrimaryKey:
					_queries.Add(new AlterQuery(table.Id, table, AlterQueryType.CreatePrimaryKey, null, constraint, null, GetTableSpace(table, EntityType.Constraint)));
					break;
				case ConstraintType.NotNull:
					_queries.Add(new AlterQuery(table.Id, table, AlterQueryType.CreateNotNull, null, constraint, null, GetTableSpace(table, EntityType.Constraint)));
					break;
				case ConstraintType.Check:
					_queries.Add(new AlterQuery(table.Id, table, AlterQueryType.CreateCheckConstraint, null, constraint, null, GetTableSpace(table, EntityType.Constraint)));
					break;
			}
	}

	private void AppendDdlCommand(AlterQueryType type, Table table, in Column? column = null)
	{
		switch (type)
		{
			
			case AlterQueryType.CreateTable:
				_queries.Add(new AlterQuery(table.Id, table, type, null, null, null, GetTableSpace(table, EntityType.Table)));
				break;
		}
	}

	private void AppendDdlCommand(AlterQueryType type, Table table, Index? index)
	{
		switch (type)
		{
			case AlterQueryType.CreateIndex:
				if (table.Type != TableType.Business && index?.Unique==true && index.IsPrimaryKey(table)) break; 
				_queries.Add(new AlterQuery(table.Id, table, type, null,null, index, GetTableSpace(table, EntityType.Index)));
				break;
		}
	}

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

	#endregion

}
