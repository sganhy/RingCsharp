using Ring.Data.Models;

namespace Ring.Data;

public interface IConnection : IDisposable
{
	int ProviderId();
	void BeginTransaction();
	void Commit();
	void Rollback();
	long Id { get; }
	string ConnectionString { get; }
	DateTime CreationTime { get; }
	DateTime? LastConnectionTime { get; }
	ConnectionState State { get; }
	void Open();
	Task OpenAsync(CancellationToken cancellationToken);
	void Close();
	Task CloseAsync(CancellationToken cancellationToken);
	IConnection CreateInstance(int id);
	string?[] Execute(in RetrieveQuery query);
	long Execute(in AlterQuery query);
	long Execute(in SaveQuery query);
	ValueTask<int> ExecuteAsync(in AlterQuery query, CancellationToken cancellationToken = default);
}
