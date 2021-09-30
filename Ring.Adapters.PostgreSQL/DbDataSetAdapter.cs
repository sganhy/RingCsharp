using System.Data;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public sealed class DbDataSetAdapter: DataSet, IDbDataSet
    {
        public IDbDataTable FirstTable => new DbDataTableAdapter(Tables);
    }
}
