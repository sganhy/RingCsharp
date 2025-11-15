using Ring.Schema.Models;

namespace Ring.Schema;

internal interface IDocumentValidator
{
	ValueTask<DocumentStats> GetMetaCountAsync(string filePath, CancellationToken cancellationToken = default);

}
