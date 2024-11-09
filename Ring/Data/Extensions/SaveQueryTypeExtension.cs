using Ring.Data.Enums;

namespace Ring.Data.Extensions;

internal static class SaveQueryTypeExtension
{
	public static SaveQueryType CancelOperation(this SaveQueryType operation) 
	{
#pragma warning disable IDE0066 // Convert switch statement to expression
		switch (operation)
		{
			case SaveQueryType.DeleteRecord: return SaveQueryType.CancelledDeleteRecord;
			case SaveQueryType.InsertRecord: return SaveQueryType.CancelledInsertRecord;
			case SaveQueryType.UpdateRecord: return SaveQueryType.CancelledUpdateRecord;
		}
#pragma warning restore IDE0066
		return SaveQueryType.Undefined;
	}

	public static bool IsCancelled(this SaveQueryType operation)
		=> operation == SaveQueryType.CancelledDeleteRecord ||
			operation == SaveQueryType.CancelledInsertRecord ||
			operation == SaveQueryType.CancelledUpdateRecord;
			
}