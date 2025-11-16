using Ring.Schema.Models;

namespace Ring.Schema;

internal interface IDocumentValidator
{
	Dictionary<string, int> ReferenceTables { get; }
	ValueTask<DocumentStats> GetMetaCountAsync(string filePath, CancellationToken cancellationToken = default);

}
