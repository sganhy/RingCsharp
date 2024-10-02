using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class IntExtensions
{
    static IntExtensions()
    {
        // avoid The type initializer for 'Ring.Schema.Extensions.IntExtensions' threw an exception.
    }

    // cache of Ring.Schema.Enums.TableType
    private static readonly Dictionary<int, TableType> TableTypeEnumsId = GetTableTypeId();
    // cache of Ring.Schema.Enums.ParameterType
    private static readonly Dictionary<int, ParameterType> ParameterTypeEnumsId = GetParameterTypeId();
    // cache of Ring.Schema.Enums.FieldType
    private static readonly Dictionary<int, FieldType> FieldTypeEnumsId = GetFieldTypeId();
    // cache of Ring.Schema.Enums.FieldType
    private static readonly Dictionary<int, EntityType> EntityTypeEnumsId = GetEntityTypeId();
    
    /// <summary>
    /// Casting from int to TableType
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TableType ToTableType(this int dataType) => 
        TableTypeEnumsId.ContainsKey(dataType) ? TableTypeEnumsId[dataType] : TableType.Undefined;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static FieldType ToFieldType(this int dataType) =>
        FieldTypeEnumsId.ContainsKey(dataType) ? FieldTypeEnumsId[dataType] : FieldType.Undefined;

    internal static ParameterType ToParameterType(this int id) =>
        ParameterTypeEnumsId.ContainsKey(id)? ParameterTypeEnumsId[id] : ParameterType.Undefined;

    internal static EntityType ToEntityType(this int objectType) =>
        EntityTypeEnumsId.ContainsKey(objectType) ? EntityTypeEnumsId[objectType] : EntityType.Undefined;

    #region private methods

    private static Dictionary<int, TableType> GetTableTypeId()
    {
        var tableTypes = Enum.GetValues<TableType>();
        var result = new Dictionary<int, TableType>(tableTypes.Length * 4); // multiply by four bucket size to reduce collisions
        for (var i=0; i<tableTypes.Length; ++i)
        {
            var tableType = tableTypes[i];
            var tableTypeId = (int)tableType;
            if (!result.ContainsKey(tableTypeId)) result.Add(tableTypeId, tableType);
        }
        return result;
    }

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

    private static Dictionary<int, FieldType> GetFieldTypeId()
    {
        var fieldTypes = Enum.GetValues<FieldType>();
        var result = new Dictionary<int, FieldType>(fieldTypes.Length * 4); // multiply by four bucket size to reduce collisions
        for (var i = 0; i < fieldTypes.Length; ++i)
        {
            var fieldType = fieldTypes[i];
            var fieldTypeId = (int)fieldType;
            if (!result.ContainsKey(fieldTypeId)) result.Add(fieldTypeId, fieldType);
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
