using System.Data.SQLite;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public class DbDataReaderAdapter: IDbDataReader
    {
        private SQLiteDataReader _reader;

        public DbDataReaderAdapter(SQLiteDataReader reader)
        {
            _reader = reader;
        }
        public void Dispose()
        {
            _reader.Dispose();
        }
        public bool Read() => _reader.Read();
        public void Close() => _reader.Close();
        public string Get(string fieldName) => _reader[fieldName].ToString();
        public bool HasRows => _reader.HasRows;

    }
}
