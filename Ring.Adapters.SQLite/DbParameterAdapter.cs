using System;
using System.Data.SQLite;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public sealed class DbParameterAdapter: IDbParameter
    {
        private readonly SQLiteParameter _parameter;

        public DbParameterAdapter(DbType type, string name, object value)
        {
            _parameter = new SQLiteParameter(name, GetDataType(type)) {Value = value};
            _parameter.IsNullable = true;
        }

        public SQLiteParameter Parameter => _parameter;

        private static System.Data.DbType GetDataType(DbType type)
        {
            switch (type)
            {
                case DbType.Double:
                    return System.Data.DbType.Double;
                case DbType.Int64:
                case DbType.Bool:
					return System.Data.DbType.Int64;
                case DbType.DateTime:
                case DbType.String:
                case DbType.Clob:
                    return System.Data.DbType.String;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

    }
}
