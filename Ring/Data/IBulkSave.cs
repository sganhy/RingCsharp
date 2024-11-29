namespace Ring.Data;

public interface IBulkSave : IEquatable<BulkSave>
{
	void CancelRecord(Record? recordToCancel);
	void Clear();
	int CountByType(string? objectType);
	void DeleteRecord(Record record);
	void DeleteRecordById(string recordType, long id);
	Record? GetRecordByIndex(int index, string objectType);
	void InsertRecord(Record record);
	void UpdateRecord(Record record);
	void Save();
}
