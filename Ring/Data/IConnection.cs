using Ring.Data.Models;
using System.Text;

namespace Ring.Data;

public interface IConnection : IDisposable
{
	int ProviderId();
	void BeginTransaction();
	void Commit();
	void Rollback();
	bool IsConnectionAlive();
	long Id { get; }
	DateTime CreationTime { get; }
	DateTime? LastConnectionTime { get; }
	ConnectionState State { get; }
	Encoding ClientEncoding { get; }
	void Open();
	Task OpenAsync(CancellationToken cancellationToken);
	void Close();
	Task CloseAsync(CancellationToken cancellationToken);
	IConnection CreateInstance(int id, int sqlSendBufferSize);
	string?[] Execute(in RetrieveQuery query, ReadOnlySpan<byte> sql);
	OperationalError? Execute(in AlterQuery query, ReadOnlySpan<byte> sql); // AlterQuery: No defensive-copy penalty — the JIT knows no member access can mutate it.
	ValueTask<OperationalError?> ExecuteAsync(AlterQuery query, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default);
	OperationalError? Execute(in SaveQuery query, ReadOnlySpan<byte> sql);
}
