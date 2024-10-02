using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class IntExtensions
{
    #region constants

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
    
    #endregion 

    static IntExtensions()
    {
        // avoid The type initializer for 'Ring.Schema.Extensions.IntExtensions' threw an exception.
    }

    // cache of Ring.Schema.Enums.ParameterType
    private static readonly Dictionary<int, ParameterType> ParameterTypeEnumsId = GetParameterTypeId();
    // cache of Ring.Schema.Enums.FieldType
    private static readonly Dictionary<int, EntityType> EntityTypeEnumsId = GetEntityTypeId();

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
        }
        return FieldType.Undefined;
    }

    internal static ParameterType ToParameterType(this int id) =>
        ParameterTypeEnumsId.ContainsKey(id)? ParameterTypeEnumsId[id] : ParameterType.Undefined;

    /// <summary>
    /// Low performance !
    /// </summary>
    internal static EntityType ToEntityType(this int objectType) =>
        EntityTypeEnumsId.ContainsKey(objectType) ? EntityTypeEnumsId[objectType] : EntityType.Undefined;

    #region private methods

    private static Dictionary<int, ParameterType> GetParameterTypeId()
    {
        var parameterTypes = Enum.GetValues<ParameterType>();
        var result = new Dictionary<int, ParameterType>(parameterTypes.Length * 2); // multiply by four bucket size to reduce collisions
        for (var i = 0; i < parameterTypes.Length; ++i)
        {
            var parameterType = parameterTypes[i];
            var parameterTypeId = (int)parameterType;
            if (!result.ContainsKey(parameterTypeId)) result.Add(parameterTypeId, parameterType);
        }
        return result;
    }

    private static Dictionary<int, EntityType> GetEntityTypeId()
    {
        var entityTypes = Enum.GetValues<EntityType>();
        var result = new Dictionary<int, EntityType>(entityTypes.Length * 2); // multiply by four bucket size to reduce collisions
        for (var i = 0; i < entityTypes.Length; ++i)
        {
            var entityType = entityTypes[i];
            var entityTypeId = (int)entityType;
            if (!result.ContainsKey(entityTypeId)) result.Add(entityTypeId, entityType);
        }
        return result;
    }

    #endregion 
}
