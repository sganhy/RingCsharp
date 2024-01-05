using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Data.Extensions;
internal static class OperationTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetSqlOperator(this OperatorType GetStringOperation, DatabaseProvider provider)
    {
        string result;
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (GetStringOperation)
        {
            case OperatorType.Equal: result = "="; break;
            case OperatorType.NotEqual: result = provider == DatabaseProvider.PostgreSql?"<>":"!="; break;
            case OperatorType.Greater: result = ">"; break;
            case OperatorType.GreaterOrEqual: result = ">="; break;
            case OperatorType.Less: result = "<"; break;
            case OperatorType.LessOrEqual: result = "<="; break;
            case OperatorType.Like: result = " like "; break;
            case OperatorType.NotLike: result = " NOT like "; break;
            case OperatorType.In: result = " IN "; break;
            default: throw new NotSupportedException();
        }
#pragma warning restore IDE0066
        return result;
    }
}
