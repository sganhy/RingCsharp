using System.Data;

namespace Ring.Data;

public interface IRingConnection: IDisposable
{
    int Id { get; }
    string ConnectionString { get; }
    DateTime CreationTime { get; }
    DateTime? LastConnectionTime { get; }
    ConnectionState State { get; }
    void Open();
    Task OpenAsync(CancellationToken cancellationToken);
    void Close();
    Task CloseAsync(CancellationToken cancellationToken);
    IRingConnection CreateNewInstance();
    Span<string?> ExecuteSelect(string sql, int columnCount,Span<(string, byte)> parameters);
}
