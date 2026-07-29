using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Util.Builders;

namespace Ring.Data.Extensions;

internal static class OperationalErrorExtensions
{
	internal static void Set(this OperationalError operationalError, in AlterQuery query, IDdlBuilder ddlBuilder)
	{
		operationalError.TableName = ddlBuilder.GetPhysicalName(EntityType.Table, query.Table.Name);
	}
}
