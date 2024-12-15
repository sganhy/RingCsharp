namespace Ring.Data.Enums;

internal enum SaveQueryType : byte
{
	DeleteRecord = 0,
	InsertRecord = 1,
	UpdateRecord = 2,
	UpdateReturningRecord = 3,

	// above 100, reserved for cancel operations
	FirstCancelOperation = 101,
	CancelledDeleteRecord = 105,
	CancelledInsertRecord = 106,
	CancelledUpdateRecord = 107,

	// 126 reserved for unit tests
	Undefined = 127
}
