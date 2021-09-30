using System.Data;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public sealed class DbDataSetAdapter: DataSet, IDbDataSet
    {
        public IDbDataTable FirstTable => new DbDataTableAdapter(Tables);
    }
}
