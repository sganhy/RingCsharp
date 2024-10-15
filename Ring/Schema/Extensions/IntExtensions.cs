using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class IntExtensions
{
    #region constants

    // entity type constants
    private const int TableId = (int)EntityType.Table;
    private const int FieldId = (int)EntityType.Field;
    private const int RelationId = (int)EntityType.Relation;
    private const int IndexId = (int)EntityType.Index;
    private const int SchemaId = (int)EntityType.Schema;
    private const int SequenceId = (int)EntityType.Sequence;
    private const int LanguageId = (int)EntityType.Language;
    private const int TablespaceId = (int)EntityType.Tablespace;
    private const int ParameterId = (int)EntityType.Parameter;
    private const int AliasId = (int)EntityType.Alias;
    private const int ConstraintId = (int)EntityType.Constraint;

    // field types constants
    private const int FieldTypeLongId = (int)FieldType.Long;
    private const int FieldTypeIntId = (int)FieldType.Int;
    private const int FieldTypeShortId = (int)FieldType.Short;
    private const int FieldTypeByteId = (int)FieldType.Byte;
    private const int FieldTypeFloatId = (int)FieldType.Float;
    private const int FieldTypeDoubleId = (int)FieldType.Double;
    private const int FieldTypeStringId = (int)FieldType.String;
    private const int FieldTypeShortDateTimeId = (int)FieldType.ShortDateTime;
    private const int FieldTypeDateTimeId = (int)FieldType.DateTime;
    private const int FieldTypeLongDateTimeId = (int)FieldType.LongDateTime;
    private const int FieldTypeByteArrayId = (int)FieldType.ByteArray;
    private const int FieldTypeBooleanId = (int)FieldType.Boolean;
    private const int FieldTypeLongStringId = (int)FieldType.LongString;

    // table types constants
    private const int TableTypeBusinessId = (int)TableType.Business;
    private const int TableTypeBusinessLogId = (int)TableType.BusinessLog;
    private const int TableTypeMetaId = (int)TableType.Meta;
    private const int TableTypeMetaIdId = (int)TableType.MetaId;
    private const int TableTypeFakeId = (int)TableType.Fake;
    private const int TableTypeMtmId = (int)TableType.Mtm;
    private const int TableTypeLogId = (int)TableType.Log;
    private const int TableTypeLexiconId = (int)TableType.Lexicon;
    private const int TableTypeLexiconItemId = (int)TableType.LexiconItem;
    private const int TableTypeSchemaCatalogId = (int)TableType.SchemaCatalog;
    private const int TableTypeTableCatalogId = (int)TableType.TableCatalog;
    private const int TableTypeTableSpaceCatalogId = (int)TableType.TableSpaceCatalog;
    private const int TableTypeLogicalId = (int)TableType.Logical;

    // relation types constants
    private const int RelationTypeOtopId = (int)RelationType.Otop;
    private const int RelationTypeOtmId = (int)RelationType.Otm;
    private const int RelationTypeMtmId = (int)RelationType.Mtm;
    private const int RelationTypeMtoId = (int)RelationType.Mto;
    private const int RelationTypeOtofId = (int)RelationType.Otof;

    #endregion 

    static IntExtensions()
    {
        // avoid The type initializer for 'Ring.Schema.Extensions.IntExtensions' threw an exception.
    }

    // cache of Ring.Schema.Enums.ParameterType
    private static readonly Dictionary<int, ParameterType> ParameterTypeEnumsId = GetParameterTypeId();

    /// <summary>
    /// Casting from int to TableType
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TableType ToTableType(this int dataType)
    {
        switch (dataType)
        {
            case TableTypeBusinessId: return TableType.Business;
            case TableTypeBusinessLogId: return TableType.BusinessLog;
            case TableTypeMetaId: return TableType.Meta;
            case TableTypeMetaIdId: return TableType.MetaId;
            case TableTypeFakeId: return TableType.Fake;
            case TableTypeMtmId: return TableType.Mtm;
            case TableTypeLogId: return TableType.Log;
            case TableTypeLexiconId: return TableType.Lexicon;
            case TableTypeLexiconItemId: return TableType.LexiconItem;
            case TableTypeSchemaCatalogId: return TableType.SchemaCatalog;
            case TableTypeTableCatalogId: return TableType.TableCatalog;
            case TableTypeTableSpaceCatalogId: return TableType.TableSpaceCatalog;
            case TableTypeLogicalId: return TableType.Logical;
        }
        return TableType.Undefined;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FieldType ToFieldType(this int dataType)
    {
        // high performance ! 
        // avoid boxing operation - add unit test on all field type enum fields
        switch (dataType)
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
            default: break;
        }
        return FieldType.Undefined;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RelationType ToRelationType(this int flags)
    {
        // avoid boxing operation
        switch (flags)
        {
            case RelationTypeOtopId: return RelationType.Otop;
            case RelationTypeOtmId: return RelationType.Otm;
            case RelationTypeMtmId: return RelationType.Mtm;
            case RelationTypeMtoId: return RelationType.Mto;
            case RelationTypeOtofId: return RelationType.Otof;
            default: break;
        }
        return RelationType.Undefined;
    }

    /// <summary>
    /// Low performance !
    /// </summary>
    internal static ParameterType ToParameterType(this int id) =>
        ParameterTypeEnumsId.ContainsKey(id) ? ParameterTypeEnumsId[id] : ParameterType.Undefined;

    internal static EntityType ToEntityType(this int entityType) {
        // avoid boxing operation
        switch (entityType)
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
            default: break;
        }
        return EntityType.Undefined;
    }

    #region private methods

    private static Dictionary<int, ParameterType> GetParameterTypeId()
    {
        var parameterTypes = Enum.GetValues<ParameterType>();
        var result = new Dictionary<int, ParameterType>(parameterTypes.Length * 2); // multiply by two, the bucket size to reduce collisions
        for (var i = 0; i < parameterTypes.Length; ++i)
        {
            var parameterType = parameterTypes[i];
            var parameterTypeId = (int)parameterType;
            if (!result.ContainsKey(parameterTypeId)) result.Add(parameterTypeId, parameterType);
        }
        return result;
    }

    #endregion 
}
