using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;

namespace Ring.Data;

internal struct BulkAlter : IEquatable<BulkAlter>
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

	internal readonly SpanList<AlterQuery> Queries => _queries;

	internal void CreateTable(string tableName)
	{
		// Code size: 118 (0x76)
		var table = _schema.GetTable(tableName);
		if (table is null) ThrowInvalidObjectType(tableName);
		AppendDdlCommand(AlterQueryType.CreateTable, table);
		// create constraints 
		foreach(var constraint in _schema.DdlBuiler.GetConstraints(table)) AppendDdlCommand(AlterQueryType.CreateTable, constraint);
		// create indexes
		foreach (var index in table.Indexes) AppendDdlCommand(AlterQueryType.CreateIndex, table, index);
	}

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
		if (column == null) ThrowInvalidFieldName(tableName, columnName);
		AppendDdlCommand(AlterQueryType.AlterTableAddColumn, table, column);
	}

	internal readonly void Apply()
	{
        // Code size: 59 (0x3b)
        if (_queries.Count == 0) return;
        var connection = _schema.Connections.Get();
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

    internal readonly void Apply(IConnection connection)
	{
		// Code size: 91 (0x5b)
		// sort by Type
#pragma warning disable RCS1048 // Use lambda expression instead of anonymous method - never!
		_queries.Sort(static delegate (AlterQuery q1, AlterQuery q2)
		{
			if (q1.Type == q2.Type) return q1.Id.CompareTo(q2.Id);
			return q1.Type.CompareTo(q2.Type);
		});
#pragma warning restore RCS1048
		foreach (var query in _queries) connection.Execute(query);
	}

	public readonly override int GetHashCode() => GetHashCode(this);
	public static bool operator ==(BulkAlter left, BulkAlter right) => left.Equals(right);
	public static bool operator !=(BulkAlter left, BulkAlter right) => !left.Equals(right);
	public override readonly bool Equals(object? obj) => obj is BulkAlter bulkAlter && Equals(bulkAlter);
	public readonly bool Equals(BulkAlter other)
	{
		// Code size: 71 (0x47)
		if (_schema.Id == other._schema.Id && _queries.Count == other._queries.Count)
		{
			return GetHashCode(this) == GetHashCode(other);
		}
		return false;
	}

	#region private methods

	private void AppendDdlCommand(AlterQueryType type, Constraint constraint)
	{
		// Code size: 83 (0x53)
		var table = constraint.ToTable;
		if (type == AlterQueryType.CreateTable && constraint.Type == ConstraintType.PrimaryKey)
			_queries.Add(new AlterQuery(table.Id, table, AlterQueryType.CreatePrimaryKey, _schema.DdlBuiler, null, constraint, null, 
				GetTableSpace(table, EntityType.Constraint)));
	}

	private void AppendDdlCommand(AlterQueryType type, Table table, Column? column = null)
	{
		switch (type)
		{
			
			case AlterQueryType.CreateTable:
				_queries.Add(new AlterQuery(table.Id, table, type, _schema.DdlBuiler, null,
					null, null, GetTableSpace(table, EntityType.Table)));
				break;
		}
	}

	private void AppendDdlCommand(AlterQueryType type, Table table, Index? index)
	{
		switch (type)
		{
			case AlterQueryType.CreateIndex:
				if (table.Type != TableType.Business && index?.Unique==true && index.IsPrimaryKey(table)) break; 
				_queries.Add(new AlterQuery(table.Id, table, type, _schema.DdlBuiler, null,null, index, 
					GetTableSpace(table, EntityType.Index)));
				break;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidObjectType(string objectType) =>
		throw new ArgumentException(string.Format(DefaultCulture,
				  ResourceHelper.GetErrorMessage(ResourceType.BulkAlterInvalidTableName), objectType));

	//TODO - create a specific message for invalid index name
	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidIndexName(string objectType, string indexName) =>
		throw new ArgumentException(string.Format(DefaultCulture,
				ResourceHelper.GetErrorMessage(ResourceType.BulkAlterInvalidTableName), objectType));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidFieldName(string objectType, string fieldName) =>
		throw new ArgumentException(string.Format(DefaultCulture,
				ResourceHelper.GetErrorMessage(ResourceType.BulkAlterInvalidFieldName), fieldName, objectType));

	private readonly TableSpace? GetTableSpace(Table table, EntityType entityType)
	{
		if (_tablespaces.ContainsKey(entityType))
		{
			var subDico = _tablespaces[entityType];
			var key = table.Name;
			if (subDico.ContainsKey(key)) return subDico[key];
			// find default tablespace: not connected to a specific table
			key = string.Empty;
			if (subDico.ContainsKey(key)) return subDico[key];
		}
		return null;
	}

	private static Dictionary<EntityType, Dictionary<string, TableSpace>> GetTableSpaceDictionary(Database schema)
	{
		// Code size: 239 (0xef)
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
			result[EntityType.Index].ContainsKey(string.Empty))
		{
			result[EntityType.Constraint].Add(string.Empty, result[EntityType.Index][string.Empty]);
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

	private static int GetHashCode(BulkAlter bulkAlter)
	{
		var span = bulkAlter._queries.AsReadOnlySpan();
		var hash = 0;
		foreach (var query in span) hash += AlterQueryExtensions.GetHashCode(query);
		return hash;
	}

	#endregion

}
