using Npgsql;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public class DbTransactionAdapter: IDbTransaction
    {
        private readonly NpgsqlTransaction _transaction;

        public DbTransactionAdapter(NpgsqlTransaction transaction)
        {
            _transaction = transaction;
        }

        public NpgsqlTransaction Transaction => _transaction;

        public void Dispose()
        {
            _transaction?.Dispose();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
        }

        public void Commit()
        {
            _transaction.Commit();
        }
    }
}
