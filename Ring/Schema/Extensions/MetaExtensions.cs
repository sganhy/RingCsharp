using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using Index = Ring.Schema.Models.Index;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Util.Builders;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class MetaExtensions
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsTable(this Meta meta) => meta.ObjectType == TableId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSchema(this Meta meta) => meta.ObjectType == SchemaId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsField(this Meta meta) => meta.ObjectType == FieldId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsIndex(this Meta meta) => meta.ObjectType == IndexId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsRelation(this Meta meta) => meta.ObjectType == RelationId;
    internal static bool IsSequence(this Meta meta) => meta.ObjectType == SequenceId;
    internal static bool IsTableSpace(this Meta meta) => meta.ObjectType == TablespaceId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsParameter(this Meta meta) => meta.ObjectType == ParameterId;

    #region field methods  
    internal static string? GetFieldDefaultValue(this Meta meta) {
        if (!string.IsNullOrEmpty(meta.Value)) return meta.Value;
        if (meta.IsFieldNotNull()) 
        {
            var fieldType = meta.GetFieldType();
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
    internal static int GetFieldSize(this Meta meta) => (int)((meta.Flags >> BitPositionFirstPositionSize) & (int.MaxValue));
    internal static void SetFieldSize(this Meta meta, int size) {
        var temp = (long)size;
        temp <<= BitPositionFirstPositionSize;
        meta.Flags += temp;
    }
    internal static FieldType GetFieldType(this Meta meta) => ToFieldType((byte)(meta.DataType & 127));
    internal static void SetFieldType(this Meta meta, FieldType fieldType) {
        meta.DataType &= 0x7FFFFF80; // clear 7 first bits
        meta.DataType += (int)fieldType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFieldNotNull(this Meta meta) => meta.ReadFlag(BitPositionFieldNotNull);
    internal static void SetFieldNotNull(this Meta meta, bool value) => meta.WriteFlag(BitPositionFieldNotNull, value);
    internal static bool IsFieldCaseSensitive(this Meta meta) => meta.ReadFlag(BitPositionFieldCaseSensitive);
    internal static void SetFieldCaseSensitive(this Meta meta, bool value) => meta.WriteFlag(BitPositionFieldCaseSensitive, value);
    internal static bool IsFieldMultilingual(this Meta meta) => meta.ReadFlag(BitPositionFieldMultilingual);
    internal static void SetFieldMultilingual(this Meta meta, bool value) => meta.WriteFlag(BitPositionFieldMultilingual, value);
    internal static Field? ToField(this Meta meta)
        => meta.IsField() ? new Field(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), 
           meta.GetFieldType(), meta.GetFieldSize(), meta.GetFieldDefaultValue(), meta.IsEntityBaseline(), 
           meta.IsFieldNotNull(), meta.IsFieldCaseSensitive(), meta.IsFieldMultilingual(), meta.IsEntityActive()) : null;

    #endregion

    #region index methods  
    internal static void SetIndexBitmap(this Meta meta, bool bitmap) => meta.WriteFlag(BitPositionIndexBitmap, bitmap);
    internal static void SetIndexUnique(this Meta meta, bool unique) => meta.WriteFlag(BitPositionIndexUnique, unique);
    internal static bool IsIndexUnique(this Meta meta) => meta.ReadFlag(BitPositionIndexUnique);
    internal static bool IsIndexBitmap(this Meta meta) => meta.ReadFlag(BitPositionIndexBitmap);
    internal static Index? ToIndex(this Meta meta)
        => meta.IsIndex() ?
           new Index(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), meta.GetIndexedColumns(),
               meta.IsIndexUnique(), meta.IsIndexBitmap(), meta.IsEntityActive(),
               meta.IsEntityBaseline()) : null;
    internal static string[] GetIndexedColumns(this Meta meta) =>
        meta.Value != null ? meta.Value.Split(IndexColumnDelimiter) : Array.Empty<string>();

    internal static void SetIndexedColumns(this Meta meta, string[] columns) =>
        meta.Value = string.Join(IndexColumnDelimiter, columns);

    #endregion

    #region relation methods  
    internal static void SetInverseRelation(this Meta meta, string? relationName) => meta.Value=relationName;
    internal static string? GetInverseRelation(this Meta meta) => meta.Value;
    internal static bool HasRelationConstraint(this Meta meta) => meta.ReadFlag(BitPositionRelationConstraint);
    internal static void SetRelationConstraint(this Meta meta, bool notNull) => meta.WriteFlag(BitPositionRelationConstraint, notNull);
    internal static bool IsRelationNotNull(this Meta meta) => meta.ReadFlag(BitPositionRelationNotNull);
    internal static void SetRelationdNotNull(this Meta meta, bool notNull) => meta.WriteFlag(BitPositionRelationNotNull, notNull);
    internal static RelationType GetRelationType(this Meta meta) =>
        ToRelationType((byte)((meta.Flags >> BitPositionFirstPositionRelType) & 127));
    internal static void SetRelationType(this Meta meta, RelationType type)
    {
        var temp = (long)type & 127L;
        // maxInt32 & size << ()
        meta.Flags &= 0x7FFFFFFFFF00FFFF;
        temp <<= BitPositionFirstPositionRelType;
        meta.Flags += temp;
    }
    internal static void SetRelationToTable(this Meta meta, int toTableId) => meta.DataType = toTableId;

    internal static Relation? ToRelation(this Meta meta, Table to)
         => meta.IsRelation() ?
           new Relation(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), meta.GetRelationType(), to,
               meta.IsRelationNotNull(), meta.HasRelationConstraint(), meta.IsEntityBaseline(), meta.IsEntityActive()) : null;

    #endregion

    #region entity methods 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? GetEntityDescription(this Meta meta) => meta.Description;
    internal static void SetEntityDescription(this Meta meta, string? description) => meta.Description = description;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetEntityName(this Meta meta, string name) => meta.Name = name;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetEntityName(this Meta meta) => meta.Name ?? string.Empty;

    internal static void SetEntityRefId(this Meta meta, int refId) => meta.ReferenceId = refId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetEntityRefId(this Meta meta) => meta.ReferenceId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetEntityId(this Meta meta) => meta.Id;
    internal static int SetEntityId(this Meta meta, int id) => meta.Id = id;
    internal static bool IsEntityActive(this Meta meta) => meta.Active;
    internal static void SetEntityActive(this Meta meta, bool active) => meta.Active = active;
    internal static EntityType GetEntityType(this Meta meta)
    {
        switch (meta.ObjectType)
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
    internal static void SetEntityType(this Meta meta, EntityType entityType) => meta.ObjectType = (byte)entityType;
    internal static void SetEntityType(this Meta meta, byte entityType) => meta.ObjectType = entityType;
    internal static bool IsEntityBaseline(this Meta meta) => meta.ReadFlag(BitPositionEntityBaseline);
    internal static void SetEntityBaseline(this Meta meta, bool value) => meta.WriteFlag(BitPositionEntityBaseline, value);

    #endregion

    #region table methods  

    internal static void SetTableReadonly(this Meta meta, bool readonlyValue) => meta.WriteFlag(BitPositionTableReadonly, readonlyValue);
    internal static void SetTableCached(this Meta meta, bool cached) => meta.WriteFlag(BitPositionTableCached, cached);
    internal static bool IsTableReadonly(this Meta meta) => meta.ReadFlag(BitPositionTableReadonly);
    internal static bool IsTableCached(this Meta meta) => meta.ReadFlag(BitPositionTableCached);

    /// <summary>
    /// Create a instance of table, relation assigned later by schema creation
    /// </summary>
    /// <param name="meta">meta talbe</param>
    /// <param name="items">sorted by name, to improve performance</param>
    /// <param name="tableType">table type is define by schema builder</param>
    /// <param name="physicalName">physical name is define by provider</param>
    internal static Table? ToTable(this Meta meta, ArraySegment<Meta> tableItems, TableType tableType, PhysicalType physicalType, string physicalName)
    {
        if (meta.IsTable())
        {
            var fields = tableItems.GetFieldArray();
            var relations = tableItems.GetRelationArray();
            var indexes = tableItems.GetIndexArray();
            var columnMapperSize = tableItems.GetColumnMapperSize(tableType, fields.Length);

            // sort arrays
            Array.Sort(fields, (x, y) => string.CompareOrdinal(x.Name,y.Name));
            Array.Sort(indexes, (x, y) => string.CompareOrdinal(x.Name, y.Name));

            var result = new Table(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), meta.Value, physicalName,
                tableType, relations, fields, new int[columnMapperSize], indexes, meta.ReferenceId,
                physicalType, meta.IsEntityBaseline(), meta.IsEntityActive(), meta.IsTableCached(), meta.IsTableReadonly());

            return result;
        }
        return null;
    }

    #endregion

    #region schema methods

    internal static DbSchema? ToSchema(this Meta[] schema, DatabaseProvider provider, 
        SchemaSourceType source = SchemaSourceType.NativeDataBase, SchemaLoadType loadType = SchemaLoadType.Full)
    {
        // sort ASC by reference_id, name
        Array.Sort(schema, (x, y) => MetaSchemaComparer(x,y)); 
        var meta = schema.GetSchema();
        if (meta != null)
        {
            var ddlBuilder = provider.GetDdlBuilder();
            var parameters = schema.GetParameterArray();
            var lexicons = new List<Lexicon>();
            var sequences = new List<Sequence>();
            var tableByName = schema.GetTableArray(ddlBuilder, meta, provider);
            var tableById = (Table[])tableByName.Clone(); 
            var tableSpaces = schema.GetTableSpaceArray();

            // sort arrays - already sorted by name
            Array.Sort(parameters, (x, y) => x.Id.CompareTo(y.Id));
            Array.Sort(tableById, (x, y) => x.Id.CompareTo(y.Id));

            var result = new DbSchema(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), parameters,
                lexicons.ToArray(), loadType, source, sequences.ToArray(), tableById.ToArray(), tableByName.ToArray(), tableSpaces.ToArray(),
                provider, meta.IsEntityActive(), meta.IsEntityBaseline());

            result.LoadRelations(schema);
            result.LoadColumnMappers();

            return result;
        }
        return null;
    }

    #endregion 

    #region tablespace methods  

    internal static void SetTablespaceIndex(this Meta meta, bool tableSpaceIndex) => meta.WriteFlag(BitPositionTablespaceIndex, tableSpaceIndex);
    internal static void SetTablespaceTable(this Meta meta, bool tableSpaceTable) => meta.WriteFlag(BitPositionTablespaceTable, tableSpaceTable);
    internal static bool IsTablespaceTable(this Meta meta) => meta.ReadFlag(BitPositionTablespaceTable);
    internal static bool IsTablespaceIndex(this Meta meta) => meta.ReadFlag(BitPositionTablespaceIndex);
    internal static TableSpace? ToTableSpace(this Meta meta)
        => meta.IsTableSpace() ?
           new TableSpace(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), meta.IsTablespaceIndex(), meta.IsTablespaceTable(),
                false, string.Empty, meta.Value ?? string.Empty, meta.IsEntityActive(), meta.IsEntityBaseline()) : null;

    #endregion

    #region parameter methods  

    internal static FieldType GetParameterValueType(this Meta meta) => ToFieldType((byte)(meta.DataType & 127));
    internal static ParameterType GetParameterType(this Meta meta) => ToParameterType(meta.GetEntityId());
    internal static void SetParameterType(this Meta meta, ParameterType parameterType) => meta.SetEntityId((int)parameterType);
    internal static string GetParameterValue(this Meta meta) => meta.Value ?? string.Empty;
    internal static string SetParameterValue(this Meta meta, string? value) => meta.Value = value;
    internal static void SetParameterValueType(this Meta meta, FieldType valueType) => meta.DataType = (meta.DataType&0xFFF8) + ((byte)valueType)&127;
    internal static Parameter? ToParameter(this Meta meta)
    {
        var parameterType = meta.GetParameterType();
        return meta.IsParameter() ?
           new Parameter(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), parameterType,
               meta.GetParameterValueType(), meta.GetParameterValue(), parameterType.GetDefaultValue(), meta.ReferenceId,
               meta.IsEntityBaseline(), meta.IsEntityActive()) : null;
    }
    #endregion

    #region sequence methods  


    #endregion

    // set as internal to unit test it 
    internal static bool ReadFlag(this Meta meta, byte bitPosition) => ((meta.Flags >> (bitPosition - 1)) & 1) > 0;

    // set as internal to unit test it 
    internal static void WriteFlag(this Meta meta, byte bitPosition, bool value) {
        if (bitPosition < 65) {
            var mask = 1L;
            mask <<= bitPosition - 1;
            if (value) meta.Flags |= mask;
            else meta.Flags &= ~mask;
        }
    }

    internal static DbSchema GetEmptySchema(Meta meta, DatabaseProvider provider) =>
        new(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), Array.Empty<Parameter>(),
             Array.Empty<Lexicon>(), SchemaLoadType.Full, SchemaSourceType.UnDefined, Array.Empty<Sequence>(), Array.Empty<Table>(),
             Array.Empty<Table>(), Array.Empty<TableSpace>(), provider, meta.IsEntityActive(), meta.IsEntityBaseline());

    internal static Table GetEmptyTable(Meta meta, TableType tableType) =>
        new(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), meta.Value, string.Empty,
             tableType, Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<int>(), 
             Array.Empty<Index>(), meta.ReferenceId, PhysicalType.Table, meta.IsEntityBaseline(), meta.IsEntityActive(), 
             meta.IsTableCached(), meta.IsTableReadonly());

    internal static Relation GetEmptyRelation(Meta meta, RelationType relationType, TableType tableType=TableType.Fake) =>
        new(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), relationType, 
            GetEmptyTable(new Meta(meta.GetEntityName()), tableType), false, false, true, true);

    internal static Field GetEmptyField(Meta meta, FieldType fieldType) =>
        new(meta.GetEntityId(), meta.GetEntityName(), meta.GetEntityDescription(), fieldType, 0, null,true,false,
            false,false,true);

    #region private methods 

    private static ParameterType ToParameterType(int value)
        => Enum.IsDefined(typeof(ParameterType), value) ? (ParameterType)value : ParameterType.Undefined;

    private static FieldType ToFieldType(byte value)
    {
        // avoid boxing operation - add unit test on all field type enum fields
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
            default:
                break;
        }
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

    private static Field[] GetFieldArray(this ArraySegment<Meta> items)
    {
        // count element
        int i = 0, count = items.Count, fieldCount = 0;
        var primaryKey = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Int);
        for (; i<count; ++i) {
            if (items[i].IsField()) {
                ++fieldCount;
                if (string.Equals(primaryKey?.Name, items[i].Name, StringComparison.OrdinalIgnoreCase)) 
                    primaryKey = primaryKey.GetDefaultPrimaryKey(items[i].GetFieldType());
            }
        }
        var result = new Field[fieldCount]; // allow once
        var fieldIndex = 0;
        for (i=0; i<count; ++i)
        {
            if (items[i].IsField())
            {
                result[fieldIndex] = string.Equals(primaryKey?.Name, items[i].Name, StringComparison.OrdinalIgnoreCase) ?
                    primaryKey ?? default! : items[i].ToField() ?? default!; 
                ++fieldIndex;
            }
        }
        return result;
    }

    private static Relation[] GetRelationArray(this ArraySegment<Meta> items)
    {
        // count element
        int count = items.Count, relationCount = 0;
        for (var i = 0; i < count; ++i) if (items[i].IsRelation()) ++relationCount;
        // relation are assigned later
        return relationCount > 0 ? new Relation[relationCount] : Array.Empty<Relation>();
    }

    private static Meta? GetSchema(this Meta [] schema)
    {
        var i=0;
        var count=schema.Length;
        while (i<count)
        {
            if (schema[i].IsSchema()) return schema[i];
            ++i;
        }
        return null;
    }

    private static Table[] GetTableArray(this Meta[] schema, IDdlBuilder ddlBuilder, Meta metaSchema, 
        DatabaseProvider provider)
    {
        int startIndex, count;
        var i=0;
        var metaCount = schema.Length;
        var tableCount = metaCount > 250 ? metaCount / 10 : 30;
        var dico = new Dictionary<int, (int, int)>(tableCount); // table_id, start index , count
        var emptySchema = GetEmptySchema(metaSchema, provider);
        // build dico
        while (i<metaCount)
        {
            var meta = schema[i];
            if (meta.IsField() || meta.IsRelation() || meta.IsIndex())
            {
                if (!dico.ContainsKey(meta.ReferenceId)) dico.Add(meta.ReferenceId, (i,0));
                (startIndex, count) = dico[meta.ReferenceId];
                dico[meta.ReferenceId] = (startIndex, count+1);
            }
            ++i;
        }
        var result = new List<Table>(dico.Count);
        for (i=0; i<metaCount; ++i)
        {
            // Meta meta, ArraySegment<Meta> tableItems, TableType tableType, string physicalName
            if (schema[i].IsTable())
            {
                var metaTable = schema[i];
                var emptyTable = GetEmptyTable(metaTable, TableType.Fake);
                var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);
                var segment = dico.ContainsKey(metaTable.Id) ?
                    new ArraySegment<Meta>(schema, dico[metaTable.Id].Item1, dico[metaTable.Id].Item2) :
                    new ArraySegment<Meta>(schema, 0, 0);
                var table = schema[i].ToTable(segment, TableType.Business, PhysicalType.Table, physicalName);
                if (table!=null) result.Add(table);
            }
        }
        return result.ToArray();
    }

    private static TableSpace[] GetTableSpaceArray(this Meta[] schema)
    {
        var result = new List<TableSpace>();
        for (var i = 0; i < schema.Length; ++i)
        {
            if (schema[i].IsTableSpace())
            {
                var param = schema[i].ToTableSpace();
                if (param != null) result.Add(param);
            }
        }
        return result.ToArray();
    }

    private static Parameter[] GetParameterArray(this Meta[] schema)
    {
        var result = new List<Parameter>();
        for (var i=0; i<schema.Length; ++i)
        {
            if (schema[i].IsParameter()) {
                var param = schema[i].ToParameter();
                if (param!= null) result.Add(param);
            }
        }
        return result.ToArray();
    }

    private static Index[] GetIndexArray(this ArraySegment<Meta> items)
    {
        // count element
        int j= 0, count = items.Count, indexCount = 0;
        for (; j <count; ++j) if (items[j].IsIndex()) ++indexCount;
        if (indexCount > 0)
        {
            var result = new Index[indexCount];
            var fieldIndex = 0;
            for (j = 0; j < count; ++j)
            {
                if (items[j].IsIndex())
                {
                    result[fieldIndex] = items[j].ToIndex() ?? default!;
                    ++fieldIndex;
                }
            }
            return result;
        }
        return Array.Empty<Index>();
    }

    private static int GetColumnMapperSize(this ArraySegment<Meta> items, TableType tableType, int fieldCount)
    {
        if (tableType == TableType.Mtm) return 2;
        int count=items.Count, result=fieldCount;
        for (var i=0; i<count; ++i)
        {
            if (items[i].IsRelation()) {
                var relationType = GetRelationType(items[i]);
                if (relationType == RelationType.Mto || relationType == RelationType.Otop) 
                    ++result;
            }
        }
        return result;
    }

    private static int MetaSchemaComparer(Meta meta1, Meta meta2)
    {
        // sort ASC by reference_id, name
        var result = meta1.ReferenceId.CompareTo(meta2.ReferenceId);
        if (result != 0) return result;
        return string.CompareOrdinal(meta1.Name, meta2.Name);
    }
        
    #endregion
}
