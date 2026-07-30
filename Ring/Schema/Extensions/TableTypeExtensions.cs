using Ring.Schema.Enums;
using Ring.Util;

namespace Ring.Schema.Extensions;

internal static class TableTypeExtensions
{
    internal static readonly char SystemTablePrefix = '@'; // logical name prefix
	internal static readonly string MtmTablePrefix = "@mtm_"; // logical name prefix for mtm table

    internal static string GetLogicalName(this TableType tableType, string? partialName=null)
    {
        // Convert switch statement to expression
        // Normalize strings to uppercase 
#pragma warning disable CA1308
        switch (tableType)
        {
            case TableType.Mtm: 
                return MtmTablePrefix + partialName;
            case TableType.Meta:
            case TableType.Log:
            case TableType.Test:
				return SystemTablePrefix + tableType.ToString().ToLowerInvariant();
            case TableType.MetaId:
			case TableType.TableCatalog:
			case TableType.TablespaceCatalog:
			case TableType.SchemaCatalog:
				return SystemTablePrefix + NamingConvention.ToSnakeCase(tableType.ToString())?.ToLowerInvariant();
        }
#pragma warning restore CA1308
        return string.Empty;
    }

	internal static bool IsCatalog(this TableType tableType, string? partialName = null) 
        => tableType == TableType.TableCatalog || tableType == TableType.TablespaceCatalog || tableType == TableType.SchemaCatalog;

}
