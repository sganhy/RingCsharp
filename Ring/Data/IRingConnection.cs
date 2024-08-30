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
    
    string?[][]? ExecuteSelect(string sql, int columnCount, Span<string> parameterValues, Span<byte> parameterTypes);
    ValueTask<string?[][]?> ExecuteSelectAsync(string sql, int columnCount, Span<string> parameterValues, Span<byte> parameterTypes);
}
