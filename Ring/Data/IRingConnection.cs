using Ring.Data.Models;
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
    string?[] Execute(in RetrieveQuery query);
    int Execute(in AlterQuery query);
    int Execute(in SaveQuery query);
    
}
