using Ring.Schema.Enums;
using Ring.Util;

namespace Ring.Schema.Extensions;

internal static class TableTypeExtensions
{
    internal static readonly char SystemTablePrefix = '@'; // logical name prefix

    internal static string GetLogicalName(this TableType tableType, string? partialName=null)
    {
        // Convert switch statement to expression
        // Normalize strings to uppercase 
#pragma warning disable CA1308
        switch (tableType)
        {
            case TableType.Mtm: 
                return SystemTablePrefix + partialName;
            case TableType.Meta:
            case TableType.Log:
                return SystemTablePrefix + tableType.ToString().ToLowerInvariant();
            case TableType.MetaId: 
                return SystemTablePrefix + NamingConvention.ToSnakeCase(tableType.ToString());
        }
#pragma warning restore CA1308
        return string.Empty;
    }
}
