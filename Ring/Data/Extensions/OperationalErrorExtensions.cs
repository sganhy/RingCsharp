using Ring.Data.Models;

namespace Ring.Data.Extensions;

internal static class OperationalErrorExtensions
{
	internal static void Set(this OperationalError operationalError, in AlterQuery query)
	{
		operationalError.TableName = query.Table.PhysicalName;
	}
}
