namespace Ring.Data;

public interface IBulkSave : IEquatable<BulkSave>
{
    void CancelRecord(Record? recordToCancel);
}
