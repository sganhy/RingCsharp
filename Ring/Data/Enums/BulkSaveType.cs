namespace Ring.Data.Enums
{
    internal enum BulkSaveType : byte
    {
        DeleteRecord = 0,
        InsertRecord = 1,
        UpdateRecord = 2,
        RelateRecords = 3,
        BindRelation = 4,
        InsertMtm = 5,
        InsertMtmIfNotExist = 6,
        Unknown = 11
    }
}
