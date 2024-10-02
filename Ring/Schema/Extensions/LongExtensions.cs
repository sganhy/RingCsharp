using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

/// <summary>
/// Casting from Meta.Flags full or partial(Int64) to specific Enum
/// </summary>
internal static class LongExtensions
{
    // relation types constants
    private const long RelationTypeOtopId = (long)RelationType.Otop;
    private const long RelationTypeOtmId = (long)RelationType.Otm;
    private const long RelationTypeMtmId = (long)RelationType.Mtm;
    private const long RelationTypeMtoId = (long)RelationType.Mto;
    private const long RelationTypeOtofId = (long)RelationType.Otof;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RelationType ToRelationType(this long flags) 
    {
        // avoid boxing operation
        switch (flags)
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

}
