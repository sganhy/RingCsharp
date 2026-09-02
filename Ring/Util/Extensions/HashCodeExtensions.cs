using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using Index = Ring.Schema.Models.Index;

namespace Ring.Util.Extensions;

internal static class HashCodeExtensions
{
	internal static void AddField(this ref HashCode hashCode, Field field)
	{
		// Code size: 85 (0x55) 
		/*
			int Size;
			FieldType Type;
			SearchableType SearchableType;
			bool Multilingual;
			bool NotNull;
			bool AllowTruncation; 
		*/
		AddBaseEntity(ref hashCode, field);
		// Field-specific properties
		hashCode.Add((int)field.Type);
		hashCode.Add(field.Size);
		hashCode.Add(field.DefaultValue, StringComparer.Ordinal);
		hashCode.Add((int)field.SearchableType);
		hashCode.Add(field.AllowTruncation);
		hashCode.Add(field.NotNull);
		hashCode.Add(field.Multilingual);
	}

	internal static void AddParameter(this ref HashCode hashCode, Parameter parameter)
	{
		// Code size: 61 (0x3d)
		/*
			string Value;
			FieldType ValueType;
			ParameterType Type;
			EntityType ReferenceType;
		*/
		AddBaseEntity(ref hashCode, parameter);
		hashCode.Add(parameter.Value, StringComparer.Ordinal);
		hashCode.Add((int)parameter.ValueType);
		hashCode.Add((int)parameter.Type);
		hashCode.Add((int)parameter.ReferenceType);
	}

	internal static void AddConstraint(this ref HashCode hashCode, Constraint constraint)
	{
		// Code size: 97 (0x61)
		/*
			ConstraintType Type;
			Column[] Columns;
			int? MinValue;
			int? MaxValue;
		*/
		AddBaseEntity(ref hashCode, constraint);
		hashCode.Add((int)constraint.Type);
		if (constraint.MinValue.HasValue) hashCode.Add(constraint.MinValue.Value);
		if (constraint.MaxValue.HasValue) hashCode.Add(constraint.MaxValue.Value);
		AddColumns(ref hashCode, constraint.Columns);
	}

	internal static void AddIndex(this ref HashCode hashCode, Index index)
	{
		// Code size: 59 (0x3b)
		AddBaseEntity(ref hashCode, index);
		// Index-specific properties
		hashCode.Add(index.Unique);
		hashCode.Add(index.Bitmap);
		AddColumns(ref hashCode, index.Columns);
	}

	internal static void AddRelation(this ref HashCode hashCode, Relation relation)
	{
		// Code size: 170 (0xaa)
		AddBaseEntity(ref hashCode, relation);
		// Relation-specific properties
		hashCode.Add(relation.HasConstraint);
		hashCode.Add(relation.NotNull);
		hashCode.Add(relation.ToTable.Id); // pair of identification for a table
		hashCode.Add(relation.ToTable.SchemaId);
		hashCode.Add((int)relation.Type);
		hashCode.Add((int)relation.FieldType);
		// avoid recursion here calling AddRelation again
		if (!ReferenceEquals(relation.InverseRelation, relation))
		{
			hashCode.Add(relation.InverseRelation.Name);
			hashCode.Add(relation.InverseRelation.ToTable.Id); // pair of identification for a table
			hashCode.Add(relation.InverseRelation.ToTable.SchemaId);
		}			
	}

	internal static void AddConnParameter(this ref HashCode hashCode, ConnectionParameters parameters)
	{
		hashCode.Add(parameters.Host);
		hashCode.Add(parameters.Port);
		hashCode.Add(parameters.DatabaseName);
		hashCode.Add(parameters.UserName);
		hashCode.Add(parameters.ApplicationName);
	}

	internal static void AddColumn(this ref HashCode hashCode, Column column)
	{   
		// Code size: 73 (0x49)
		/*
			int Id;
			int RecordIndex;
			EntityType Type; // enum EntityType : byte
			FieldType FieldType; // enum FieldType : byte
			SearchableType SearchableType; // enum SearchableType : byte
			string PhysicalName;
		*/
		hashCode.Add(column.Id);
		hashCode.Add(column.RecordIndex);
		hashCode.Add((int)column.Type);
		hashCode.Add((int)column.FieldType);
		hashCode.Add((int)column.SearchableType);
		hashCode.Add(column.PhysicalName);
	}

	internal static void AddSaveQuery(this ref HashCode hashCode, SaveQuery saveQuery)
	{
		// Code size: 76 (0x4c)
		/*
			Table Table;
			SaveQueryType Type;
			string?[] Data;
			int Offset;
		*/
		hashCode.Add(saveQuery.Table.Id);
		hashCode.Add(saveQuery.Table.SchemaId);
		hashCode.Add((int)saveQuery.Type);
		AddStrings(ref hashCode, saveQuery.Data);
		hashCode.Add(saveQuery.Offset);
	}

	internal static void AddAlterQuery(this ref HashCode hashCode, AlterQuery alterQuery)
    {
		// Code size: 111 (0x6f)
		/*
			int Id;
			Table Table;
			AlterQueryType Type;
			Column? Column;
			Constraint? Constraint;
			Index? Index;
			TableSpace? TableSpace;
		*/
		hashCode.Add(alterQuery.Id);
		hashCode.Add(alterQuery.Table.Id); // pair of identification for a table
        hashCode.Add(alterQuery.Table.SchemaId);
        hashCode.Add((int)alterQuery.Type);
		if (alterQuery.Column.HasValue) AddColumn(ref hashCode, alterQuery.Column.Value);
		if (alterQuery.Index is not null) AddIndex(ref hashCode, alterQuery.Index);
    }

	internal static void AddRetrieveQuery(this ref HashCode hashCode, RetrieveQuery retrieveQuery)
	{
		/*
			Table Table;
			RetrieveQueryType Type;
			int ParentQueryId;
			SpanList<RetrieveFilter> Filters;
			RetrieveSort? Sorts;
			PageInfo? Page;
		*/
		hashCode.AddTable(retrieveQuery.Table);
		hashCode.Add((int)retrieveQuery.Type);
		hashCode.Add(retrieveQuery.ParentQueryId);
		//foreach (var filter in retrieveQuery.Filters) AddFilter(ref hashCode, filter);
		//if (retrieveQuery.Sorts is not null) AddSort(ref hashCode, retrieveQuery.Sorts);
		//if (retrieveQuery.Page is not null) AddPage(ref hashCode, retrieveQuery.Page);
	}


	internal static void AddTable(this ref HashCode hashCode, Table table)
	{
		// Code size: 167 (0xa7)
		/* table definition: 
			int ObjectIndex;
			bool Cached;
			Field[] Fields; // sorted by name.
			Relation[] Relations; // sorted by name.
			Index[] Indexes;
			int RecordSize;
			Column[] Columns;		 // mix Fields and Relations.
			PhysicalType PhysicalType;
			int SchemaId;
			string? Subject;
			TableType Type;
			CacheId CacheId;
			string PhysicalName;
			bool AllowHardDeletion;
			bool Readonly;
			bool UsePreparedStatement;
			bool AllowAttributeExtension; 
		*/
		AddBaseEntity(ref hashCode, table);
		// Table-specific properties
		//hashCode.Add(table.ObjectIndex); // position of table in the schema, not connected to the identity of table ==> nok
		hashCode.Add(table.Cached); // ok 
		AddFields(ref hashCode, table.Fields); // check fields => ok
		AddRelations(ref hashCode, table.Relations); // check relations => ok
		AddIndexes(ref hashCode, table.Indexes); // check indexes => ok
		//hashCode.Add(table.RecordSize); // computed value => nok
		//AddColumns(ref hashCode, table.Columns); // columns => nok
		hashCode.Add((int)table.PhysicalType); // ok 
		hashCode.Add(table.SchemaId); // ok 
		hashCode.Add(table.Subject); // ok 
		hashCode.Add((int)table.Type); // ok 
		// hashCode.Add((int)table.CacheId); // nok 
		// hashCode.Add((int)table.PhysicalName); // computed => nok
		hashCode.Add(table.AllowHardDeletion); // ok 
		hashCode.Add(table.Readonly); // ok 
		hashCode.Add(table.UsePreparedStatement); // ok 
		hashCode.Add(table.AllowAttributeExtension); // ok 
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddBaseEntity(ref HashCode hashCode, BaseEntity baseEntity)
	{
		// Code size: 79 (0x4f) - no virtual calls
		hashCode.Add(baseEntity.Id);
		hashCode.Add(baseEntity.Name, StringComparer.Ordinal);
		if (baseEntity.Description is not null) hashCode.Add(baseEntity.Description, StringComparer.Ordinal);
		hashCode.Add(baseEntity.Baseline);
		hashCode.Add(baseEntity.Active);
	}

	private static void AddFields(ref HashCode hashCode, ReadOnlySpan<Field> fields)
	{
		// Code size: 38 (0x26)
		foreach (var field in fields) AddField(ref hashCode, field);
	}

	private static void AddRelations(ref HashCode hashCode, ReadOnlySpan<Relation> relations)
	{
		// Code size: 38 (0x26)
		foreach (var relation in relations) AddRelation(ref hashCode, relation);
	}

	private static void AddIndexes(ref HashCode hashCode, ReadOnlySpan<Index> indexes)
	{
		// Code size: 38 (0x26)
		foreach (var index in indexes) AddIndex(ref hashCode, index);
	}

	private static void AddColumns(ref HashCode hashCode, ReadOnlySpan<Column> columns)
	{
		// Code size: 38 (0x26)
		foreach (var column in columns) AddColumn(ref hashCode, column);
	}

	private static void AddStrings(ref HashCode hashCode, ReadOnlySpan<String?> strings)
	{
		// Code size: 50 (0x32)
		foreach (var str in strings) if (str is null) hashCode.Add(0); else hashCode.Add(str);
	}

	#endregion
}
