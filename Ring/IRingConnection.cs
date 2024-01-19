namespace Ring;

public interface IRingConnection
{
    string ConnectionString { get; }
    DateTime CreationTime { get; }
    DateTime? LastConnection { get; }
    void Close();
    IRingConnection Create();
    string?[][]? ExecuteSelect(string sql, int columnCount, Span<string> parameterValues, Span<byte> parameterTypes);
    ValueTask<string?[][]?> ExecuteSelectAsync(string sql, int columnCount, Span<string> parameterValues, Span<byte> parameterTypes);
}
