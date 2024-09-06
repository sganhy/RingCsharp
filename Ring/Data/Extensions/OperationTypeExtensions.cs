using Ring.Schema.Enums;

namespace Ring.Data.Extensions;

internal static class OperationTypeExtensions
{
    internal static string ToSql(this OperatorType GetStringOperation, DatabaseProvider provider, string? value)
    {
        // Convert switch statement to expression
        // Ternary operators should not be nested
#pragma warning disable IDE0066, S3358 
        switch (GetStringOperation)
        {
            case OperatorType.Equal: return string.IsNullOrEmpty(value) ? " IS " : "=";
            case OperatorType.NotEqual: return string.IsNullOrEmpty(value)
                    ? " IS NOT " : provider == DatabaseProvider.PostgreSql ? "<>" : "!=";
            case OperatorType.Greater: return ">"; 
            case OperatorType.GreaterOrEqual: return ">="; 
            case OperatorType.Less: return "<";
            case OperatorType.LessOrEqual: return "<=";
            case OperatorType.Like: return " like ";
            case OperatorType.NotLike: return " NOT like "; 
            case OperatorType.In: return " IN ";
            default: throw new NotSupportedException();
        }
#pragma warning restore S3358 , IDE0066
    }
}
