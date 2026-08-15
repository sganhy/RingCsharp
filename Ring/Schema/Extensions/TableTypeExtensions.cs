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

	internal static string? GetDescription(this TableType tableType)
	{
		// Code size: 69 (0x45)
		switch (tableType)
		{
			case TableType.MetaId: return ResourceHelper.GetMessage(ResourceType.MetaIdTableDescription);
			case TableType.Meta: return ResourceHelper.GetMessage(ResourceType.MetaTableDescription);
			case TableType.Log: return ResourceHelper.GetMessage(ResourceType.MetaAuditTableDescription);
		}
		return null;
	}

	internal static PhysicalType ToPhysicalType(this TableType tableType) =>
		tableType.IsCatalog() ? PhysicalType.View : PhysicalType.Table;

	internal static bool IsCatalog(this TableType tableType) 
        => tableType == TableType.TableCatalog || tableType == TableType.TablespaceCatalog || tableType == TableType.SchemaCatalog;

}
