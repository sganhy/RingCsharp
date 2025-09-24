using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Runtime.CompilerServices;
using System.Globalization;
using DbSchema = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema;

internal readonly struct Meta : IEquatable<Meta>
{

	#region constants

	// entity type constants
	private const byte TableId = (byte)EntityType.Table;
	private const byte SchemaId = (byte)EntityType.Schema;
	private const byte FieldId = (byte)EntityType.Field;
	private const byte IndexId = (byte)EntityType.Index;
	private const byte RelationId = (byte)EntityType.Relation;
	private const byte SequenceId = (byte)EntityType.Sequence;
	private const byte TablespaceId = (byte)EntityType.Tablespace;
	private const byte ParameterId = (byte)EntityType.Parameter;
	private const byte SearchableColumnId = (byte)EntityType.SearchableColumn;
	private const byte TimeZoneColumnId = (byte)EntityType.TimeZoneColumn;

	private const char IndexColumnDelimiter = ';';

	// flags bit positions
	private const byte BitPositionFieldSearchableType = 5; // first position [bit 5,bit 10]
	private const byte BitPositionFirstPositionSize = 18;
	private const byte BitPositionFirstPositionRelType = 18;
	private static readonly FieldType DefaultColumnFieldType = FieldType.Long;
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	#endregion

	// minimizing data padding by field reordering - total: ~46 bytes + heap allocations for strings
	internal readonly long Flags;			// 8 bytes (offset 0) --> 
	internal readonly string Name;
	internal readonly string? Description;
	internal readonly string? Value;
	internal readonly int Id;				// 4 bytes (offset 32) -->
	internal readonly int ReferenceId;
	internal readonly int DataType;
	internal readonly byte ObjectType;		// 1 byte  (offset 44) -->
	internal readonly bool Active;

	internal Meta(string name) : this(default, default, default, default, default, name, null, default, true) {}
	internal Meta(int id, byte objectType, int referenceId, int dataType, long flags, string name, string? description, string? value, bool active)
	{
		Id = id;
		ObjectType = objectType;
		ReferenceId = referenceId;
		DataType = dataType;
		Flags = flags;
		Name = name;
		Description = description;	// late loading 
		Value = value;
		Active = active;
	}

	internal readonly bool IsTable => ObjectType == TableId;
	internal readonly bool IsSchema => ObjectType == SchemaId;
	internal readonly bool IsField => ObjectType == FieldId;
	internal readonly bool IsIndex => ObjectType == IndexId;
	internal readonly bool IsRelation => ObjectType == RelationId;
	internal readonly bool IsSequence => ObjectType == SequenceId;
	internal readonly bool IsTableSpace => ObjectType == TablespaceId;
	internal readonly bool IsParameter => ObjectType == ParameterId;
	internal readonly bool IsSearchableColumn => ObjectType == SearchableColumnId;
	internal readonly bool IsTimeZoneColumn => ObjectType == TimeZoneColumnId;

	#region entity methods 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsEntityBaseline() => (Flags & (long)MetaFlag.EntityBaseline)!=0; // Code size: 18 (0x12)
	internal static long SetEntityBaseline(long flags, bool value) => WriteFlag(flags, MetaFlag.EntityBaseline, value); // Code size: 12 (0xc)
	#endregion

	#region field methods
	internal FieldType GetFieldType() => (DataType & 127).ToFieldType(); // Code size: 15 (0xf)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsFieldNotNull() => (Flags & (long)MetaFlag.FieldNotNull)!=0; // Code size: 14 (0xe)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsFieldMultilingual() => (Flags & (long)MetaFlag.FieldMultilingual)!=0; // Code size: 14 (0xe)
	internal int GetFieldSize() => (int)((Flags >> (BitPositionFirstPositionSize-1)) & (int.MaxValue)); // Code size: 18 (0x12)
	internal SearchableType GetSearchableType() => ((int)((Flags >> (BitPositionFieldSearchableType-1)) & 0x3F)).ToSearchableType(); // Code size: 19 (0x13)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal string? GetFieldDefaultValue()
	{
		// Code size: 42 (0x2a)
		if (!string.IsNullOrEmpty(Value)) return Value;
		if (IsFieldNotNull()) return GetFieldType().GetDefaultValue();
		return null;
	}
	internal static int SetFieldType(int dataType, FieldType fieldType)
	{
		// Code size: 16 (0x10)
		dataType &= 0x7FFFFF80; // clear 7 first bits
		dataType += (int)fieldType;
		return dataType;
	}
	// field flags 
	internal static long SetFieldNotNull(long flags, bool value) => WriteFlag(flags, MetaFlag.FieldNotNull, value); // Code size: 10 (0xa)
	internal static long SetFieldMultilingual(long flags, bool value) => WriteFlag(flags, MetaFlag.FieldMultilingual, value); // Code size: 10 (0xa)
	internal static long SetFieldSize(long flags, int size)
	{
		// Code size: 15 (0xf)
		var temp = (long)size;
		// apply a mask here !!
		temp <<= BitPositionFirstPositionSize-1;
		flags += temp;
		return flags;
	}
	internal static long SetSearchableType(long flags, SearchableType searchableType) {
		// Code size: 14 (0xe)
		var temp = (int)searchableType;
		// apply a mask here !!
		temp <<= BitPositionFieldSearchableType-1;
		flags += temp;
		return flags;
	}

	#endregion

	#region relation methods
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsRelationNotNull() => (Flags & (long)MetaFlag.RelationNotNull) != 0; // Code size: 15 (0xf)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool HasRelationConstraint() => (Flags & (long)MetaFlag.RelationConstraint) != 0; // Code size: 15 (0xf)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal RelationType GetRelationType() => ((int)((Flags>>BitPositionFirstPositionRelType) & 127)).ToRelationType(); // Code size: 20 (0x14)

	internal static long SetRelationdNotNull(long flags, bool value) => WriteFlag(flags, MetaFlag.RelationNotNull, value); // Code size: 10 (0xa)
	internal static long SetRelationConstraint(long flags, bool value) => WriteFlag(flags, MetaFlag.RelationConstraint, value); // Code size: 11 (0xb) 
	internal static long SetRelationType(long flags, RelationType type)
	{
		// Code size: 32 (0x20)
		var temp = (long)type & 127L;
		// maxInt32 & size << ()
		flags &= 0x7FFFFFFFFC03FFFF;
		temp <<= BitPositionFirstPositionRelType;
		flags += temp;
		return flags;
	}
	#endregion

	#region index methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsIndexBitmap() => (Flags & (long)MetaFlag.IndexBitmap) != 0; // Code size: 18 (0x12)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsIndexUnique() => (Flags & (long)MetaFlag.IndexUnique) != 0; // Code size: 18 (0x12)

	// index values
	internal Column[] GetIndexedColumns() => Value != null ? new Column[Value.CharCount(IndexColumnDelimiter)+1] : Array.Empty<Column>();
	internal static string GetColumnList(string[] columns) => string.Join(IndexColumnDelimiter, columns);
	
	// index flags 
	internal static long SetIndexUnique(long flags, bool value) => WriteFlag(flags, MetaFlag.IndexUnique, value); // Code size: 14 (0xe)
	internal static long SetIndexBitmap(long flags, bool value) => WriteFlag(flags, MetaFlag.IndexBitmap, value); // Code size: 14 (0xe)
	#endregion

	#region table methods
	internal static long SetTableReadonly(long flags, bool value) => WriteFlag(flags, MetaFlag.TableReadonly, value); // Code size: 14 (0xe)
	internal static long SetTableCached(long flags, bool value) => WriteFlag(flags, MetaFlag.TableCached, value); // Code size: 14 (0xe)
	internal static long SetPhysicalDeletion(long flags, bool value) => WriteFlag(flags, MetaFlag.TableSoftDelete, !value); // Code size: 17 (0x11)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsTableReadonly() => (Flags & (long)MetaFlag.TableReadonly) != 0; // Code size: 18 (0x12)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsTableCached() => (Flags & (long)MetaFlag.TableCached) != 0; // Code size: 18 (0x12)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsPhysicalDeletion() => (Flags & (long)MetaFlag.TableSoftDelete) == 0; // Code size: 18 (0x12) (by default physical deletion)

	#endregion

	#region parameter methods
	internal FieldType GetParameterValueType() => (DataType & 127).ToFieldType(); // Code size: 15 (0xf)
	internal ParameterType GetParameterType() => Id.ToParameterType(); // Code size: 12 (0xc)
	internal string GetParameterValue() => Value ?? string.Empty;
	internal static int SetParameterValueType(int dataType, FieldType valueType) => (dataType & 0xFFF8) + ((byte)valueType) & 127;
	#endregion

	#region tablespace methods
	internal static long SetTablespaceTable(long flags, bool isTablespaceTable) => WriteFlag(flags, MetaFlag.TablespaceTable, isTablespaceTable); // Code size: 14 (0xe)
	internal static long SetTablespaceIndex(long flags, bool isTablespaceIndex) => WriteFlag(flags, MetaFlag.TablespaceIndex, isTablespaceIndex); // Code size: 14 (0xe)
	internal bool IsTablespaceTable() => (Flags & (long)MetaFlag.TablespaceTable) != 0; // Code size: 18 (0x12)
	internal bool IsTablespaceIndex() => (Flags & (long)MetaFlag.TablespaceIndex) != 0; // Code size: 18 (0x12)
	#endregion

	internal static DbSchema GetDefaultSchema(Meta meta, DatabaseProvider provider) // Code size: 90 (0x5a)
		=> new(meta.Id, meta.Name, provider.GetDdlBuilder().GetPhysicalName(EntityType.Schema,meta.Name), meta.Description, 
			Array.Empty<Parameter>(), Array.Empty<Lexicon>(), SchemaLoadType.Full, SchemaType.Undefined, Array.Empty<Sequence>(), 
			Array.Empty<Table>(), Array.Empty<Table>(), Array.Empty<TableSpace>(), provider, 0, meta.Active, meta.IsEntityBaseline());

	internal static Table GetDefaultTable(Meta meta) // Code size: 103 (0x67)
		=> new(meta.Id, meta.Name, meta.Description, meta.Value, string.Empty,
			meta.DataType.ToTableType(), Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<Column>(),
			Array.Empty<Index>(), meta.ReferenceId, PhysicalType.Table, -1, 0, meta.IsEntityBaseline(), meta.Active,
			meta.IsTableCached(), true, meta.IsTableReadonly());

	internal static Index GetDefaultIndex(Meta meta) // Code size: 64 (0x40)
		=> new(meta.Id, meta.Name, meta.Description, meta.GetIndexedColumns(), meta.Value ?? string.Empty, meta.IsIndexUnique(), 
			meta.IsIndexBitmap(), meta.Active, meta.IsEntityBaseline());

	internal static Relation GetDefaultRelation(Meta meta, RelationType relationType, TableType toTableType)
		=> new(meta.Id, meta.Name, meta.Description, relationType,
			GetDefaultTable(new Meta(0, (byte)EntityType.Table, 0, (int)toTableType, 0L,
			meta.Name,null, null, false)), FieldType.Undefined, false, false, true, true);

	internal static Field GetDefaultField(Meta meta, FieldType fieldType)
		=> new(meta.Id, meta.Name, meta.Description, fieldType, 0, null, SearchableType.None, true,
			false, false, true);

	internal static Meta Create(int id,in Meta meta)
		=> new(id, meta.ObjectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, 
			meta.Description, meta.Value, meta.Active);

	internal EntityType GetEntityType() => ((int)ObjectType).ToEntityType();

	#region convertors 

	internal Relation? ToRelation(Table to)
	{
		// Code size: 114 (0x72)
		if (IsRelation)
		{
			var fieldType = FieldType.Undefined;
			if (to.Type == TableType.Business || to.Type == TableType.Lexicon)
				fieldType = to.Fields[to.Columns[0].RecordIndex].Type;
			return new Relation(Id, Name, Description, GetRelationType(), to, fieldType,IsRelationNotNull(), HasRelationConstraint(), IsEntityBaseline(), Active);
		}
		return null;
	}
	
	internal Field? ToField() // Code size: 82 (0x52)
		=> IsField ? new Field(Id, Name, Description, GetFieldType(), 
			GetFieldSize(), GetFieldDefaultValue(), GetSearchableType(), IsEntityBaseline(), IsFieldNotNull(), IsFieldMultilingual(), Active) : null;

	/// <summary>
	///		The static method orchestrates the complex process of building a complete database schema object.
	/// </summary>
	internal static DbSchema? ToSchema(Meta[] schema, DatabaseProvider provider, SchemaType type = SchemaType.Static, SchemaLoadType loadType = SchemaLoadType.Full)
	{
		// Code size: 381 (0x17d)
		// sort ASC by reference_id, name
		schema.AsSpan().Sort(static (x, y) => MetaSchemaComparer(x, y));
		var meta = GetSchema(schema);
		if (meta.HasValue)
		{
			var metaValue = meta.Value;
			var ddlBuilder = provider.GetDdlBuilder();
			var mtmCount = GetMtmCount(schema);
			var tableCount = GetTableCount(schema);
			var parameters = GetParameters(schema);
			var lexicons = new List<Lexicon>();
			var sequences = new List<Sequence>();
			var tableByName = GetTables(schema, ddlBuilder, metaValue, provider, mtmCount);
			var tableById = new Table[tableByName.Length];
			tableByName.CopyTo(tableById,0);
			var tableSpaces = GetTableSpaces(schema, ddlBuilder);

			// sort arrays - already pre-sorted by name
			parameters.AsSpan().Sort(static (x, y) => x.Id.CompareTo(y.Id));
			tableById.AsSpan().Sort(static (x, y) => x.Id.CompareTo(y.Id));

			// build schema to result
			var result = new DbSchema(meta.Value.Id, metaValue.Name, ddlBuilder.GetPhysicalName(EntityType.Schema, metaValue.Name), 
				metaValue.Description, parameters, lexicons.ToArray(), loadType, type, sequences.ToArray(), tableById.ToArray(), tableByName.ToArray(), 
				tableSpaces.ToArray(), provider, tableCount + mtmCount, metaValue.Active, metaValue.IsEntityBaseline());

			LoadRelations(result, schema, mtmCount);

			return result;
		}
		return null;
	}

	internal TableSpace? ToTableSpace(string physicalName) => IsTableSpace ? new TableSpace(Id, Name, physicalName, Description, 
		IsTablespaceIndex(), IsTablespaceTable(), false, Array.Empty<string>(), Value ?? string.Empty, Active, IsEntityBaseline()) : null;

	internal Parameter? ToParameter()
	{
		// Code size: 86 (0x56)
		var parameterType = GetParameterType();
		var paramTemplate = ResourceHelper.GetParameter(parameterType);
		return IsParameter ? new Parameter(Id, Name, Description, parameterType,
			GetParameterValueType(), GetParameterValue(), paramTemplate.DefaultValue, ReferenceId, EntityType.Schema, IsEntityBaseline(), Active) : null;
	}

	internal Index? ToIndex() //Code size: 79 (0x4f)
		=> IsIndex ? new Index(Id, Name, Description, GetIndexedColumns(), Value ?? string.Empty, IsIndexUnique(), IsIndexBitmap(), Active, IsEntityBaseline()) : null;

	/// <summary>
	/// 	Create a instance of table, relation assigned later by schema creation
	/// </summary>
	internal Table? ToTable(ReadOnlySpan<Meta> tableItems, PhysicalType physicalType, IDdlBuilder ddlBuilder, string physicalName, int objectIndex)
	{
		// Code size: 268 (0x10c)
		if (IsTable)
		{
			var tableType = DataType.ToTableType();
			var fields = GetFieldArray(tableItems);
			var relations = GetRelationArray(tableItems);
			var indexes = GetIndexes(tableItems);
			var (colCount, physRelationCount) = GetColumnCount(fields, tableItems, ddlBuilder);

			// sort arrays (warn: relations not yet loaded here)
			fields.AsSpan().Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
			indexes.AsSpan().Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
			
			var table = new Table(Id, Name, Description, Value, physicalName,
				tableType, relations, fields, new Column[colCount], indexes,
				ReferenceId, physicalType, objectIndex, relations.Length + fields.Length + 1, IsEntityBaseline(), Active, IsTableCached(), IsPhysicalDeletion(), IsTableReadonly());

			// load relations later, we need full schema to create relations
			// load columns
			LoadColumns(table, tableItems, physRelationCount, ddlBuilder);
			LoadIndexColumns(table, tableItems, physRelationCount);

			return table;
		}
		return null;
	}

	internal Column ToColumn(int id, string physicalName, int recordIndex, SearchableType? searchableType= SearchableType.None)
	{
		// Code size: 144 (0x90)
		if (IsField)
		{
			// FieldType fieldType, EntityType type, string physicalName, SearchableType searchableType, int id, int recordIndex, int size
			return new Column(SearchableType.None == searchableType ? EntityType.Field : EntityType.SearchableColumn, GetFieldType(), physicalName, searchableType ?? SearchableType.None, id, recordIndex);
		} 
		else if (IsRelation) {
			return new Column(EntityType.Relation, DefaultColumnFieldType, physicalName, SearchableType.None, id, recordIndex);
		}
		else if (IsSearchableColumn)
		{
			return new Column(EntityType.SearchableColumn, FieldType.String, physicalName, GetSearchableType(), id, recordIndex);
		}
		else if (IsTimeZoneColumn)
		{
			return new Column(EntityType.TimeZoneColumn, FieldType.Short, physicalName, SearchableType.None, id, recordIndex);
		}
		return new Column(EntityType.Undefined, FieldType.Undefined, string.Empty, SearchableType.None, 0, 0); // default column --> throw an exception instead
	}

	#endregion

	public static bool operator ==(Meta left, Meta right) => left.Equals(right); // Code size: 9 (0x9)
	public static bool operator !=(Meta left, Meta right) => !left.Equals(right);
	public readonly bool Equals(Meta other) => // Code size: 150 (0x96)
		Id == other.Id &&
		ObjectType == other.ObjectType &&
		ReferenceId == other.ReferenceId &&
		DataType == other.DataType &&
		Flags == other.Flags &&
		string.Equals(Name, other.Name, StringComparison.Ordinal) &&
		string.Equals(Description, other.Description, StringComparison.Ordinal) &&
		string.Equals(Value, other.Value, StringComparison.Ordinal) &&
		Active == other.Active;
	
	public override readonly bool Equals(object? obj) => obj is Meta record && Equals(record); // Code size: 25 (0x19) - unbox.any present!!

	public override int GetHashCode()
	{
		// Code size: 119 (0x77)
		var result = Id.GetHashCode(); 
		result += ObjectType.GetHashCode();
		result += ReferenceId.GetHashCode();
		result += DataType.GetHashCode();
		result += ((int)Flags.GetHashCode() & int.MaxValue);
		result += GetStringHash(Name);
		result += GetStringHash(Description);
		result += GetStringHash(Value);
		result += GetStringHash(Active.ToString());
		return result;	
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int ColumnTypeWeight(EntityType entityType)
	{
		// Code size: 36 (0x24)
		switch (entityType)
		{
			case EntityType.Field: return 1;
			case EntityType.SearchableColumn: return 2;
			case EntityType.TimeZoneColumn: return 3;
			case EntityType.Relation: return 4;
		}
		return 5;
	}

#if DEBUG
	public override string ToString() => string.IsNullOrEmpty(Name) ? string.Empty : $"{Id} - {Name}";
#endif

	#region private methods 

	private static int GetStringHash(string? value)
	{
		// Code size: 15 (0xf) - using DJB2 algorithm for better hash distribution
		if (value == null) return 0;
		HashHelper.Djb2X(value, out int hash);
		return hash;
	}

	private static int GetTableCount(ReadOnlySpan<Meta> schema)
	{
		// Code size: 51 (0x33)
		var result = 0;
		foreach (var meta in schema) if (meta.IsTable) ++result;
		return result;
	}

	private static int GetMtmCount(ReadOnlySpan<Meta> schema)
	{
		// Code size: 63 (0x3f)
		var result = 0;
		foreach (var meta in schema) if (meta.IsRelation && meta.GetRelationType()==RelationType.Mtm) ++result;
		return result >> 1; // divided by 2
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static long WriteFlag(long flags, MetaFlag flag, bool value) => value ? flags | (long)flag : flags & (~((long)flag)); // Code size: 13 (0xd)

	private static Index[] GetIndexes(ReadOnlySpan<Meta> items)
	{
		// Code size: 150 (0x96)
		// count element
		var indexCount = 0;
		foreach (var item in items) if (item.IsIndex) ++indexCount;
		if (indexCount <= 0) return Array.Empty<Index>();
		var result = new Index[indexCount];
		var fieldIndex = 0;
		foreach (var item in items)
		{
			if (item.IsIndex)
			{
				result[fieldIndex] = item.ToIndex() ?? GetDefaultIndex(item);
				++fieldIndex;
			}
		}
		return result;
	}

	private static TableSpace[] GetTableSpaces(Span<Meta> schema, IDdlBuilder ddlBuilder)
	{
		// Code size: 91 (0x5b)
		var result = new List<TableSpace>();
		foreach (var meta in schema)
		{
			if (meta.IsTableSpace)
			{
				var tablespace = meta.ToTableSpace(ddlBuilder.GetPhysicalName(EntityType.Tablespace, meta.Name));
				if (tablespace!=null) result.Add(tablespace);
			}
		}
		return result.ToArray();
	}

	private static Parameter[] GetParameters(Span<Meta> schema)
	{
		// Code size: 77 (0x4d)
		var result = new List<Parameter>();
		foreach (var meta in schema) 
		{
			if (meta.IsParameter)
			{
				var parameter = meta.ToParameter();
				if (parameter!=null) result.Add(parameter);
			}
		}

		return result.ToArray(); // sorted by Id later !!!
	}

	private static Field[] GetFieldArray(ReadOnlySpan<Meta> items)
	{
		// Code size: 228 (0xe4)
		// count element
		int fieldCount = 0;
		var primaryKey = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Int);
		foreach (var item in items)
		{
			if (item.IsField)
			{
				++fieldCount;
				if (string.Equals(primaryKey?.Name, item.Name, StringComparison.OrdinalIgnoreCase))	primaryKey = primaryKey.GetDefaultPrimaryKey(item.GetFieldType());
			}
		}
		var result = new Field[fieldCount]; // allow once
		var fieldIndex = 0;
		foreach (var item in items)
		{
			if (item.IsField)
			{
				result[fieldIndex] = (string.Equals(primaryKey?.Name, item.Name, StringComparison.OrdinalIgnoreCase) ? primaryKey : item.ToField()) ?? GetDefaultField(item, item.GetFieldType());
				++fieldIndex;
			}
		}
		return result;
	}

	private static Relation[] GetRelationArray(ReadOnlySpan<Meta> items)
	{
		// Code size: 79 (0x4f)
		// count element
		var relationCount = 0;
		foreach (var item in items) if (item.IsRelation) ++relationCount;
		// relation are assigned later
		return relationCount > 0 ? new Relation[relationCount] : Array.Empty<Relation>();
	}

	private static Meta? GetSchema(Span<Meta> schema)
	{
		var i = 0;
		var count = schema.Length;
		while (i<count)
		{
			if (schema[i].IsSchema) return schema[i];
			++i;
		}
		return null;
	}

	private static Table[] GetTables(Meta[] schema, IDdlBuilder ddlBuilder, Meta metaSchema, DatabaseProvider provider,	int mtmCount)
	{
		// Code size: 449 (0x1c1)
		int startIndex, count, i = 0;
		var metaCount = schema.Length;
		var tableCount = metaCount > 400 ? metaCount / 4 : 100;
		var dico = new Dictionary<int, (int, int)>(tableCount); // table_id, start index , count
		var emptySchema = GetDefaultSchema(metaSchema, provider);
		var schemaSpan = new ReadOnlySpan<Meta>(schema);

		//pass 1: build dico
		foreach (var meta in schemaSpan)
		{
			if (meta.IsField || meta.IsRelation || meta.IsIndex || meta.IsSearchableColumn || meta.IsTimeZoneColumn)
			{
				if (!dico.ContainsKey(meta.ReferenceId)) dico.Add(meta.ReferenceId, (i, 0));
				(startIndex, count) = dico[meta.ReferenceId];
				dico[meta.ReferenceId] = (startIndex, count + 1);
			}
			++i;
		}

		//pass 2: create tableArray
		var result = new List<Table>(dico.Count);
		var tableIndex = 0;
		foreach (var meta in schemaSpan)
		{
			if (meta.IsTable)
			{
				var segment = dico.ContainsKey(meta.Id) ? new ReadOnlySpan<Meta>(schema, dico[meta.Id].Item1, dico[meta.Id].Item2) : new ReadOnlySpan<Meta>(schema, 0, 0);
				var physicalName = ddlBuilder.GetPhysicalName(GetDefaultTable(meta), emptySchema);
				var table = meta.ToTable(segment, PhysicalType.Table, ddlBuilder, physicalName, mtmCount + tableIndex) ?? GetDefaultTable(meta);
				result.Add(table);
				++tableIndex;
			}
		}
		return result.ToArray();
	}

	private static int MetaSchemaComparer(Meta meta1, Meta meta2)
	{
		// sort ASC by reference_id, name
		var result = meta1.ReferenceId.CompareTo(meta2.ReferenceId);
		if (result != 0) return result;
		return string.CompareOrdinal(meta1.Name, meta2.Name);
	}

	private static (int colCount, int relationCount) GetColumnCount(ReadOnlySpan<Field> fields, ReadOnlySpan<Meta> tableItems, IDdlBuilder ddlBuilder)
	{
		// Code size: 167 (0xa7)
		var count = fields.Length;
		var relationCount = 0;
		var hasTimeZoneOffsetColumn = ddlBuilder.HasTimeZoneOffsetColumn;
		
		// count fields 
		for (var i=0; i < fields.Length; ++i)
		{
			var field = fields[i];
			// searchable field ? 
			if (field.Type == FieldType.String && field.SearchableType != SearchableType.None) ++count;
			if (field.Type == FieldType.LongDateTime && hasTimeZoneOffsetColumn) ++count;
		}

		// count relations 
		for (var i=0; i < tableItems.Length; ++i)
		{
			var item = tableItems[i];
			if (item.IsRelation)
			{
				var relationType = item.GetRelationType();
				if (relationType == RelationType.Mto || relationType == RelationType.Otop)
				{
					++count;
					++relationCount;
				}
			}
		}
		return (count, relationCount);
	}

	/// <summary>
	/// 	Load Table.RecordIndexes[] & Table.Columns[]
	/// </summary>
	private static void LoadColumns(Table table, ReadOnlySpan<Meta> tableItems, int physRelationCount, IDdlBuilder ddlBuilder)
	{
		// Code size: 846 (0x34e)
		// relation are not yet loaded here !!!!
		var fieldCount = table.Fields.Length; // searchable fields + tz fields 
		var extraFieldCount = table.Columns.Length - physRelationCount - table.Fields.Length; // searchable fields + tz fields 
		Span<int> relationId = physRelationCount <= 64 ? stackalloc int[physRelationCount] : new int[physRelationCount]; 
		var extraFields = new Dictionary<string, Meta>(extraFieldCount*2); // increase bucket to reduce collisions
		var hasTimeZoneOffsetColumn = ddlBuilder.HasTimeZoneOffsetColumn;
		var relationIndex = 0;
		var columnIndex = 0;

		// cannot use yet table.GetFieldIndex() && table.GetRelationIndex() here !!!!
		// pass 1
		for (var i = 0; i < tableItems.Length; ++i)
		{
			var meta = tableItems[i];
			if (meta.IsSearchableColumn || meta.IsTimeZoneColumn) extraFields.Add(meta.Name, meta);
			else if (meta.IsRelation)
			{
				var relType = meta.GetRelationType();
				if (relType == RelationType.Mto || relType == RelationType.Otop)
				{
					relationId[relationIndex] = meta.Id;
					++relationIndex;
				}
			}
		}
		relationId.Sort(); // sort RelationId to compute during the second pass the relation RecordIndex

		// pass 2
		for (var i = 0; i < tableItems.Length; ++i)
		{
			var meta = tableItems[i];
			if (meta.IsField)
			{
				var field = table.GetField(meta.Name);
				var id = field?.Id ?? 1;
				var recordIndex = table.GetFieldIndex(meta.Name);
				table.Columns[columnIndex] = meta.ToColumn(id, ddlBuilder.GetPhysicalName(EntityType.Field, meta.Name), recordIndex);
				++columnIndex;

				// searchable field ?
				if (field?.Type == FieldType.String && field.SearchableType != SearchableType.None)
				{
					// meta not define for the searchable field
					if (!extraFields.ContainsKey(field.Name))
						table.Columns[columnIndex] = meta.ToColumn(id, ddlBuilder.GetPhysicalName(EntityType.SearchableColumn, meta.Name), recordIndex, field.SearchableType);
					else 
					{
						var metaExtra = extraFields[field.Name];
						table.Columns[columnIndex] = metaExtra.ToColumn(metaExtra.Id, ddlBuilder.GetPhysicalName(EntityType.SearchableColumn, meta.Name), recordIndex);
					}
					++columnIndex;
				}

				// time zone extra column ?
				if (field?.Type == FieldType.LongDateTime && hasTimeZoneOffsetColumn)
				{
					// meta not define for the searchable field
					if (!extraFields.ContainsKey(field.Name))
						table.Columns[columnIndex] = (SetObjectType(meta, TimeZoneColumnId)).ToColumn(id, ddlBuilder.GetPhysicalName(EntityType.TimeZoneColumn,
							meta.Id.ToString(DefaultCulture)), recordIndex);
					else table.Columns[columnIndex] = extraFields[field.Name].ToColumn(meta.Id, ddlBuilder.GetPhysicalName(EntityType.TimeZoneColumn,
							meta.Id.ToString(DefaultCulture)), recordIndex);
					++columnIndex;
				}
			} 
			else if (meta.IsRelation)
			{
				var recordIndex = relationId.GetIndex(meta.Id);
				if (recordIndex >= 0)
				{
					table.Columns[columnIndex] = meta.ToColumn(meta.Id, ddlBuilder.GetPhysicalName(EntityType.Relation, meta.Name), recordIndex + fieldCount);
					++columnIndex;
				}
			}
		}
		Array.Sort(table.Columns, (x, y) => ColumnComparer(x, y));
	}

	private static void LoadIndexColumns(Table table, ReadOnlySpan<Meta> tableItems, int physRelationCount)
	{
		Dictionary<string, int>? relDico = null;
		var defaultCol = new Column(EntityType.Undefined, FieldType.Undefined, string.Empty, SearchableType.None, 0, 0);
		if (physRelationCount > 0)
		{
			relDico = new Dictionary<string, int>(physRelationCount * 2); // allow bucket
			for (var i = 0; i < tableItems.Length; ++i)
			{
				var meta = tableItems[i];
				if (meta.IsRelation)
				{
					var relType = meta.GetRelationType();
					if (relType == RelationType.Mto || relType == RelationType.Otop) relDico.Add(meta.Name, meta.Id);
				}
			}
		}
		// relation is not yet loaded here !!
		for (var i=0; i < table.Indexes.Length; ++i)
		{
			var index = table.Indexes[i];
			var logicalCols = index.ColumnList.Split(IndexColumnDelimiter);
			for (var j=0; j < logicalCols.Length; ++j)
			{
				var logicalName = logicalCols[j];
				// field ?
				var fieldIndex = table.GetFieldIndex(logicalName);
				if (fieldIndex >= 0) index.Columns[j] = table.GetColumn(logicalName) ?? defaultCol;
				else 
				{
					if (relDico != null && relDico.ContainsKey(logicalName))
						index.Columns[j] = table.GetColumn(relDico[logicalName], EntityType.Relation) ?? defaultCol;
				}

			}
		}
	}

	/// <summary>
	/// 	Load relationships object into partial schema 
	/// </summary>
	private static void LoadRelations(DbSchema schema, ReadOnlySpan<Meta> schemaItems, int mtmCount)
	{
		// Code size: 260 (0x104)
		var relationDicoIndex = new Dictionary<int, int>(schema.TablesById.Length * 2); // (tableId, relation index)

		// load dico
		foreach (var table in new ReadOnlySpan<Table>(schema.TablesById)) relationDicoIndex.Add(table.Id, 0);

		// load relation
		foreach (var meta in schemaItems)
		{
			if (meta.IsRelation)
			{
				var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
				var toTable = schema.GetTable(meta.DataType);
				if (toTable != null && fromTable != null)
				{
					var relation = meta.ToRelation(toTable);
					fromTable.Relations[relationDicoIndex[fromTable.Id]] = relation ?? GetDefaultRelation(meta,RelationType.Undefined,TableType.Undefined);
					++relationDicoIndex[fromTable.Id];
				}
			}
		}

		// load inverse relations
		LoadInverseRelations(schema, schemaItems);
		// load mtm relations
		LoadMtm(schema, mtmCount);
		// load relation type columns
		LoadTypeColumns(schema);
	}

	private static void LoadInverseRelations(DbSchema schema, ReadOnlySpan<Meta> schemaItems)
	{
		// Code size: 127 (0x7f)
		foreach (var meta in schemaItems)
		{
			if (meta.IsRelation)
			{
				var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
				if (fromTable != null)
				{
					var relation = fromTable.GetRelation(meta.Name);
					var invRelation = relation?.ToTable.GetRelation(meta.Value ?? string.Empty);
					if (relation != null && invRelation != null) relation.SetInverseRelation(invRelation);
				}
			}
		}
	}

	private static void LoadMtm(DbSchema schema, int mtmCount)
	{
		// Code size: 371 (0x173)
		var ddlBuilder = schema.Provider.GetDdlBuilder();
		var tableBuilder = new TableBuilder();
		var span = new Span<Table>(schema.TablesById);
		var mtm = new Dictionary<string, Table>(mtmCount * 2); // store mtm physical name
		foreach (var table in span)
		{
			for (var j = table.Relations.Length - 1; j >= 0; --j)
			{
				if (table.Relations[j].Type == RelationType.Mtm)
				{
					// step 1 - generate physical name
					
					var relation = table.Relations[j];
					var metaTable = new Meta(0, (byte)EntityType.Relation, 0, (int)TableType.Mtm, 0L, relation.GetMtmName(), null, null, true);
					var emptyTable = GetDefaultTable(metaTable);
					var physicalName = ddlBuilder.GetPhysicalName(emptyTable, schema);
					var inverseRelation = relation.InverseRelation;

					Table mtmTable;
					if (!mtm.ContainsKey(physicalName))
					{
						mtmTable = tableBuilder.GetMtm(emptyTable, ddlBuilder, physicalName, mtm.Count,
							string.CompareOrdinal(relation.Name, inverseRelation.Name) < 0 ? relation.SetTypeAndId(RelationType.Mto,1, true) 
							: inverseRelation.SetTypeAndId(RelationType.Mto,1, true),
							string.CompareOrdinal(relation.Name, inverseRelation.Name) < 0 ? inverseRelation.SetTypeAndId(RelationType.Mto,2, true) 
							: relation.SetTypeAndId(RelationType.Mto, 2, true));
						mtm.Add(physicalName, mtmTable);
					}
					else mtmTable = mtm[physicalName];

					// step 2 - create two new relations
					table.Relations[j] = CreateMtmRelation(relation, mtmTable, ddlBuilder);
					table.Relations[j].SetInverseRelation(inverseRelation);
				}
			}
		}
	}

	private static void LoadTypeColumns(DbSchema schema)
	{
		// Code size: 45 (0x2d)
		var spanTable = new Span<Table>(schema.TablesByName);
		foreach (var table in spanTable) LoadTypeColumns(table);
	}
	private static void LoadTypeColumns(Table table)
	{
		// Code size: 144 (0x90)
		var relations = new Span<Relation>(table.Relations);
		foreach (var relation in relations)
		{
			if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
			{
				var index = table.GetColumnIndex(relation.Id, EntityType.Relation);
				if (index >= 0)
				{
					var column = table.Columns[index];
					// change if necessary
					if (column.FieldType!= relation.FieldType) 
						table.Columns[index] = column.SetFieldType(relation.FieldType);
				}
			}
			if (relation.Type == RelationType.Mtm)
			{
				// load mtm table columns
				LoadTypeColumns(relation.ToTable);
			}
		}
	}

	private static Meta SetObjectType(Meta meta, byte objectType) =>
		new (meta.Id, objectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, meta.Description, meta.Value, meta.Active);

	private static Relation CreateMtmRelation(Relation relation, Table mtmTable, IDdlBuilder ddlBuilder)
	{
		var meta = relation.ToMeta(0);
		return meta.ToRelation(mtmTable);
	}

	private static int ColumnComparer(Column col1, Column col2)
	{
		// sort ASC by ID, then by Entity Type depend on weight see ColumnTypeWeight()
		var result = col1.Id.CompareTo(col2.Id);
		if (result != 0) return result;
		var w1 = ColumnTypeWeight(col1.Type);
		var w2 = ColumnTypeWeight(col2.Type);
		// define rank for entityType
		return w1.CompareTo(w2);
	}

	#endregion

}
