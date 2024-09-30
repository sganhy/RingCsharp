using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using System.Globalization;
using DbSchema = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema;

internal readonly struct Meta
{
	#region constants

	// entity type constants
	private const byte TableId = (byte)EntityType.Table;
	private const byte SchemaId = (byte)EntityType.Schema;
	private const byte FieldId = (byte)EntityType.Field;
	private const byte IndexId = (byte)EntityType.Index;
	private const byte RelationId = (byte)EntityType.Relation;
	private const byte SequenceId = (byte)EntityType.Sequence;
	private const byte LanguageId = (byte)EntityType.Language;
	private const byte TablespaceId = (byte)EntityType.Tablespace;
	private const byte ParameterId = (byte)EntityType.Parameter;
	private const byte ConstraintId = (byte)EntityType.Constraint;
	private const byte AliasId = (byte)EntityType.Alias;
	private const char IndexColumnDelimiter = ';';

	// relation types constants
	private const byte RelationTypeOtopId = (byte)RelationType.Otop;
	private const byte RelationTypeOtmId = (byte)RelationType.Otm;
	private const byte RelationTypeMtmId = (byte)RelationType.Mtm;
	private const byte RelationTypeMtoId = (byte)RelationType.Mto;
	private const byte RelationTypeOtofId = (byte)RelationType.Otof;

	// field types constants
	private const byte FieldTypeLongId = (byte)FieldType.Long;
	private const byte FieldTypeIntId = (byte)FieldType.Int;
	private const byte FieldTypeShortId = (byte)FieldType.Short;
	private const byte FieldTypeByteId = (byte)FieldType.Byte;
	private const byte FieldTypeFloatId = (byte)FieldType.Float;
	private const byte FieldTypeDoubleId = (byte)FieldType.Double;
	private const byte FieldTypeStringId = (byte)FieldType.String;
	private const byte FieldTypeShortDateTimeId = (byte)FieldType.ShortDateTime;
	private const byte FieldTypeDateTimeId = (byte)FieldType.DateTime;
	private const byte FieldTypeLongDateTimeId = (byte)FieldType.LongDateTime;
    private const byte FieldTypeByteArrayId = (byte)FieldType.ByteArray;
    private const byte FieldTypeBooleanId = (byte)FieldType.Boolean;
    private const byte FieldTypeLongStringId = (byte)FieldType.LongString;

    // flags bit positions
	private const byte BitPositionFieldCaseSensitive = 2;
	private const byte BitPositionFieldNotNull = 3;
	private const byte BitPositionFieldMultilingual = 4;
	private const byte BitPositionIndexBitmap = 9;
	private const byte BitPositionIndexUnique = 10;
	private const byte BitPositionEntityBaseline = 14;
	private const byte BitPositionFirstPositionSize = 17;
	private const byte BitPositionFirstPositionRelType = 18;
	private const byte BitPositionRelationNotNull = 4;
	private const byte BitPositionRelationConstraint = 5;
	private const byte BitPositionTableCached = 9;
	private const byte BitPositionTableReadonly = 10;
	private const byte BitPositionTablespaceIndex = 11;
	private const byte BitPositionTablespaceTable = 12;
	private static readonly string DefaultNumberValue = "0";
	private static readonly string DefaultBoolValue = false.ToString(CultureInfo.InvariantCulture);

	#endregion 

	internal readonly int Id;
	internal readonly byte ObjectType;
	internal readonly int ReferenceId;
	internal readonly int DataType;
	internal readonly long Flags;
	internal readonly string Name;		  // name of entity
	internal readonly string? Description;  // late loading 
	internal readonly string? Value;
	internal readonly bool Active;

	internal Meta(string name) 
		: this(default, default, default, default, default, name, null, default, true) { }
	internal Meta(int id, string name, EntityType entityType) 
		: this(id, (byte)entityType, default, default, default, name, null, default, true) { }
	internal Meta(int id, string name, EntityType entityType, long flags, string? value) 
		: this(id,(byte)entityType,default,default,flags,name,null,value,true) {}
	internal Meta(int id, Meta meta)
		: this(id, meta.ObjectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, meta.Description, meta.Value, meta.Active) { }
	internal Meta(int id, byte objectType, int referenceId, int dataType, long flags, string name, string? description, string? value, bool active)
	{
		Id = id;
		ObjectType = objectType;
		ReferenceId = referenceId;
		DataType = dataType;
		Flags = flags;
		Name = name;
		Description = description;  // late loading 
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

	#region entity methods 
	internal bool IsEntityBaseline() => ReadFlag(BitPositionEntityBaseline);
	internal static long SetEntityBaseline(long flags, bool value) => WriteFlag(flags, BitPositionEntityBaseline, value);
	#endregion

	#region field methods  
	internal FieldType GetFieldType() => ToFieldType((byte)(DataType & 127));
	internal bool IsFieldNotNull() => ReadFlag(BitPositionFieldNotNull);
	internal int GetFieldSize() => (int)((Flags >> BitPositionFirstPositionSize) & (int.MaxValue));
	internal string? GetFieldDefaultValue()
	{
		if (!string.IsNullOrEmpty(Value)) return Value;
		if (IsFieldNotNull())
		{
			var fieldType = GetFieldType();
			switch (fieldType)
			{
				case FieldType.Int:
				case FieldType.Long:
				case FieldType.Byte:
				case FieldType.Short:
				case FieldType.Float:
				case FieldType.Double:
					return DefaultNumberValue;
				case FieldType.Boolean:
					return DefaultBoolValue;
			}
		}
		return null;
	}
	internal bool IsFieldCaseSensitive() => ReadFlag(BitPositionFieldCaseSensitive);
	internal bool IsFieldMultilingual() => ReadFlag(BitPositionFieldMultilingual);
	// data type 
	internal static int SetFieldType(int dataType, FieldType fieldType)
	{
		dataType &= 0x7FFFFF80; // clear 7 first bits
		dataType += (int)fieldType;
		return dataType;
	}
	// field flags 
	internal static long SetFieldNotNull(long flags, bool value) => WriteFlag(flags, BitPositionFieldNotNull, value);
	internal static long SetFieldCaseSensitive(long flags, bool value) => WriteFlag(flags, BitPositionFieldCaseSensitive, value);
	internal static long SetFieldMultilingual(long flags, bool value) => WriteFlag(flags, BitPositionFieldMultilingual, value);
	internal static long SetFieldSize(long flags, int size)
	{
		var temp = (long)size;
		temp <<= BitPositionFirstPositionSize;
		flags += temp;
		return flags;
	}
	#endregion

	#region relation methods  
    internal bool IsRelationNotNull() => ReadFlag(BitPositionRelationNotNull);
    internal bool HasRelationConstraint() => ReadFlag(BitPositionRelationConstraint);
    internal RelationType GetRelationType() => ToRelationType((byte)((Flags >> BitPositionFirstPositionRelType) & 127));

    internal static long SetRelationdNotNull(long flags, bool value) => WriteFlag(flags, BitPositionRelationNotNull, value);
    internal static long SetRelationConstraint(long flags, bool value) => WriteFlag(flags, BitPositionRelationConstraint, value);
    internal static long SetRelationType(long flags, RelationType type)
    {
        var temp = (long)type & 127L;
        // maxInt32 & size << ()
        flags &= 0x7FFFFFFFFF00FFFF;
        temp <<= BitPositionFirstPositionRelType;
        flags += temp;
        return flags;
    }
    #endregion

    #region index methods
    internal bool IsIndexBitmap() => ReadFlag(BitPositionIndexBitmap);
    internal bool IsIndexUnique() => ReadFlag(BitPositionIndexUnique);
    internal string[] GetIndexedColumns() => Value != null ? Value.Split(IndexColumnDelimiter) : Array.Empty<string>();
    // index value 
    internal static string? SetIndexedColumns(string[] columns) => string.Join(IndexColumnDelimiter, columns);
    // index flags 
    internal static long SetIndexUnique(long flags, bool value) => WriteFlag(flags, BitPositionIndexUnique, value);
    internal static long SetIndexBitmap(long flags, bool value) => WriteFlag(flags, BitPositionIndexBitmap, value);
    #endregion

    #region table methods  
    internal static long SetTableReadonly(long flags, bool readonlyValue) => WriteFlag(flags, BitPositionTableReadonly, readonlyValue);
    internal static long SetTableCached(long flags, bool cached) => WriteFlag(flags, BitPositionTableCached, cached);
    internal bool IsTableReadonly() => ReadFlag(BitPositionTableReadonly);
    internal bool IsTableCached() => ReadFlag(BitPositionTableCached);
    #endregion

    #region parameter methods
    internal FieldType GetParameterValueType() => ToFieldType((byte)(DataType & 127));
    internal ParameterType GetParameterType() => ToParameterType(Id);
    internal string GetParameterValue() => Value ?? string.Empty;
    internal static int SetParameterValueType(int dataType, FieldType valueType) => (dataType & 0xFFF8) + ((byte)valueType) & 127;
    #endregion

    #region tablespace methods  
    internal bool IsTablespaceTable() => ReadFlag(BitPositionTablespaceTable);
    internal bool IsTablespaceIndex() => ReadFlag(BitPositionTablespaceIndex);
    #endregion

    internal bool ReadFlag(byte bitPosition) => ((Flags >> (bitPosition - 1)) & 1) > 0;
    internal static long WriteFlag(long flags, byte bitPosition, bool value)
    {
        if (bitPosition < 65)
        {
            var mask = 1L;
            mask <<= bitPosition - 1;
            if (value) flags |= mask;
            else flags &= ~mask;
        }
        return flags;
    }

    internal static DbSchema GetEmptySchema(Meta meta, DatabaseProvider provider) =>
     new(meta.Id, meta.Name, meta.Description, Array.Empty<Parameter>(),
          Array.Empty<Lexicon>(), SchemaLoadType.Full, SchemaType.Undefined, Array.Empty<Sequence>(), Array.Empty<Table>(),
          Array.Empty<Table>(), Array.Empty<TableSpace>(), provider, meta.Active, meta.IsEntityBaseline());

    internal static Table GetEmptyTable(Meta meta, TableType tableType) =>
        new(meta.Id, meta.Name, meta.Description, meta.Value, string.Empty,
             tableType, Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<int>(), Array.Empty<IColumn>(),
             Array.Empty<Index>(), meta.ReferenceId, PhysicalType.Table, meta.IsEntityBaseline(), meta.Active,
             meta.IsTableCached(), meta.IsTableReadonly());

    internal static Relation GetEmptyRelation(Meta meta, RelationType relationType, TableType tableType = TableType.Fake) =>
        new(meta.Id, meta.Name, meta.Description, relationType,
            GetEmptyTable(new Meta(meta.Name), tableType), -1, false, false, true, true);

    internal static Field GetEmptyField(Meta meta, FieldType fieldType) =>
        new(meta.Id, meta.Name, meta.Description, fieldType, 0, null, true, false,
            false, false, true);

    

    internal EntityType GetEntityType()
    {
        switch (ObjectType)
        {
            case TableId: return EntityType.Table;
            case FieldId: return EntityType.Field;
            case RelationId: return EntityType.Relation;
            case IndexId: return EntityType.Index;
            case SchemaId: return EntityType.Schema;
            case SequenceId: return EntityType.Sequence;
            case LanguageId: return EntityType.Language;
            case TablespaceId: return EntityType.Tablespace;
            case ParameterId: return EntityType.Parameter;
            case AliasId: return EntityType.Alias;
            case ConstraintId: return EntityType.Constraint;
            default:
                break;
        }
        return EntityType.Undefined;
    }

    #region convertors 

    internal Relation? ToRelation(Table to)
         => IsRelation ? new Relation(Id, Name, Description, GetRelationType(), to, -1,
               IsRelationNotNull(), HasRelationConstraint(), IsEntityBaseline(), Active) : null;
    
    internal Field? ToField()
        => IsField ? new Field(Id, Name, Description, GetFieldType(), GetFieldSize(), GetFieldDefaultValue(), IsEntityBaseline(),
           IsFieldNotNull(), IsFieldCaseSensitive(), IsFieldMultilingual(), Active) : null;

    internal static DbSchema? ToSchema(Meta[] schema, DatabaseProvider provider,
       SchemaType type = SchemaType.Static, SchemaLoadType loadType = SchemaLoadType.Full)
    {
        // sort ASC by reference_id, name
        Array.Sort(schema, (x, y) => MetaSchemaComparer(x, y));
        var meta = GetSchema(schema);
        if (meta != null)
        {
            var metaValue = meta.Value;
            var ddlBuilder = provider.GetDdlBuilder();
            var parameters = GetParameters(schema);
            var lexicons = new List<Lexicon>();
            var sequences = new List<Sequence>();
            var tableByName = GetTables(schema, ddlBuilder, metaValue, provider);
            var tableById = ShallowCopy(tableByName);
            var tableSpaces = GetTableSpaces(schema);

            // sort arrays - already sorted by name
            Array.Sort(parameters, (x, y) => x.Id.CompareTo(y.Id));
            Array.Sort(tableById, (x, y) => x.Id.CompareTo(y.Id));

            var result = new DbSchema(meta.Value.Id, metaValue.Name, metaValue.Description, parameters,
                lexicons.ToArray(), loadType, type, sequences.ToArray(), tableById.ToArray(), tableByName.ToArray(), tableSpaces.ToArray(),
                provider, metaValue.Active, metaValue.IsEntityBaseline());

            result.LoadRelations(schema);
            result.LoadColumnMappers(); // load column mapper on tables
            result.LoadRecordIndexes(); // load record indexes on relations

            return result;
        }
        return null;
    }
    internal TableSpace? ToTableSpace() => IsTableSpace ? new TableSpace(Id, Name, Description, IsTablespaceIndex(), IsTablespaceTable(),
                false, string.Empty, Value ?? string.Empty, Active, IsEntityBaseline()) : null;
    internal Parameter? ToParameter()
    {
        var parameterType = GetParameterType();
        return IsParameter ? new Parameter(Id, Name, Description, parameterType,
               GetParameterValueType(), GetParameterValue(), parameterType.GetDefaultValue(), ReferenceId,
               IsEntityBaseline(), Active) : null;
    }
    internal Index? ToIndex() => IsIndex ? new Index(Id, Name, Description, GetIndexedColumns(),
               IsIndexUnique(), IsIndexBitmap(), Active, IsEntityBaseline()) : null;

    /// <summary>
    /// Create a instance of table, relation assigned later by schema creation
    /// </summary>
    /// <param name="meta">meta talbe</param>
    /// <param name="tableItems">sorted by name, to improve performance</param>
    /// <param name="tableType">table type is define by schema builder</param>
    /// <param name="physicalName">physical name is define by provider</param>
    internal Table? ToTable(ArraySegment<Meta> tableItems, TableType tableType, PhysicalType physicalType, string physicalName)
    {
        if (IsTable)
        {
            var fields = GetFieldArray(tableItems);
            var relations = GetRelationArray(tableItems);
            var indexes = GetIndexes(tableItems);
            var columnMapperSize = GetColumnMapperSize(tableItems, tableType, fields.Length);

            // sort arrays
            Array.Sort(fields, (x, y) => string.CompareOrdinal(x.Name, y.Name));
            Array.Sort(indexes, (x, y) => string.CompareOrdinal(x.Name, y.Name));

            var result = new Table(Id, Name, Description, Value, physicalName,
                tableType, relations, fields, new int[columnMapperSize], new IColumn[columnMapperSize], indexes, ReferenceId,
                physicalType, IsEntityBaseline(), Active, IsTableCached(), IsTableReadonly());

            return result;
        }
        return null;
    }
    #endregion

    #region private methods 

    private static Index[] GetIndexes(ArraySegment<Meta> items)
    {
        // count element
        var indexCount = 0;
        var span = items.AsSpan();
        foreach (var item in span) if (item.IsIndex) ++indexCount;
        if (indexCount <= 0) return Array.Empty<Index>();
        var result = new Index[indexCount];
        var fieldIndex = 0;
        foreach (var item in span)
        {
            if (item.IsIndex)
            {
                // cannot be null here 
#pragma warning disable CS8601 // Possible null reference assignment.
                result[fieldIndex] = item.ToIndex();
#pragma warning restore CS8601
                ++fieldIndex;
            }
        }
        return result;
    }

    private static TableSpace[] GetTableSpaces(Span<Meta> schema)
    {
        var result = new List<TableSpace>();
#pragma warning disable CS8604
        foreach (var meta in schema) if (meta.IsTableSpace) result.Add(meta.ToTableSpace());
#pragma warning restore CS8604 // Possible null reference argument.
        return result.ToArray();
    }

    private static Parameter[] GetParameters(Span<Meta> schema)
    {
        var result = new List<Parameter>();
#pragma warning disable CS8604
        foreach (var meta in schema) if (meta.IsParameter) result.Add(meta.ToParameter());
#pragma warning restore CS8604 // Possible null reference argument.
        return result.ToArray();
    }


    private static FieldType ToFieldType(byte value)
    {
        // avoid boxing operation - add unit test on all field type enum fields
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (value)
        {
            case FieldTypeLongId: return FieldType.Long;
            case FieldTypeIntId: return FieldType.Int;
            case FieldTypeShortId: return FieldType.Short;
            case FieldTypeByteId: return FieldType.Byte;
            case FieldTypeFloatId: return FieldType.Float;
            case FieldTypeDoubleId: return FieldType.Double;
            case FieldTypeStringId: return FieldType.String;
            case FieldTypeShortDateTimeId: return FieldType.ShortDateTime;
            case FieldTypeDateTimeId: return FieldType.DateTime;
            case FieldTypeLongDateTimeId: return FieldType.LongDateTime;
            case FieldTypeByteArrayId: return FieldType.ByteArray;
            case FieldTypeBooleanId: return FieldType.Boolean;
            case FieldTypeLongStringId: return FieldType.LongString;
        }
#pragma warning restore IDE0066 // Convert switch statement to expression
        return FieldType.Undefined;
    }

    private static RelationType ToRelationType(byte value)
    {
        // avoid boxing operation
        switch (value)
        {
            case RelationTypeOtopId: return RelationType.Otop;
            case RelationTypeOtmId: return RelationType.Otm;
            case RelationTypeMtmId: return RelationType.Mtm;
            case RelationTypeMtoId: return RelationType.Mto;
            case RelationTypeOtofId: return RelationType.Otof;
            default:
                break;
        }
        return RelationType.Undefined;
    }

    private static Field[] GetFieldArray(ArraySegment<Meta> items)
    {
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
        // count element
        var relationCount = 0;
        var span = items.AsSpan();
        foreach (var item in span) if (item.IsRelation) ++relationCount;
        // relation are assigned later
        return relationCount > 0 ? new Relation[relationCount] : Array.Empty<Relation>();
    }

    private static Meta? GetSchema(Span<Meta> schema)
    {
        var i = 0;
        var count = schema.Length;
        while (i < count)
        {
            if (schema[i].IsSchema) return schema[i];
            ++i;
        }
        return null;
    }
   
    private static Table[] GetTables(Meta[] schema, IDdlBuilder ddlBuilder, Meta metaSchema, DatabaseProvider provider)
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
        foreach (var meta in schemaSpan)
        {
            if (meta.IsTable)
            {
                var emptyTable = GetEmptyTable(meta, TableType.Fake);
                var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);
                var segment = dico.ContainsKey(meta.Id) ?
                    new ArraySegment<Meta>(schema, dico[meta.Id].Item1, dico[meta.Id].Item2) :
                    new ArraySegment<Meta>(schema, 0, 0);
                var table = meta.ToTable(segment, TableType.Business, PhysicalType.Table, physicalName);
#pragma warning disable CS8604 // Possible null reference argument.
                result.Add(table);
#pragma warning restore CS8604
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

    private static int GetColumnMapperSize(ArraySegment<Meta> items, TableType tableType, int fieldCount)
    {
        if (tableType == TableType.Mtm) return 2;
        var result = fieldCount;
        var span = items.AsSpan();
        foreach (var item in span)
        {
            if (item.IsRelation)
            {
                var relationType = item.GetRelationType();
                if (relationType == RelationType.Mto || relationType == RelationType.Otop)
                    ++result;
            }
        }
        return result;
    }

    private static ParameterType ToParameterType(int value)
        => Enum.IsDefined(typeof(ParameterType), value) ? (ParameterType)value : ParameterType.Undefined;

    #endregion

}
