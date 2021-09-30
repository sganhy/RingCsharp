using System.Data;
using System.Data.SQLite;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public sealed class DbAdapter: IDbAdapter
    {
        private readonly SQLiteDataAdapter _adapter; 

        public DbAdapter()
        {
            _adapter = new SQLiteDataAdapter();
            
        }

        public DbAdapter(DbCommandLineAdapter command)
        {
            _adapter = new SQLiteDataAdapter(command.Command);
        }

        public void Dispose()
        {
            _adapter.Dispose();
        }

        public void Fill(IDbDataSet dataset)
        {
            var data = dataset as DataSet;
            if (data != null) _adapter.Fill(data);
        }
    }
}
