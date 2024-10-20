namespace Ring.Data.Enums;

internal enum BulkRetrieveQueryType : byte
{
	SimpleQuery = 1,
	SetRoot = 2,
	TraverseFromParent = 3,
	TraverseFromRoot = 4,
	// 126 reserved for unit tests
	Undefined = 127
}
