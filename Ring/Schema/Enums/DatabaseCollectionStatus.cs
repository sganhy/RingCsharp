namespace Ring.Schema.Enums;

internal enum DatabaseCollectionStatus : byte
{
    NotReady = 0,
    Ready = 1,
    Loading = 2,
    Upgrading = 6
}
