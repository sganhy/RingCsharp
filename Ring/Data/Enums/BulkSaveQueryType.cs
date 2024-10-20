namespace Ring.Data.Enums;

internal enum BulkSaveQueryType : byte
{
	DeleteRecord = 0,
	InsertRecord = 1,
	UpdateRecord = 2,
	RelateRecords = 3,
	BindRelation = 4,
	InsertMtm = 5,
	InsertMtmIfNotExist = 6,
	// 126 reserved for unit tests
	Undefined = 127
}
