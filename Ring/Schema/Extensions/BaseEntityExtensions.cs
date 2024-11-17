using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
    private static readonly char HashCodeSeparator = '/';

    internal static long GetHashCode(this BaseEntity baseEntity)
    {
        var result = new StringBuilder();
        result.Append(baseEntity.Active);
        result.Append(HashCodeSeparator);
        result.Append(baseEntity.Baseline);
        result.Append(HashCodeSeparator);
        result.Append(baseEntity.Description);
        result.Append(HashCodeSeparator);
        result.Append(baseEntity.Id);
        result.Append(HashCodeSeparator);
        result.Append(baseEntity.Name);
        HashHelper.Djb2X(result.ToString(), out long hash);
        return hash;
    }
}
