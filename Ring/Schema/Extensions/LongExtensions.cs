using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

/// <summary>
/// Casting from Meta.Flags full or partial(Int64) to specific Enum
/// </summary>
internal static class LongExtensions
{
    // cache of Ring.Schema.Enums.RelationType
    private static readonly Dictionary<long, RelationType> RelationTypeEnumsId = GetRelationTypeId();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RelationType ToRelationType(this long flags) =>
        RelationTypeEnumsId.ContainsKey(flags) ? RelationTypeEnumsId[flags] : RelationType.Undefined;

    #region private methods

    private static Dictionary<long, RelationType> GetRelationTypeId()
    {
        var tableTypes = Enum.GetValues<RelationType>();
        var result = new Dictionary<long, RelationType>(tableTypes.Length * 4); // multiply by four bucket size to reduce collisions
        for (var i = 0; i < tableTypes.Length; ++i)
        {
            var tableType = tableTypes[i];
            var tableTypeId = (long)tableType;
            if (!result.ContainsKey(tableTypeId)) result.Add(tableTypeId, tableType);
        }
        return result;
    }

    #endregion

}
