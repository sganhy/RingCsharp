using System.Data;
using Npgsql;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public sealed class DbAdapter: IDbAdapter
    {
        private readonly NpgsqlDataAdapter _adapter; 

        public DbAdapter()
        {
            _adapter = new NpgsqlDataAdapter();
            
        }

        public DbAdapter(DbCommandLineAdapter command)
        {
            _adapter = new NpgsqlDataAdapter(command.Command);
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
