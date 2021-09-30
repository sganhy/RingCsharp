using Npgsql;
using System;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public class DbDataReaderAdapter: IDbDataReader
    {
        private readonly NpgsqlDataReader _reader;
        private static readonly string Iso8601Format = @"yyyy-MM-ddTHH:mm:ssZ";


        public DbDataReaderAdapter(NpgsqlDataReader reader)
        {
            _reader = reader;
        }



        public void Dispose()
        {
            _reader.Dispose();
        }

        public bool Read() => _reader.Read();
        public void Close() => _reader.Close();
        public string Get(string fieldName) {
            var result= _reader[fieldName];
            if (result is DateTime)
            {
                return ((DateTime) result).ToString(Iso8601Format);
            }
            return result?.ToString();
        }

        public bool HasRows => _reader.HasRows;
    }
}
