using Ring.Schema.Enums;

namespace Ring.Data.Extensions;

internal static class OperatorExtensions
{
    internal static string ToSql(this Operator GetStringOperation, DatabaseProvider provider, string? value)
    {
        // Convert switch statement to expression
        // Ternary operators should not be nested
#pragma warning disable IDE0066, S3358 
        switch (GetStringOperation)
        {
            case Operator.Equal: return string.IsNullOrEmpty(value) ? " IS " : "=";
            case Operator.NotEqual: return string.IsNullOrEmpty(value)
                    ? " IS NOT " : provider == DatabaseProvider.PostgreSql ? "<>" : "!=";
            case Operator.Greater: return ">"; 
            case Operator.GreaterOrEqual: return ">="; 
            case Operator.Less: return "<";
            case Operator.LessOrEqual: return "<=";
            case Operator.Like: return " like ";
            case Operator.NotLike: return " NOT like "; 
            case Operator.In: return " IN ";
            default: throw new NotSupportedException();
        }
#pragma warning restore S3358 , IDE0066
    }
}
