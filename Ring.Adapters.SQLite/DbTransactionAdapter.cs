using System.Data.SQLite;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public class DbTransactionAdapter: IDbTransaction
    {
        private readonly SQLiteTransaction _transaction;

        public DbTransactionAdapter(SQLiteTransaction transaction)
        {
            _transaction = transaction;
        }
        public SQLiteTransaction Transaction => _transaction;
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
