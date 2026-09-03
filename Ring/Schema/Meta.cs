using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema;

/// <summary>
/// 	The Meta struct is a compact, memory-optimized data structure that serves as a universal container for database logical schema metadata. 
/// </summary>
internal readonly struct Meta : IEquatable<Meta>
{

	// BUGS & IMPROVEMENTS:
	//     1) GetTables() pass 1: Wrong segment start index → wrong field slice per table; Severity: High (not a bug)
	//     2) SetRelationType(): Wrong bitmask → corrupts adjacent flags; Severity: Medium (Done)
	//     3) GetSearchableType(): bit shift is off by one; Severity: Low (Done)
	//     4) GetFieldSize(): bit shift is off by one; Severity: Low (not a bug)

	#region constants
	private static readonly Meta DefaultMetaRelation = GetDefaultMeta(EntityType.Relation);
	private static readonly Meta DefaultMetaField = GetDefaultMeta(EntityType.Field);
	private const char CsvSeparator = ',';
	private const char CsvStringTag = '"';

	// entity type constants
	private const byte TableId = (byte)EntityType.Table;
	private const byte SchemaId = (byte)EntityType.Schema;
	private const byte FieldId = (byte)EntityType.Field;
	private const byte IndexId = (byte)EntityType.Index;
	private const byte RelationId = (byte)EntityType.Relation;
	private const byte SequenceId = (byte)EntityType.Sequence;
	private const byte TablespaceId = (byte)EntityType.Tablespace;
	private const byte ConstraintId = (byte)EntityType.Constraint;
	private const byte ParameterId = (byte)EntityType.Parameter;
	private const byte SearchableColumnId = (byte)EntityType.SearchableColumn;
	private const byte TimeZoneColumnId = (byte)EntityType.TimeZoneColumn;
	private const char IndexColumnDelimiter = ';';
	private const char ConstraintColumnDelimiter = IndexColumnDelimiter;


	// flags bit positions
	private const byte BitPositionFieldSearchableType = 5; // bits 4-9  (6 bits, positions 4..9)
	private const byte BitPositionFirstPositionSize = 18; // bits 17.. (used as shift - 1 = 17)
	private const byte BitCountFieldSize = 31; // max size = 2147483647 ( (2^31) - 1)
	private const byte BitCountConstraintProvider = 9; // max size = 2147483647 ( (2^31) - 1)
	private const byte BitShiftFieldSize = BitPositionFirstPositionSize - 1; // = 17
	private const byte BitShiftConstraintProvider = 16; // should be greater than baseline(14)
	private const long MaskFieldSize = ((1L << BitCountFieldSize) - 1L) << BitShiftFieldSize;
	private const long MaskConstraintProvider = ((1L << BitCountConstraintProvider) - 1L) << BitShiftConstraintProvider;
	private const byte BitCountFieldSearchableType = 6;     // 6 bits → max value 63
	private const long MaskSearchableType = ((1L << BitCountFieldSearchableType) - 1L) << BitPositionFieldSearchableType;
	private const byte BitPositionFirstPositionRelType = 20;
	private const long MaskRelationType = 127L << BitPositionFirstPositionRelType;
	private static readonly FieldType DefaultColumnFieldType = FieldType.Long;
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly CacheId DefaultCacheId = new(-1L,long.MinValue,0);
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture);
	private static readonly string MetaSchemaId = "0";
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

	internal readonly bool IsTable => ObjectType == TableId; // Code size: 10 (0xa)
	internal readonly bool IsSchema => ObjectType == SchemaId;
	internal readonly bool IsField => ObjectType == FieldId;
	internal readonly bool IsIndex => ObjectType == IndexId;
	internal readonly bool IsRelation => ObjectType == RelationId;
	internal readonly bool IsSequence => ObjectType == SequenceId;
	internal readonly bool IsTableSpace => ObjectType == TablespaceId;
	internal readonly bool IsConstraint => ObjectType == ConstraintId;
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsFieldAllowTruncation() => (Flags & (long)MetaFlag.FieldAllowTruncation) != 0; // Code size: 18 (0x12)
	internal int GetFieldSize() => (int)((Flags >> BitShiftFieldSize) & ((1L << BitCountFieldSize) - 1L)); // Code size: 18 (0x12)
	internal SearchableType GetSearchableType() => ((int)((Flags >> BitPositionFieldSearchableType) & ((1L << BitCountFieldSearchableType) - 1L))).ToSearchableType(); // Code size: 19 (0x13)

	internal static int SetFieldType(int dataType, FieldType fieldType)
	{
		// Code size: 16 (0x10)
		dataType &= 0x7FFFFF80; // clear 7 first bits
		dataType += (int)fieldType;
		return dataType;
	}
	// field flags 
	internal static long SetFieldNotNull(long flags, bool value) => WriteFlag(flags, MetaFlag.FieldNotNull, value); // Code size: 10 (0xa)
	internal static long SetFieldAllowTruncation(long flags, bool value) => WriteFlag(flags, MetaFlag.FieldAllowTruncation, value); // Code size: 14 (0xe)
	internal static long SetFieldMultilingual(long flags, bool value) => WriteFlag(flags, MetaFlag.FieldMultilingual, value); // Code size: 10 (0xa)
	internal static long SetFieldSize(long flags, int size)
	{
		// Code size: 33 (0x21)
		var clampedSize = size & ((1L << BitCountFieldSize) - 1L); // guard against overflow into adjacent bits
		flags &= ~MaskFieldSize;          // clear existing size bits
		flags |= clampedSize << BitShiftFieldSize; // write new value
		return flags;
	}
	internal static long SetSearchableType(long flags, SearchableType searchableType) {
		// Code size: 24 (0x18)
		flags &= ~MaskSearchableType;                                              // clear existing bits
		flags |= ((long)searchableType & ((1L << BitCountFieldSearchableType) - 1L)) << BitPositionFieldSearchableType; // write new value
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

	internal static long SetRelationNotNull(long flags, bool value) => WriteFlag(flags, MetaFlag.RelationNotNull, value); // Code size: 10 (0xa)
	internal static long SetRelationConstraint(long flags, bool value) => WriteFlag(flags, MetaFlag.RelationConstraint, value); // Code size: 11 (0xb) 
	internal static long SetRelationType(long flags, RelationType type)
	{
		// Code size: 25 (0x19)
		flags &= ~MaskRelationType;                          // clear bits 20-26 exactly
		flags |= ((long)type & 127L) << BitPositionFirstPositionRelType; // write new value
		return flags;
	}
	#endregion

	#region index methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsIndexBitmap() => (Flags & (long)MetaFlag.IndexBitmap) != 0; // Code size: 18 (0x12)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsIndexUnique() => (Flags & (long)MetaFlag.IndexUnique) != 0; // Code size: 18 (0x12)

	// index values
	internal Column[] GetIndexedColumns() => Value is not null ? new Column[Value.GetCharCount(IndexColumnDelimiter)+1] : Array.Empty<Column>(); // fill array later - Code size: 35 (0x23)
	internal static string GetColumnList(string[] columns) => string.Join(IndexColumnDelimiter, columns);

	// index flags 
	internal static long SetIndexUnique(long flags, bool value) => WriteFlag(flags, MetaFlag.IndexUnique, value); // Code size: 14 (0xe)
	internal static long SetIndexBitmap(long flags, bool value) => WriteFlag(flags, MetaFlag.IndexBitmap, value); // Code size: 14 (0xe)
	#endregion

	#region table methods
	internal static long SetTableReadonly(long flags, bool value) => WriteFlag(flags, MetaFlag.TableReadonly, value); // Code size: 14 (0xe)
	internal static long SetTableCached(long flags, bool value) => WriteFlag(flags, MetaFlag.TableCached, value); // Code size: 14 (0xe)
	internal static long SetTableAllowAttributeExtension(long flags, bool value) => WriteFlag(flags, MetaFlag.TableAttributeExtension, value); // Code size: 14 (0xe)
	internal static long SetPhysicalDeletion(long flags, bool value) => WriteFlag(flags, MetaFlag.TableHardDelete, value); // Code size: 17 (0x11)
	internal static long SetPreparedStatement(long flags, bool value) => WriteFlag(flags, MetaFlag.TablePreparedStatement, value); // Code size: 17 (0x11)
	internal bool IsTableReadonly() => (Flags & (long)MetaFlag.TableReadonly) != 0; // Code size: 18 (0x12) - (by default writable)
	internal bool IsTableCached() => (Flags & (long)MetaFlag.TableCached) != 0; // Code size: 18 (0x12)
	internal bool IsTableAllowAttributeExtension() => (Flags & (long)MetaFlag.TableAttributeExtension) != 0; // Code size: 18 (0x12)
	internal bool IsPhysicalDeletion() => (Flags & (long)MetaFlag.TableHardDelete) != 0; // Code size: 18 (0x12) (by default physical deletion)
	internal bool HasPreparedStatement() => (Flags & (long)MetaFlag.TablePreparedStatement) != 0; // Code size: 15 (0xf) (by default no prepared statement)

	#endregion

	#region parameter methods
	internal FieldType GetParameterValueType() => (DataType & 127).ToFieldType(); // Code size: 15 (0xf)
	internal ParameterType GetParameterType() => Id.ToParameterType(); // Code size: 12 (0xc)
	internal string GetParameterValue() => Value ?? string.Empty;
	#endregion

	#region constraint methods
	internal ConstraintType GetConstraintType() => DataType.ToConstraintType(); // Code size: 12 (0xc)
	internal DatabaseProvider GetDatabaseProvider() => ((int)((Flags >> BitShiftConstraintProvider) & ((1L << BitCountConstraintProvider) - 1L))).ToDatabaseProvider(); // Code size: 23 (0x17)
	internal static long SetDatabaseProvider(long flags, DatabaseProvider databaseProvider)
	{
		// Code size: 30 (0x1e)
		var clampedProvider = ((int)databaseProvider) & ((1L << BitCountConstraintProvider) - 1L); // guard against overflow into adjacent bits
		flags &= ~MaskConstraintProvider;          // clear existing size bits
		flags |= clampedProvider << BitShiftConstraintProvider; // write new value
		return flags;
	}

	#endregion

	#region tablespace methods
	internal static long SetTablespaceTable(long flags, bool isTablespaceTable) => WriteFlag(flags, MetaFlag.TablespaceTable, isTablespaceTable); // Code size: 14 (0xe)
	internal static long SetTablespaceIndex(long flags, bool isTablespaceIndex) => WriteFlag(flags, MetaFlag.TablespaceIndex, isTablespaceIndex); // Code size: 14 (0xe)
	internal bool IsTablespaceTable() => (Flags & (long)MetaFlag.TablespaceTable) != 0; // Code size: 18 (0x12)
	internal bool IsTablespaceIndex() => (Flags & (long)MetaFlag.TablespaceIndex) != 0; // Code size: 18 (0x12)
	#endregion

	internal static DbSchema GetDefaultSchema(in Meta meta, DatabaseProvider provider) // Code size: 90 (0x5a)
		=> new(meta.Id, meta.Name, provider.GetDdlBuilder().GetPhysicalName(EntityType.Schema,meta.Name), meta.Description, 
			Array.Empty<Parameter>(), Array.Empty<Lexicon>(), SchemaLoadType.Full, SchemaType.Undefined, Array.Empty<Sequence>(), 
			Array.Empty<Table>(), Array.Empty<Table>(), Array.Empty<TableSpace>(), provider, 0, meta.Active, meta.IsEntityBaseline());

	internal static Table GetDefaultTable(in Meta meta) // Code size: 103 (0x67)
		=> new(meta.Id, meta.Name, meta.Description, meta.Value, string.Empty,
			meta.DataType.ToTableType(), Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<Column>(),
			Array.Empty<Index>(), Array.Empty<Constraint>(), meta.ReferenceId, PhysicalType.Table, -1, 0, DefaultCacheId, meta.IsEntityBaseline(), meta.Active,
			meta.IsTableCached(), true, meta.IsTableReadonly(), meta.HasPreparedStatement(), meta.IsTableAllowAttributeExtension());
	internal static Constraint GetDefaultConstraint(in Meta meta, ConstraintType constraintType) // Code size: 103 (0x67)
		=> new(meta.Id, meta.Name, meta.Description, meta.IsEntityBaseline(), meta.Active, constraintType, Array.Empty<Column>(), null, null);

	internal static Index GetDefaultIndex(in Meta meta) // Code size: 64 (0x40)
		=> new(meta.Id, meta.Name, meta.Description, meta.GetIndexedColumns(), meta.IsIndexUnique(), 
			meta.IsIndexBitmap(), meta.Active, meta.IsEntityBaseline());

	internal static Relation GetDefaultRelation(in Meta meta, RelationType relationType, TableType toTableType) // Code size: 56 (0x38)
		=> new(meta.Id, meta.Name, meta.Description, relationType,
			GetDefaultTable(new Meta(0, (byte)EntityType.Table, 0, (int)toTableType, 0L,
			meta.Name,null, null, false)), FieldType.Undefined, false, false, true, true);

	internal static Meta GetDefaultMeta(EntityType  entityType) // Code size: 20 (0x14)
		=> new (0, (byte)entityType, 0, 0, 0L,	string.Empty, null, null, false);

	internal static Field GetDefaultField(in Meta meta, FieldType fieldType) // Code size: 33 (0x21)
		=> new(meta.Id, meta.Name, meta.Description, fieldType, 0, null, null, SearchableType.None, true, false, false, true, true);

	internal static Meta Create(string name) => new(default, default, default, default, default, name, null, null, true);
	internal static char GetIndexColumnDelimiter() => IndexColumnDelimiter; // Code size: 3 (0x3)
	internal EntityType GetEntityType() => ((int)ObjectType).ToEntityType(); // Code size: 12 (0xc)

	#region convertors 

	internal string ToCsv() // Code size: 208 (0xd0)
		=> new StringBuilder().Append(Id).Append(CsvSeparator).Append(ObjectType).Append(CsvSeparator).Append(ReferenceId).Append(CsvSeparator).Append(DataType).Append(CsvSeparator)
		.Append(Flags).Append(CsvSeparator).Append(CsvStringTag).Append(Name).Append(CsvStringTag).Append(CsvSeparator).Append(CsvStringTag).Append(Description).Append(CsvStringTag)
		.Append(CsvSeparator).Append(CsvStringTag).Append(Value).Append(CsvStringTag).Append(CsvSeparator).Append(Active).ToString();

	internal Relation? ToRelation(Table to)
	{
		// Code size: 114 (0x72)
		// BUG : Unsafe array access without bounds check — Line 255 — High Severity
		if (IsRelation)
		{
			var fieldType = FieldType.Undefined;
			if (to.Type == TableType.Business || to.Type == TableType.Lexicon)
				fieldType = to.Fields[to.Columns[0].RecordIndex].Type;
			return new Relation(Id, Name, Description, GetRelationType(), to, fieldType,IsRelationNotNull(), HasRelationConstraint(), IsEntityBaseline(), Active);
		}
		return null;
	}

	internal Field? ToField()
	{
		// Code size: 121 (0x79)
		if (IsField)
		{
			var isNotNull = IsFieldNotNull();
			var fieldType = GetFieldType();
			var effectiveValue = isNotNull ? Value ?? fieldType.GetDefaultValue() : Value;
			return new Field(Id, Name, Description, fieldType, GetFieldSize(), Value, effectiveValue, GetSearchableType(), IsEntityBaseline(), isNotNull, IsFieldMultilingual(), IsFieldAllowTruncation(), Active);
		}
		return null;
	}

	internal Constraint? ToConstraint()
	{
		// Code size: 123 (0x7b)
		if (IsConstraint)
		{
			var columnCount = Math.Max(0,Value.GetCharCount(ConstraintColumnDelimiter)-1); // min columnCount=0
			var minValue = GetLongValue(Value, 1, ConstraintColumnDelimiter);
			var maxValue = GetLongValue(Value, 2, ConstraintColumnDelimiter);
			return new Constraint(Id, Name, Description, IsEntityBaseline(), Active, GetConstraintType(), columnCount <=0 ? Array.Empty<Column>() : new Column[columnCount], minValue, maxValue);
		}
		return null;
	}

	/// <summary>
	///		The static method orchestrates the complex process of building a complete database schema object.
	/// </summary>
	internal static DbSchema? ToSchema(Meta[] schema, DatabaseProvider provider, SchemaType type = SchemaType.Static, SchemaLoadType loadType = SchemaLoadType.Full, Table[]? prebuiltTables = null)
	{
		// Code size: 387 (0x183)
		// sort ASC by reference_id, name
		// prebuiltTables: table array should be sorted by name, if not, sort it before passing to this method
		schema.AsSpan().Sort(static (x, y) => MetaSchemaComparer(x, y));
		var meta = GetSchema(schema);
		if (meta.HasValue)
		{
			var metaValue = meta.Value;
			var ddlBuilder = provider.GetDdlBuilder();
			var mtmCount = GetMtmCount(schema);
			var tableCount = prebuiltTables is null ? GetTableCount(schema) : prebuiltTables.Length; 
			var parameters = GetParameters(schema);
			var lexicons = GetLexicons(schema);
			var sequences = GetSequences(schema);
			var tableByName = prebuiltTables ?? GetTables(schema, ddlBuilder, metaValue, provider, mtmCount, tableCount);
			var tableById = new Table[tableByName.Length];
			tableByName.CopyTo(tableById,0);
			var tableSpaces = GetTableSpaces(schema, ddlBuilder);

			// sort arrays - already pre-sorted by name
			parameters.AsSpan().Sort(static (x, y) => x.Id.CompareTo(y.Id));
			tableById.AsSpan().Sort(static (x, y) => x.Id.CompareTo(y.Id));

			// build schema to result - 
			// ObjectCount <-- table count + mtm count + view count
			var result = new DbSchema(meta.Value.Id, metaValue.Name, ddlBuilder.GetPhysicalName(EntityType.Schema, metaValue.Name), 
				metaValue.Description, parameters, lexicons, loadType, type, sequences, tableById, tableByName, 
				tableSpaces, provider, tableCount + mtmCount, metaValue.Active, metaValue.IsEntityBaseline());

			LoadRelations(result, schema, mtmCount);

			return result;
		}
		return null;
	}

	internal TableSpace? ToTableSpace(string physicalName) => IsTableSpace ? new TableSpace(Id, Name, physicalName, Description, 
		IsTablespaceIndex(), IsTablespaceTable(), false, Array.Empty<string>(), Value ?? string.Empty, Active, IsEntityBaseline()) : null;

	internal Parameter? ToParameter() // Code size: 71 (0x47)
		=> IsParameter ? new Parameter(Id, Name, Description, GetParameterType(), GetParameterValueType(), GetParameterValue(), EntityType.Schema, ReferenceId, IsEntityBaseline(), Active) : null;

	internal Index? ToIndex() //Code size: 79 (0x4f)
		=> IsIndex ? new Index(Id, Name, Description, GetIndexedColumns(), IsIndexUnique(), IsIndexBitmap(), Active, IsEntityBaseline()) : null;

	/// <summary>
	/// 	Create a instance of table, relation assigned later by schema creation
	/// </summary>
	internal Table? ToTable(ReadOnlySpan<Meta> tableItems, PhysicalType physicalType, IDdlBuilder ddlBuilder, string physicalName, int objectIndex)
	{
		// Code size: 320 (0x140)
		if (IsTable)
		{
			var tableType = DataType.ToTableType();
			var fields = GetFieldArray(tableItems, tableType);
			var relations = GetRelationArray(tableItems);
			var indexes = GetIndexes(tableItems);
			var (colCount, physRelationCount, constraintCount) = GetCount(fields, tableItems, ddlBuilder);
			var recordSize = physRelationCount + fields.Length + 1;

			// sort arrays (warn: relations not yet loaded here)
			fields.AsSpan().Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
			indexes.AsSpan().Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
			
			var table = new Table(Id, Name, Description, Value, physicalName, tableType, relations, fields, new Column[colCount], indexes, new Constraint[constraintCount], 
				ReferenceId, physicalType, objectIndex, recordSize, GetCacheId(tableType), IsEntityBaseline(), Active, IsTableCached(), IsPhysicalDeletion(), IsTableReadonly(), 
				HasPreparedStatement(), IsTableAllowAttributeExtension());

			// load relations later, we need full schema to create relations
			// load columns
			LoadColumns(table, tableItems, physRelationCount, ddlBuilder);
			LoadItemColumns(table, tableItems, physRelationCount); // load indexes & constraints columns
			LoadConstraints(table, tableItems,	constraintCount, ddlBuilder.Provider);

			return table;
		}
		return null;
	}

	internal Column ToColumn(int id, string physicalName, int recordIndex, SearchableType? searchableType= SearchableType.None)
	{
		// Code size: 148 (0x94)
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

	internal Record ToRecord(Table table) // Code size: 217 (0xd9) - TODO: throw an exception if tableType is not equal to TableType.Meta
		=> table.Type == TableType.Meta ? new(table,new string?[] { Active? BooleanTrue : BooleanFalse, DataType.ToString(DefaultCulture), Description, Flags.ToString(DefaultCulture), Id.ToString(DefaultCulture), 
			Name, ObjectType.ToString(DefaultCulture), ReferenceId.ToString(DefaultCulture), MetaSchemaId, Value, null },0) 
			: throw new ArgumentException(string.Format(DefaultCulture,ResourceHelper.GetMessage(ResourceType.UnexpectedTableType), table.Type));

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
		// Code size: 133 (0x85) - no virtual calls
		var hashCode = new HashCode();
		hashCode.Add(Id); //1
		hashCode.Add(ObjectType);
		hashCode.Add(ReferenceId);
		hashCode.Add(DataType);
		hashCode.Add(Flags); //5 
		hashCode.Add(Name);
		hashCode.Add(Description);
		hashCode.Add(Value);
		hashCode.Add(Active); //9
		return hashCode.ToHashCode();	
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
	public override string ToString() => string.IsNullOrEmpty(Name) ? string.Empty : $"{IntExtensions.ToEntityType((int)ObjectType)} {Id} - {Name}";
#endif

	#region private methods 
	private static int GetTableCount(ReadOnlySpan<Meta> schema)
	{
		// Code size: 43 (0x2b)
		var result = 0;
		foreach (ref readonly Meta meta in schema) if (meta.IsTable) ++result;
		return result;
	}

	private static int GetMtmCount(ReadOnlySpan<Meta> schema)
	{
		// Code size: 56 (0x38)
		var result = 0;
		foreach (ref readonly Meta meta in schema) if (meta.IsRelation && meta.GetRelationType()==RelationType.Mtm) ++result;
		return result >> 1; // divided by 2
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static long WriteFlag(long flags, MetaFlag flag, bool value) => value ? flags | (long)flag : flags & (~(long)flag); // Code size: 12 (0xc)

	private static Index[] GetIndexes(ReadOnlySpan<Meta> items)
	{
		// Code size: 146 (0x92) - no virtual calls
		// count element
		var indexCount = 0;
		foreach (var item in items) if (item.IsIndex) ++indexCount;
		if (indexCount <= 0) return Array.Empty<Index>();
		var result = new Index[indexCount];
		var fieldIndex = 0;
		foreach (ref readonly Meta item in items)
		{
			if (item.IsIndex)
			{
				result[fieldIndex] = item.ToIndex() ?? GetDefaultIndex(item);
				++fieldIndex;
			}
		}
		return result;
	}

	private static TableSpace[] GetTableSpaces(ReadOnlySpan<Meta> schema, IDdlBuilder ddlBuilder)
	{
		// Code size: 84 (0x54)
		var result = new List<TableSpace>();
		foreach (ref readonly Meta meta in schema)
		{
			if (meta.IsTableSpace)
			{
				var tablespace = meta.ToTableSpace(ddlBuilder.GetPhysicalName(EntityType.Tablespace, meta.Name));
				if (tablespace is not null) result.Add(tablespace);
			}
		}
		return result.ToArray();
	}

	private static Sequence[] GetSequences(ReadOnlySpan<Meta> schema) => Array.Empty<Sequence>();
	private static Lexicon[] GetLexicons(ReadOnlySpan<Meta> schema) => Array.Empty<Lexicon>();

	private static Parameter[] GetParameters(ReadOnlySpan<Meta> schema)
	{
		// Code size: 70 (0x46)
		var result = new List<Parameter>();
		foreach (ref readonly Meta meta in schema) 
		{
			if (meta.IsParameter)
			{
				var parameter = meta.ToParameter();
				if (parameter is not null) result.Add(parameter);
			}
		}

		return result.ToArray(); // sorted by Id later !!!
	}

	private static Field[] GetFieldArray(ReadOnlySpan<Meta> items, TableType tableType)
	{
		// Code size: 236 (0xec)
		int fieldCount = 0;
		// no default primary keys for non-business tables, use built-in field instead!
		var primaryKey = tableType != TableType.Business ? GetDefaultField(DefaultMetaField, FieldType.Undefined) : FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Int);

		foreach (ref readonly Meta item in items)
		{
			if (item.IsField)
			{
				++fieldCount;
				if (string.Equals(primaryKey?.Name, item.Name, StringComparison.OrdinalIgnoreCase))	primaryKey = primaryKey.GetDefaultPrimaryKey(item.GetFieldType());
			}
		}
		var result = new Field[fieldCount]; // allow once
		var fieldIndex = 0;
		foreach (ref readonly Meta item in items)
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
		// Code size: 58 (0x3a)
		// count element
		var relationCount = 0;
		foreach (ref readonly Meta item in items) if (item.IsRelation) ++relationCount;
		// relation are assigned later
		return relationCount > 0 ? new Relation[relationCount] : Array.Empty<Relation>();
	}

	private static Meta? GetSchema(Span<Meta> schema)
	{
		// Code size: 82 (0x52)
		var i = 0;
		var count = schema.Length;
		while (i<count)
		{
			if (schema[i].IsSchema) return schema[i];
			++i;
		}
		return null;
	}

	private static Table[] GetTables(Meta[] schema, IDdlBuilder ddlBuilder, in Meta metaSchema, DatabaseProvider provider,	int mtmCount, int tableCount)
	{
		// Code size: 393 (0x189)
		// bug : pass 1 : Incorrect segment start index — High Severity
		var dico = new Dictionary<int, (int, int)>(tableCount * 2); // table_id, (start index, count)
		var emptySchema = GetDefaultSchema(metaSchema, provider);
		var schemaSpan = new ReadOnlySpan<Meta>(schema);
		var i = 0;

		// pass 1: build dico
		foreach (ref readonly Meta meta in schemaSpan)
		{
			if (meta.IsField || meta.IsRelation || meta.IsIndex || meta.IsSearchableColumn || meta.IsTimeZoneColumn || meta.IsConstraint)
			{
				if (dico.TryGetValue(meta.ReferenceId, out var existing))
					dico[meta.ReferenceId] = (existing.Item1, existing.Item2 + 1);
				else
					dico[meta.ReferenceId] = (i, 1); // record first child index, not outer counter
			}
			++i;
		}

		// pass 2: create tableArray
		var result = new Table[tableCount];
		var tableIndex = 0;
		foreach (ref readonly Meta meta in schemaSpan)
		{
			if (meta.IsTable)
			{
				var segment = dico.TryGetValue(meta.Id, out var range) ? new ReadOnlySpan<Meta>(schema, range.Item1, range.Item2) : ReadOnlySpan<Meta>.Empty;
				var physicalName = ddlBuilder.GetPhysicalName(GetDefaultTable(meta), emptySchema);
				var tableType = meta.DataType.ToTableType();
				var table = meta.ToTable(segment, tableType.ToPhysicalType(), ddlBuilder, physicalName, mtmCount + tableIndex)
					?? GetDefaultTable(meta);
				result[tableIndex] = table;
				++tableIndex;
			}
		}
		return result;
	}

	private static int MetaSchemaComparer(in Meta meta1,in Meta meta2)
	{
		// sort ASC by reference_id, name
		var result = meta1.ReferenceId.CompareTo(meta2.ReferenceId);
		if (result != 0) return result;
		return string.CompareOrdinal(meta1.Name, meta2.Name);
	}

	private static (int colCount, int relationCount, int constraintCount) GetCount(ReadOnlySpan<Field> fields, ReadOnlySpan<Meta> tableItems, IDdlBuilder ddlBuilder)
	{
		// Code size: 220 (0xdc)
		var count = fields.Length;
		var relationCount = 0;
		var constraintCount = 0;
		var hasTimeZoneOffsetColumn = ddlBuilder.HasTimeZoneOffsetColumn;
		var databaseProvider = ddlBuilder.Provider;

		// count fields 
		for (var i=0; i < fields.Length; ++i)
		{
			var field = fields[i];
			// searchable field ? 
			if (field.Type == FieldType.String && field.SearchableType != SearchableType.None) ++count;
			if (field.Type == FieldType.DateTimeOffset && hasTimeZoneOffsetColumn) ++count;
		}

		// count relations 
		foreach (ref readonly var item in tableItems)
		{
			if (item.IsRelation)
			{
				var relationType = item.GetRelationType();
				if (relationType == RelationType.Mto || relationType == RelationType.Otop)
				{
					++count;
					++relationCount;
				}
			}
			if (item.IsConstraint)
			{
				var itemProvider = item.GetDatabaseProvider();
				// test databaseProvider ?
				if (itemProvider == DatabaseProvider.Undefined || itemProvider == databaseProvider) 
					++constraintCount; // constraint specific to a DB provider.
			}
		}
		return (count, relationCount, constraintCount);
	}


	/// <summary>
	/// 	Load Table.RecordIndexes[] & Table.Columns[]
	/// </summary>
	[SkipLocalsInit]
	private static void LoadColumns(Table table, ReadOnlySpan<Meta> tableItems, int physRelationCount, IDdlBuilder ddlBuilder)
	{
		// Code size: 848 (0x350)
		// relation are not yet loaded here !!!!
		// BUG : pass 2: Wrong id fallback — Medium Severity (not really a bug)
		// relation are not yet loaded here !!!!
		var fieldCount = table.Fields.Length;
		var extraFieldCount = table.Columns.Length - physRelationCount - table.Fields.Length; // searchable fields + tz fields
		Span<int> relationId = physRelationCount <= 64 ? stackalloc int[physRelationCount] : new int[physRelationCount];
		var extraFields = new Dictionary<string, Meta>(extraFieldCount * 2); // increase bucket to reduce collisions
		var hasTimeZoneOffsetColumn = ddlBuilder.HasTimeZoneOffsetColumn;
		var relationIndex = 0;
		var columnIndex = 0;

		// cannot use yet table.GetFieldIndex() && table.GetRelationIndex() here !!!!
		// pass 1
		foreach (ref readonly var meta in tableItems)
		{
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
		foreach (ref readonly var meta in tableItems)
		{
			if (meta.IsField)
			{
				var field = table.GetField(meta.Name);
				var id = field?.Id ?? meta.Id;            // BUG 2 fix: use meta.Id instead of magic value 1
				var recordIndex = table.GetFieldIndex(meta.Name);
				table.Columns[columnIndex] = meta.ToColumn(id, ddlBuilder.GetPhysicalName(EntityType.Field, meta.Name), recordIndex);
				++columnIndex;

				// searchable field ?
				if (field?.Type == FieldType.String && field.SearchableType != SearchableType.None)
				{
					if (extraFields.TryGetValue(field.Name, out var metaExtra))
						table.Columns[columnIndex] = metaExtra.ToColumn(metaExtra.Id, ddlBuilder.GetPhysicalName(EntityType.SearchableColumn, meta.Name), recordIndex);
					else
						table.Columns[columnIndex] = meta.ToColumn(id, ddlBuilder.GetPhysicalName(EntityType.SearchableColumn, meta.Name), recordIndex, field.SearchableType);

					++columnIndex;
				}

				// time zone extra column ?
				if (field?.Type == FieldType.DateTimeOffset && hasTimeZoneOffsetColumn)
				{
					if (extraFields.TryGetValue(field.Name, out var metaExtra))
						table.Columns[columnIndex] = metaExtra.ToColumn(metaExtra.Id,  // BUG 3 fix: use metaExtra.Id, not meta.Id
							ddlBuilder.GetPhysicalName(EntityType.TimeZoneColumn, meta.Id.ToString(DefaultCulture)), recordIndex);
					else
						table.Columns[columnIndex] = SetObjectType(meta, TimeZoneColumnId).ToColumn(id,
							ddlBuilder.GetPhysicalName(EntityType.TimeZoneColumn, meta.Id.ToString(DefaultCulture)), recordIndex);
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

	private static void LoadItemColumns(Table table, ReadOnlySpan<Meta> tableItems, int physRelationCount)
	{
		// Code size: 355 (0x163)
		if (table.Indexes.Length <= 0) return;
		Dictionary<string, int>? relDico = null;
		var defaultCol = new Column(EntityType.Undefined, FieldType.Undefined, string.Empty, SearchableType.None, 0, 0);
		if (physRelationCount > 0)
		{
			relDico = new Dictionary<string, int>(physRelationCount * 2); // <relation_logical_name, relation_id>
			foreach (ref readonly var meta in tableItems)
			{
				if (meta.IsRelation)
				{
					var relType = meta.GetRelationType();
					if (relType == RelationType.Mto || relType == RelationType.Otop) relDico.Add(meta.Name, meta.Id);
				}
			}
		}
		// relation is not yet loaded here !!
		foreach (ref readonly var meta in tableItems)
		{
			if (meta.IsIndex)
			{
				var index = table.GetIndex(meta.Name);
				if (index is null) continue;
				var columnList = meta.Value;
				if (columnList is null) continue;
				var logicalCols = columnList.Split(IndexColumnDelimiter);
				for (var j = 0; j < logicalCols.Length && j < index.Columns.Length; ++j)
				{
					var logicalName = logicalCols[j];
					// field ?
					var fieldIndex = table.GetFieldIndex(logicalName);
					if (fieldIndex >= 0) index.Columns[j] = table.GetColumn(logicalName) ?? defaultCol;
					else
					{
						if (relDico is not null && relDico.TryGetValue(logicalName, out var relationId))
							index.Columns[j] = table.GetColumn(relationId, EntityType.Relation) ?? defaultCol;
					}
				}
			}
		}
	}

	private static void LoadConstraints(Table table, ReadOnlySpan<Meta> tableItems, int constraintCount, DatabaseProvider databaseProvider)
	{
		// Code size: 241 (0xf1)
		if (constraintCount <= 0) return;
		var defaultCol = new Column(EntityType.Undefined, FieldType.Undefined, string.Empty, SearchableType.None, 0, 0);
		var constraintIndex = 0;
		foreach (var meta in tableItems)
		{
			if (meta.IsConstraint) 
			{
				var metaProvider = meta.GetDatabaseProvider();
				if (metaProvider != DatabaseProvider.Undefined && metaProvider!= databaseProvider) continue;
				var constraint = meta.ToConstraint();
				if (constraint is null) continue;
				// load columns
				table.Constraints[constraintIndex++] = constraint;
				var colCount = constraint.Columns.Length;
				var arr = meta.Value?.Split(ConstraintColumnDelimiter) ?? Array.Empty<string>();
				var columnIndex=0;
				for (var j=2; j < arr.Length; ++j) if (columnIndex < colCount) constraint.Columns[columnIndex++] = table.GetColumn(arr[j]) ?? defaultCol;
			}
		}
	}

	/// <summary>
	/// 	Load relationships object into partial schema 
	/// </summary>
	private static void LoadRelations(DbSchema schema, ReadOnlySpan<Meta> schemaItems, int mtmCount)
	{
		// Code size: 255 (0xff)
		var relationDicoIndex = new Dictionary<int, int>(schema.TablesById.Length * 2); // (tableId, relation index)

		// load dico
		foreach (var table in new ReadOnlySpan<Table>(schema.TablesById)) relationDicoIndex.Add(table.Id, 0);

		// load relation
		foreach (ref readonly var meta in schemaItems)
		{
			if (meta.IsRelation)
			{
				var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
				var toTable = schema.GetTable(meta.DataType);
				if (toTable is not null && fromTable is not null)
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
		// Code size: 121 (0x79)
		foreach (ref readonly var meta in schemaItems)
		{
			if (meta.IsRelation)
			{
				var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
				if (fromTable is not null)
				{
					var relation = fromTable.GetRelation(meta.Name);
					var invRelation = relation?.ToTable.GetRelation(meta.Value ?? string.Empty);
					if (relation is not null && invRelation is not null) relation.SetInverseRelation(invRelation);
				}
			}
		}
	}

	private static void LoadMtm(DbSchema schema, int mtmCount)
	{
		// Code size: 350 (0x15e) - boxing removed
		// BUG — inverseRelation dereferenced without null check
		var ddlBuilder = schema.Provider.GetDdlBuilder();
		var span = new Span<Table>(schema.TablesById);
		var mtm = new Dictionary<string, Table>(mtmCount * 2); // store mtm physical name
		foreach (var table in span)
		{
			for (var j = table.Relations.Length - 1; j >= 0; --j)
			{
				if (table.Relations[j].Type == RelationType.Mtm)
				{
					// step 1 - generate physical name
					Relation relation = table.Relations[j];
					var metaTable = new Meta(0, (byte)EntityType.Relation, 0, (int)TableType.Mtm, 0L, TableType.Mtm.GetLogicalName(relation.GetMtmName()), null, null, true);
					var emptyTable = GetDefaultTable(metaTable);
					var physicalName = ddlBuilder.GetPhysicalName(emptyTable, schema);
					Relation inverseRelation = relation.InverseRelation;

					if (!mtm.TryGetValue(physicalName, out var mtmTable))
					{
						var compareLessThan0 = string.CompareOrdinal(relation.Name, inverseRelation.Name) < 0;
						mtmTable = GetMtm(emptyTable, ddlBuilder, physicalName, mtm.Count,
							compareLessThan0 ? relation.SetTypeAndId(RelationType.Mto,1, true) : inverseRelation.SetTypeAndId(RelationType.Mto,1, true),
							compareLessThan0 ? inverseRelation.SetTypeAndId(RelationType.Mto,2, true) : relation.SetTypeAndId(RelationType.Mto, 2, true));
						mtm.Add(physicalName, mtmTable);
					}

					// step 2 - create two new relations -- mtmTable cannot be null here
					table.Relations[j] = CreateMtmRelation(relation, mtmTable);
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
		// BUG: Potential infinite recursion on cyclic MTM graphs → impossible due to mtm relation are created with type RelationType.Mto and not RelationType.Mtm
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

	private static Meta SetObjectType(in Meta meta, byte objectType) => // Code size: 55 (0x37)
		new (meta.Id, objectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, meta.Description, meta.Value, meta.Active);

	private static Relation CreateMtmRelation(Relation relation, Table mtmTable)
	{
		// Code size: 44 (0x2c)
		var meta = relation.ToMeta(0);
		return meta.ToRelation(mtmTable) ?? GetDefaultRelation(DefaultMetaRelation, RelationType.Undefined,TableType.Undefined);
	}

	/// <summary>
	/// Returns appropriate CacheId based on table type.
	/// Business tables get a new CacheId, others get default.
	/// </summary>
	private static CacheId GetCacheId(TableType tableType)
	{
		// Code size: 16 (0x10)
		return tableType == TableType.Business? new CacheId() : DefaultCacheId;
	}

	private static int ColumnComparer(in Column col1, in Column col2)
	{
		// Code size: 56 (0x38)
		// sort ASC by ID, then by Entity Type depend on weight see ColumnTypeWeight()
		var result = col1.Id.CompareTo(col2.Id);
		if (result != 0) return result;
		var w1 = ColumnTypeWeight(col1.Type);
		var w2 = ColumnTypeWeight(col2.Type);
		// define rank for entityType
		return w1.CompareTo(w2);
	}

	private static long? GetLongValue(string? value, int index, char separator) 
	{
		// Code size: 135 (0x87)
		if (value is not null)
		{
			var endIndex = value.IndexOfOccurrence(separator, index);
			var startIndex = value.IndexOfOccurrence(separator, index - 1);
			string subResult= string.Empty; 
			if (endIndex >= 0 && startIndex >= 0) subResult = value.Substring(startIndex + 1, endIndex - startIndex - 1);
			if (endIndex >= 0 && startIndex < 0) subResult = value[..endIndex];
			if (endIndex < 0 && startIndex >= 0) subResult = value.Substring(startIndex + 1, value.Length - startIndex - 1);
			if (endIndex < 0 && startIndex < 0) subResult = value;
			if (long.TryParse(subResult, out long result)) return result;
		}
		return null;
	} 

	private static Table GetMtm(Table partialTable, IDdlBuilder ddlBuilder, string physicalName, int objectIndex, Relation relation1, Relation relation2)
	{
		// Code size: 218 (0xda)
		// add @ prefix to logical name
		var metaTable = new Meta(0, (byte)EntityType.Table, 0, (int)TableType.Mtm, 0L, partialTable.Name, null, null, true);
		var metaRelation1 = relation1.ToMeta(partialTable.Id);
		var metaRelation2 = relation2.ToMeta(partialTable.Id);
		// add index 
		var flags = 0L;
		var reltArr = new[] { metaRelation1.Name, metaRelation2.Name };
		var value = GetColumnList(reltArr);
		flags = SetIndexUnique(flags, true);
		var metaIndex = new Meta(0, (byte)EntityType.Index, 0, 0, flags, partialTable.Name, null, value, true);
		var metaArr = new[] { metaRelation1, metaRelation2, metaIndex };
		var segMent = new ReadOnlySpan<Meta>(metaArr, 0, 3);
		var result = metaTable.ToTable(segMent, PhysicalType.Table, ddlBuilder, physicalName, objectIndex) ?? partialTable;
		result.Relations[0] = relation1;
		result.Relations[1] = relation2;
		return result;
	}

	#endregion
}
