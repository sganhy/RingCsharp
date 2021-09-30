namespace Ring.Data.Enums
{
    internal enum BulkQueryType : byte
    {
        Undefined = 0,
        SimpleQuery = 1,
        SetRoot = 2,
        TraverseFromParent = 3,
        TraverseFromRoot = 4
    }
}
