namespace Ring.Data.Enums;

internal enum SaveQueryType : byte
{
	DeleteRecord = 0,
	InsertRecord = 1,
	UpdateRecord = 2,
	UpdateReturningRecord = 3,

	CancelledDeleteRecord = 65,
	CancelledInsertRecord = 66,
	CancelledUpdateRecord = 67,

	// 126 reserved for unit tests
	Undefined = 127
}
