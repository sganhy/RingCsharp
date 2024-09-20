using System.Data;

namespace Ring.Data;

public interface IRingConnection: IDisposable
{
    string ConnectionString { get; }
    DateTime CreationTime { get; }
    DateTime? LastConnectionTime { get; }
    ConnectionState State { get; }
    void Close();
    IRingConnection Create();
    Span<string?> ExecuteSelect(string sql, int columnCount, Span<string> parameterValues, Span<byte> parameterTypes);
}
