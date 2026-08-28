using Ring.Schema.Enums;
using Ring.Util;
using Ring.Util.Enums;
using Ring.Util.Helpers;

namespace Ring.Schema.Extensions;

internal static class TableTypeExtensions
{
    internal static readonly char SystemTablePrefix = '@'; // logical name prefix
	internal static readonly string MtmTablePrefix = "@mtm_"; // logical name prefix for mtm table

    internal static string GetLogicalName(this TableType tableType, string? partialName=null)
    {
		// Code size: 188 (0xbc)
		switch (tableType)
        {
			case TableType.Mtm: return MtmTablePrefix + partialName;
            case TableType.Meta:
            case TableType.Log:
#pragma warning disable CA1308
			case TableType.Test: return SystemTablePrefix + tableType.ToString().ToLowerInvariant();
#pragma warning restore CA1308
			case TableType.MetaId:
			case TableType.TableCatalog:
			case TableType.TablespaceCatalog:
			case TableType.SchemaCatalog: return SystemTablePrefix + NamingConvention.ToSnakeCase(tableType.ToString());
            case TableType.NonBusinessTable: return SystemTablePrefix.ToString();
		}
        return string.Empty;
    }

	internal static int GetObjectIndex(this TableType tableType)
	{
		switch (tableType)
		{
			case TableType.Meta: return 0;
			case TableType.MetaId: return 1;
			case TableType.Log: return 2;
			case TableType.Test: return 3;
			case TableType.TableCatalog: return 4;
			case TableType.TablespaceCatalog: return 5;
			case TableType.SchemaCatalog: return 6;
			default: return -1;
		}
	}

	internal static PhysicalType ToPhysicalType(this TableType tableType) =>
		tableType.IsCatalog() ? PhysicalType.View : PhysicalType.Table;

	internal static bool IsCatalog(this TableType tableType) 
        => tableType == TableType.TableCatalog || tableType == TableType.TablespaceCatalog || tableType == TableType.SchemaCatalog;

}
