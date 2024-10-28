using Ring.Data.Enums;
using Ring.Schema.Enums;

namespace Ring.Data.Extensions;

internal static class AlterQueryTypeExtensions
{
    internal static ConstraintType ToConstraintType(this AlterQueryType queryType)
    {
        switch (queryType)
        {
            case AlterQueryType.CreatePrimaryKey: return ConstraintType.PrimaryKey;
        }
        return ConstraintType.Undefined;
    }

}
