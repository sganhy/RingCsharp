using Ring.Data.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Data.Extensions;

internal static class SaveQueryTypeExtensions
{
	public static SaveQueryType CancelOperation(this SaveQueryType operation) 
	{
		switch (operation)
		{
			case SaveQueryType.DeleteRecord: return SaveQueryType.CancelledDeleteRecord;
			case SaveQueryType.InsertRecord: return SaveQueryType.CancelledInsertRecord;
			case SaveQueryType.UpdateRecord: return SaveQueryType.CancelledUpdateRecord;
		}
		return SaveQueryType.Undefined;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCancelled(this SaveQueryType operation)
		=> operation == SaveQueryType.CancelledDeleteRecord ||
			operation == SaveQueryType.CancelledInsertRecord ||
			operation == SaveQueryType.CancelledUpdateRecord;

}