using Ring.Data.Models;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data;

public struct BulkRetrieve
{
    private readonly List<RetrieveQuery> _queries;
    private readonly Database? _schema;

    public BulkRetrieve()
    {
        _queries = new List<RetrieveQuery>();
        _schema = null;
    }


}
