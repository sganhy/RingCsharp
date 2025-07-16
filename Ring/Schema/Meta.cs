using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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
	private const char HashCodeSeparator = (char)7777;

	// flags bit positions
	private const byte BitPositionFieldSearchableType = 5; // first position [bit 5,bit 10]
	private const byte BitPositionFieldNotNull = 3;
	private const byte BitPositionFieldMultilingual = 4;
	private const byte BitPositionIndexBitmap = 9;
	private const byte BitPositionIndexUnique = 10;
	private const byte BitPositionEntityBaseline = 14;
	private const byte BitPositionFirstPositionSize = 18;
	private const byte BitPositionFirstPositionRelType = 18;
	private const byte BitPositionRelationNotNull = 4;
	private const byte BitPositionRelationConstraint = 5;
	private const byte BitPositionTableCached = 9;
	private const byte BitPositionTableReadonly = 10;
	private const byte BitPositionTablespaceIndex = 11;
	private const byte BitPositionTablespaceTable = 12;

	#endregion 

	internal readonly int Id;
	internal readonly byte ObjectType;
	internal readonly int ReferenceId;
	internal readonly int DataType;
	internal readonly long Flags;
	internal readonly string Name;			// name of entity
	internal readonly string? Description;	// late loading 
	internal readonly string? Value;
	internal readonly bool Active;

	internal Meta(string name)
		: this(default, default, default, default, default, name, null, default, true) {}
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
	internal bool IsEntityBaseline => ReadFlag(BitPositionEntityBaseline);
	internal static long SetEntityBaseline(long flags, bool value) => WriteFlag(flags, BitPositionEntityBaseline, value);
	#endregion

	#region field methods
	internal FieldType GetFieldType() => (DataType & 127).ToFieldType(); // Code size: 15 (0xf)
	internal bool IsFieldNotNull() => ReadFlag(BitPositionFieldNotNull); // Code size: 8 (0x8)
	internal bool IsFieldMultilingual() => ReadFlag(BitPositionFieldMultilingual); // Code size: 8 (0x8)
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
	internal static long SetFieldNotNull(long flags, bool value) => WriteFlag(flags, BitPositionFieldNotNull, value); // Code size: 9 (0x9)
	internal static long SetFieldMultilingual(long flags, bool value) => WriteFlag(flags, BitPositionFieldMultilingual, value); // Code size: 9 (0x9)
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
	internal bool IsRelationNotNull => ReadFlag(BitPositionRelationNotNull);
	internal bool HasRelationConstraint => ReadFlag(BitPositionRelationConstraint);
	internal RelationType GetRelationType() => ((int)((Flags>>BitPositionFirstPositionRelType) & 127)).ToRelationType();
	internal static long SetRelationdNotNull(long flags, bool value) => WriteFlag(flags, BitPositionRelationNotNull, value);
	internal static long SetRelationConstraint(long flags, bool value) => WriteFlag(flags, BitPositionRelationConstraint, value);
	internal static long SetRelationType(long flags, RelationType type)
	{
		var temp = (long)type & 127L;
		// maxInt32 & size << ()
		flags &= 0x7FFFFFFFFC03FFFF;
		temp <<= BitPositionFirstPositionRelType;
		flags += temp;
		return flags;
	}
	#endregion

	#region index methods
	internal bool IsIndexBitmap => ReadFlag(BitPositionIndexBitmap);
	internal bool IsIndexUnique => ReadFlag(BitPositionIndexUnique);
	// index values
    internal Column[] GetIndexedColumns() => Value != null ? new Column[Value.CharCount(IndexColumnDelimiter)+1] : Array.Empty<Column>();
    internal static string? SetIndexedColumns(string[] columns) => string.Join(IndexColumnDelimiter, columns);
    
	// index flags 
    internal static long SetIndexUnique(long flags, bool value) => WriteFlag(flags, BitPositionIndexUnique, value);
	internal static long SetIndexBitmap(long flags, bool value) => WriteFlag(flags, BitPositionIndexBitmap, value);
	#endregion

	#region table methods
	internal static long SetTableReadonly(long flags, bool readonlyValue) => WriteFlag(flags, BitPositionTableReadonly, readonlyValue);
	internal static long SetTableCached(long flags, bool cached) => WriteFlag(flags, BitPositionTableCached, cached);
	internal bool IsTableReadonly => ReadFlag(BitPositionTableReadonly);
	internal bool IsTableCached => ReadFlag(BitPositionTableCached);
	#endregion

	#region parameter methods
	internal FieldType GetParameterValueType() => (DataType & 127).ToFieldType();
	internal ParameterType GetParameterType() => Id.ToParameterType();
	internal string GetParameterValue() => Value ?? string.Empty;
	internal static int SetParameterValueType(int dataType, FieldType valueType) => (dataType & 0xFFF8) + ((byte)valueType) & 127;
	#endregion

	#region tablespace methods
	internal static long SetTablespaceTable(long flags, bool isTablespaceTable) => WriteFlag(flags, BitPositionTablespaceTable, isTablespaceTable);
	internal static long SetTablespaceIndex(long flags, bool isTablespaceIndex) => WriteFlag(flags, BitPositionTablespaceIndex, isTablespaceIndex);
	internal bool IsTablespaceTable() => ReadFlag(BitPositionTablespaceTable);
	internal bool IsTablespaceIndex() => ReadFlag(BitPositionTablespaceIndex);
	#endregion

	internal static DbSchema GetEmptySchema(Meta meta, DatabaseProvider provider) // Code size: 90 (0x5a)
		=> new(meta.Id, meta.Name, provider.GetDdlBuilder().GetPhysicalName(EntityType.Schema,meta.Name), meta.Description, 
			Array.Empty<Parameter>(), Array.Empty<Lexicon>(), SchemaLoadType.Full, SchemaType.Undefined, Array.Empty<Sequence>(), 
			Array.Empty<Table>(), Array.Empty<Table>(), Array.Empty<TableSpace>(), provider, 0, meta.Active, meta.IsEntityBaseline);

	internal static Table GetEmptyTable(Meta meta) // Code size: 106 (0x6a)
		=> new(meta.Id, meta.Name, meta.Description, meta.Value, string.Empty,
			meta.DataType.ToTableType(), Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<Column>(),
			Array.Empty<Index>(), meta.ReferenceId, PhysicalType.Table, -1, 0, meta.IsEntityBaseline, meta.Active,
			meta.IsTableCached, meta.IsTableReadonly);

	internal static Index GetEmptyIndex(Meta meta) // Code size: 64 (0x40)
		=> new(meta.Id, meta.Name, meta.Name, meta.Description, meta.GetIndexedColumns(), meta.Value ?? string.Empty, meta.IsIndexUnique, 
			meta.IsIndexBitmap, meta.Active, meta.IsEntityBaseline);

	internal static Relation GetEmptyRelation(Meta meta, RelationType relationType, TableType toTableType) =>
		new(meta.Id, meta.Name, meta.Description, relationType,
			GetEmptyTable(new Meta(0, (byte)EntityType.Table, 0, (int)toTableType, 0L,
			meta.Name,null, null, false)), -1, FieldType.Undefined, false, false, true, true);

	internal static Field GetEmptyField(Meta meta, FieldType fieldType) =>
		new(meta.Id, meta.Name, meta.Description, fieldType, 0, null, SearchableType.None, true,
			false, false, true);
    internal static Meta? FirstOrDefault(Meta[] metas, EntityType entityType) 
	{
		Meta? result=null;
		var span = new ReadOnlySpan<Meta>(metas);
		var entityTypeId = (byte)entityType;
		for (var i = 0; i < span.Length; ++i) {
			var meta = span[i];
			if (entityTypeId == meta.ObjectType) return meta;
		}
		return result;
	}

	internal static Meta Create(int id,in Meta meta) =>
		new(id, meta.ObjectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, 
			meta.Description, meta.Value, meta.Active);

	internal EntityType GetEntityType() => ((int)ObjectType).ToEntityType();

	#region convertors 

	internal Relation? ToRelation(Table to)
	{
		// Code size: 134 (0x86)
		if (IsRelation)
		{
			var fieldType = FieldType.Undefined;
			if (to.Type == TableType.Business || to.Type == TableType.Lexicon)
				fieldType = to.Fields[to.Columns[0].RecordIndex].Type;
			return new Relation(Id, Name, 
				Description, GetRelationType(), to, -1, fieldType,IsRelationNotNull, HasRelationConstraint, IsEntityBaseline, Active);
		}
		return null;
	}
	
	internal Field? ToField() // Code size: 82 (0x52)
		=> IsField ? new Field(Id, Name, Description, GetFieldType(), 
			GetFieldSize(), GetFieldDefaultValue(), GetSearchableType(), IsEntityBaseline, IsFieldNotNull(), IsFieldMultilingual(), Active) : null;

	internal static DbSchema? ToSchema(Meta[] schema, DatabaseProvider provider, SchemaType type = SchemaType.Static, SchemaLoadType loadType = SchemaLoadType.Full)
	{
		// sort ASC by reference_id, name
		Array.Sort(schema, (x, y) => MetaSchemaComparer(x, y));
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
			var tableById = ShallowCopy(tableByName);
			var tableSpaces = GetTableSpaces(schema, ddlBuilder);

			// sort arrays - already pre-sorted by name
			Array.Sort(parameters, (x, y) => x.Id.CompareTo(y.Id));
			Array.Sort(tableById, (x, y) => x.Id.CompareTo(y.Id));

			// build schema to result
			var result = new DbSchema(meta.Value.Id, metaValue.Name, ddlBuilder.GetPhysicalName(EntityType.Schema, metaValue.Name), 
				metaValue.Description, parameters, lexicons.ToArray(), loadType, type, sequences.ToArray(), tableById.ToArray(), 
				tableByName.ToArray(), tableSpaces.ToArray(), provider, tableCount + mtmCount, metaValue.Active, metaValue.IsEntityBaseline);

			LoadRelations(result, schema, mtmCount);

			return result;
		}
		return null;
	}

	internal TableSpace? ToTableSpace(string physicalName) => IsTableSpace ? new TableSpace(Id, Name, physicalName, Description, 
		IsTablespaceIndex(), IsTablespaceTable(), false, Array.Empty<string>(), Value ?? string.Empty, Active, IsEntityBaseline) : null;

	internal Parameter? ToParameter()
	{
		var parameterType = GetParameterType();
		return IsParameter ? new Parameter(Id, Name, Description, parameterType,
			GetParameterValueType(), GetParameterValue(), parameterType.GetDefaultValue(), ReferenceId,
				IsEntityBaseline, Active) : null;
	}

	internal Index? ToIndex(string physicalName) // Code size: 65 (0x41)
		=> IsIndex ? new Index(Id, Name, physicalName, Description, GetIndexedColumns(), Value ?? string.Empty, IsIndexUnique, IsIndexBitmap, Active, IsEntityBaseline) : null;

	/// <summary>
	/// 	Create a instance of table, relation assigned later by schema creation
	/// </summary>
	internal Table? ToTable(ArraySegment<Meta> tableItems, PhysicalType physicalType, IDdlBuilder ddlBuilder, string physicalName, int objectIndex)
	{
		// Code size: 246 (0xf6)
		if (IsTable)
		{
			var tableType = DataType.ToTableType();
			var fields = GetFieldArray(tableItems, ddlBuilder);
			var relations = GetRelationArray(tableItems);
			var indexes = GetIndexes(tableItems, ddlBuilder);
			(var colCount, var relationCount) = GetColumnCount(fields, tableItems, ddlBuilder);

			// sort arrays (warn: relations not yet loaded here)
			Array.Sort(fields, (x, y) => string.CompareOrdinal(x.Name, y.Name));
			Array.Sort(indexes, (x, y) => string.CompareOrdinal(x.Name, y.Name));
			
			var table = new Table(Id, Name, Description, Value, physicalName,
				tableType, relations, fields, new Column[colCount], indexes,
				ReferenceId, physicalType, objectIndex, relations.Length + fields.Length + 1, IsEntityBaseline, Active, IsTableCached, IsTableReadonly);

			// load relations later , we need full schema to create relations
			// load columns
			LoadColumns(table, tableItems, relationCount, ddlBuilder);
            LoadIndexColumns(table);

            return table;
		}
		return null;
	}

	internal Column ToColumn(int id, string physicalName, int recordIndex, SearchableType? searchableType= SearchableType.None)
	{
		// Code size: 90 (0x5a)
		if (IsField)
		{
			// FieldType fieldType, EntityType type, string physicalName, SearchableType searchableType, int id, int recordIndex, int size
			return new Column(SearchableType.None == searchableType ? EntityType.Field : EntityType.SearchableColumn, GetFieldType(), physicalName, searchableType ?? SearchableType.None, id, recordIndex);
		} 
		else if (IsRelation) {
			return new Column(EntityType.Relation, FieldType.Long, physicalName, SearchableType.None, id, recordIndex); // TODO: computed later to get the right FieldType
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
	public readonly bool Equals(Meta other) =>
		Id == other.Id &&
		ObjectType == other.ObjectType &&
		ReferenceId == other.ReferenceId &&
		DataType == other.DataType &&
		Flags == other.Flags &&
		string.Equals(Name, other.Name, StringComparison.Ordinal) &&
		string.Equals(Description, other.Description, StringComparison.Ordinal) &&
		string.Equals(Value, other.Value, StringComparison.Ordinal) &&
		Active == other.Active;
	public override readonly bool Equals(object? obj) => obj is Record record && Equals(record);
	public override readonly int GetHashCode()
	{
		// Code size: 15 (0xf)
		HashHelper.Djb2X(GetStringCode(), out int hash);
		return hash;
	}

	// Code size: 190 (0xbe)
	internal readonly string GetStringCode()
		=> new StringBuilder()
			.Append(Id)
			.Append(HashCodeSeparator)
			.Append(ObjectType)
			.Append(HashCodeSeparator)
			.Append(ReferenceId)
			.Append(HashCodeSeparator)
			.Append(DataType)
			.Append(HashCodeSeparator)
			.Append(Flags)
			.Append(HashCodeSeparator)
			.Append(Name)
			.Append(HashCodeSeparator)
			.Append(Description)
			.Append(HashCodeSeparator)
			.Append(Value)
			.Append(HashCodeSeparator)
			.Append(Active).ToString();

    internal static int ColumnTypeWeight(EntityType entityType)
    {
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

	private static long WriteFlag(long flags, byte bitPosition, bool value)
	{ 
		// Code size: 35 (0x23)
		if (bitPosition < 65)
		{
			var mask = 1L;
			mask <<= bitPosition - 1;
			if (value) flags |= mask;
			else flags &= ~mask;
		}
		return flags;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool ReadFlag(byte bitPosition) => ((Flags >> (bitPosition - 1)) & 1) > 0; // Code size: 21 (0x15)

	private Index[] GetIndexes(ArraySegment<Meta> items, IDdlBuilder ddlBuilder)
	{
		// count element
		var indexCount = 0;
		var span = items.AsSpan();
		var table = GetEmptyTable(this);
		foreach (var item in span) if (item.IsIndex) ++indexCount;
		if (indexCount <= 0) return Array.Empty<Index>();
		var result = new Index[indexCount];
		var fieldIndex = 0;
		foreach (var item in span)
		{
			if (item.IsIndex)
			{
				var tempIndex = GetEmptyIndex(item);
				// cannot be null here 
#pragma warning disable CS8601 // Possible null reference assignment.
				result[fieldIndex] = item.ToIndex(ddlBuilder.GetPhysicalName(tempIndex, table));
#pragma warning restore CS8601
				++fieldIndex;
			}
		}
		return result;
	}

	private static TableSpace[] GetTableSpaces(Span<Meta> schema, IDdlBuilder ddlBuilder)
	{
		var result = new List<TableSpace>();
#pragma warning disable CS8604 // Possible null reference argument.
		foreach (var meta in schema) if (meta.IsTableSpace) 
			result.Add(meta.ToTableSpace(ddlBuilder.GetPhysicalName(EntityType.Tablespace, meta.Name)));
#pragma warning restore CS8604
		return result.ToArray();
	}

	private static Parameter[] GetParameters(Span<Meta> schema)
	{
		var result = new List<Parameter>();
#pragma warning disable CS8604 // Possible null reference argument.
		foreach (var meta in schema) if (meta.IsParameter) result.Add(meta.ToParameter());
#pragma warning restore CS8604
		return result.ToArray();
	}

	private static Field[] GetFieldArray(ArraySegment<Meta> items, IDdlBuilder ddlBuilder)
	{
		// Code size: 235 (0xeb)
		// count element
		int fieldCount = 0;
		var primaryKey = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Int);
		var span = items.AsSpan();
		foreach (var item in span)
		{
			if (item.IsField)
			{
				++fieldCount;
				if (string.Equals(primaryKey?.Name, item.Name, StringComparison.OrdinalIgnoreCase))
					primaryKey = primaryKey.GetDefaultPrimaryKey(item.GetFieldType());
			}
		}
		var result = new Field[fieldCount]; // allow once
		var fieldIndex = 0;
		foreach (var item in span)
		{
			if (item.IsField)
			{
#pragma warning disable CS8601 // Possible null reference assignment.
				result[fieldIndex] = string.Equals(primaryKey?.Name, item.Name, StringComparison.OrdinalIgnoreCase) ?
					primaryKey : item.ToField();
#pragma warning restore CS8601
				++fieldIndex;
			}
		}
		return result;
	}

	private static Relation[] GetRelationArray(ArraySegment<Meta> items)
	{
		// Code size: 71 (0x47)
		// count element
		var relationCount = 0;
		foreach (var item in items.AsSpan()) if (item.IsRelation) ++relationCount;
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

	private static Table[] GetTables(Meta[] schema, IDdlBuilder ddlBuilder, Meta metaSchema, DatabaseProvider provider,
		int mtmCount)
	{
		int startIndex, count, i = 0;
		var metaCount = schema.Length;
		var tableCount = metaCount > 400 ? metaCount / 4 : 100;
		var dico = new Dictionary<int, (int, int)>(tableCount); // table_id, start index , count
		var emptySchema = GetEmptySchema(metaSchema, provider);
		var schemaSpan = new ReadOnlySpan<Meta>(schema);

		//pass 1: build dico
		foreach (var meta in schemaSpan)
		{
			if (meta.IsField || meta.IsRelation || meta.IsIndex)
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
				var segment = dico.ContainsKey(meta.Id) ?
					new ArraySegment<Meta>(schema, dico[meta.Id].Item1, dico[meta.Id].Item2) :
					new ArraySegment<Meta>(schema, 0, 0);
				var physicalName = ddlBuilder.GetPhysicalName(GetEmptyTable(meta),emptySchema);
				var table = meta.ToTable(segment, PhysicalType.Table, ddlBuilder, physicalName, mtmCount + tableIndex);

#pragma warning disable CS8604 // Possible null reference argument.
				result.Add(table);
#pragma warning restore CS8604
				++tableIndex;
			}
		}
		return result.ToArray();
	}

	private static Table[] ShallowCopy(Span<Table> tables)
	{
		var result = new Table[tables.Length]; //Modify start & length as required
		tables.CopyTo(result.AsSpan());
		return result;
	}

	private static int MetaSchemaComparer(Meta meta1, Meta meta2)
	{
		// sort ASC by reference_id, name
		var result = meta1.ReferenceId.CompareTo(meta2.ReferenceId);
		if (result != 0) return result;
		return string.CompareOrdinal(meta1.Name, meta2.Name);
	}

	private static (int colCount, int relationCount) GetColumnCount(ReadOnlySpan<Field> fields, ArraySegment<Meta> tableItems, IDdlBuilder ddlBuilder)
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
		for (var i=0; i < tableItems.Count; ++i)
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
	private static void LoadColumns(Table table, ArraySegment<Meta> tableItems, int physRelationCount, IDdlBuilder ddlBuilder)
	{
		// Code size: 636 (0x27c)
		// relation are not yet loaded here !!!!
		var fieldCount = table.Fields.Length; // searchable fields + tz fields 
		var extraFieldCount = table.Columns.Length - physRelationCount - table.Fields.Length; // searchable fields + tz fields 
		var relationId = new int[physRelationCount]; 
		var extraFields = new Dictionary<string, Meta>(extraFieldCount*2); // increase bucket to reduce collisions
		var hasTimeZoneOffsetColumn = ddlBuilder.HasTimeZoneOffsetColumn;
		var relationIndex = 0;
		var columnIndex = 0;

		// cannot use yet table.GetFieldIndex() && table.GetRelationIndex() here !!!!
		// pass 1
		for (var i = 0; i < tableItems.Count; ++i)
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
		Array.Sort(relationId); // sort RelationId to compute during the second pass the relation RecordIndex

		// pass 2
		for (var i = 0; i < tableItems.Count; ++i)
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
					else table.Columns[columnIndex] = extraFields[field.Name].ToColumn(meta.Id, ddlBuilder.GetPhysicalName(EntityType.SearchableColumn, meta.Name), recordIndex);
					++columnIndex;
				}

				// time zone extra column ?
				if (field?.Type == FieldType.LongDateTime && hasTimeZoneOffsetColumn)
				{
					// meta not define for the searchable field
					if (!extraFields.ContainsKey(field.Name))
						table.Columns[columnIndex] = (SetObjectType(meta, TimeZoneColumnId)).ToColumn(id, ddlBuilder.GetPhysicalName(EntityType.TimeZoneColumn,
							meta.Id.ToString(CultureInfo.InvariantCulture)), recordIndex);
					else table.Columns[columnIndex] = extraFields[field.Name].ToColumn(meta.Id, ddlBuilder.GetPhysicalName(EntityType.TimeZoneColumn,
							meta.Id.ToString(CultureInfo.InvariantCulture)), recordIndex);
					++columnIndex;
				}
			} 
			else if (meta.IsRelation)
			{
				var relIndex = relationId.GetIndex(meta.Id);
				if (relIndex >= 0)
				{
					table.Columns[columnIndex] = meta.ToColumn(meta.Id, ddlBuilder.GetPhysicalName(EntityType.Relation, meta.Name), relIndex + fieldCount);
					++columnIndex;
				}
			}
		}
		Array.Sort(table.Columns, (x, y) => ColumnComparer(x, y));
	}

	private static void LoadIndexColumns(Table table)
	{
        // Code size: 100 (0x64)
        for (var i=0; i < table.Indexes.Length; ++i)
		{
			var index = table.Indexes[i];
			var logicalCols = index.ColumnList.Split(IndexColumnDelimiter);
			for (var j=0; j < logicalCols.Length; ++j)
			{
				var logicalName = logicalCols[j];
				index.Columns[j] = table.GetColumn(logicalName) ??
					new Column(EntityType.Undefined, FieldType.Undefined, string.Empty, SearchableType.None, 0, 0);
            }
		}
	}

    /// <summary>
    /// 	Load relationships objects into partial schema 
    /// </summary>
    /// <param name="schema">Partial built in schema</param>
    /// <param name="schemaItems">Sorted ASC by reference_id, name</param>
    private static void LoadRelations(DbSchema schema, ReadOnlySpan<Meta> schemaItems, int mtmCount)
	{
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
#pragma warning disable CS8601 // Possible null reference assignment. Cannot be null here !!
					fromTable.Relations[relationDicoIndex[fromTable.Id]] = relation;
#pragma warning restore CS8601
					++relationDicoIndex[fromTable.Id];
				}
			}
		}

		// load inverse relations
		LoadInverseRelations(schema, schemaItems);
		// load mtm relations
		LoadMtm(schema, mtmCount);
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
		var ddlBuilder = schema.Provider.GetDdlBuilder();
		var tableBuilder = new TableBuilder();
		var span = new Span<Table>(schema.TablesById);
		Table mtmTable;
		var mtm = new Dictionary<string, Table>(mtmCount * 2); // store mtm physical name
		foreach (var table in span)
		{
			for (var j = table.Relations.Length - 1; j >= 0; --j)
			{
				if (table.Relations[j].Type == RelationType.Mtm)
				{
					// step 1 - generate physical name
					var relation = table.Relations[j];
					var metaTable = new Meta(0, (byte)EntityType.Relation, 0, (int)TableType.Mtm, 0L, relation.GetMtmName(),
						null, null, true);
					var emptyTable = GetEmptyTable(metaTable);
					var physicalName = ddlBuilder.GetPhysicalName(emptyTable, schema);
					var inverseRelation = relation.InverseRelation;

					if (!mtm.ContainsKey(physicalName))
					{
						mtmTable = tableBuilder.GetMtm(emptyTable, ddlBuilder, physicalName, mtm.Count);
						//	step 2 - load relations - sort relation
						if (string.CompareOrdinal(relation.Name, inverseRelation.Name) < 0)
						{
							mtmTable.Relations[0] = relation.GetRelation(RelationType.Mto);
							mtmTable.Relations[1] = inverseRelation.GetRelation(RelationType.Mto);
						}
						else
						{
							mtmTable.Relations[1] = relation.GetRelation(RelationType.Mto);
							mtmTable.Relations[0] = inverseRelation.GetRelation(RelationType.Mto);
						}
						mtm.Add(physicalName, mtmTable);
					}
					else mtmTable = mtm[physicalName];
					// step 3 - create two new relations
					table.Relations[j] = CreateMtmRelation(relation, mtmTable, ddlBuilder);
					table.Relations[j].SetInverseRelation(inverseRelation);
				}
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
		// sort ASC by Id, then by Entity Type depend of weight see ColumnTypeWeight()
		var result = col1.Id.CompareTo(col2.Id);
		if (result != 0) return result;
		var w1 = ColumnTypeWeight(col1.Type);
		var w2 = ColumnTypeWeight(col2.Type);
		// define rank for entityType
		return w1.CompareTo(w2);
	}

	#endregion

}
