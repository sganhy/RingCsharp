using System;
using System.Globalization;
using Npgsql;
using NpgsqlTypes;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public sealed class DbParameterAdapter: IDbParameter
    {
        private readonly NpgsqlParameter _parameter;
	    //private static readonly string TrueString = "1";
        private static readonly string TimeZoneInfo = "Z";
        private static readonly object TrueObj = true;
        private static readonly object FalseObj = false;
        private static readonly object NullString = DBNull.Value;

        //TODO create a pool of parameters
        public DbParameterAdapter(DbType type, string name, string value)
        {
            switch (type)
            {
                case DbType.Bool:
                    _parameter = string.Equals(value,bool.TrueString, StringComparison.OrdinalIgnoreCase)
                        ? new NpgsqlParameter(name, GetDataType(type)) {Value = TrueObj}
                        : new NpgsqlParameter(name, GetDataType(type)) {Value = FalseObj};
                    break;
	            case DbType.String:
		            _parameter = !string.IsNullOrEmpty(value) 
						? new NpgsqlParameter(name, GetDataType(type)) {Value = value} 
						: new NpgsqlParameter(name, System.Data.DbType.String) {Value = DBNull.Value};
		            break;
	            case DbType.Int64:
		            _parameter = value!=null
			            ? new NpgsqlParameter(name, GetDataType(type)) { Value = value }
			            : new NpgsqlParameter(name, System.Data.DbType.Int64) { Value = DBNull.Value };
					break;
	            case DbType.DateTime:
                    _parameter = value != null
                        ? new NpgsqlParameter(name, GetDataType(type)) { Value = RemoveTimeZone(value) }
                        : new NpgsqlParameter(name, GetDataType(type)) { Value = NullString };
                    break;
	            case DbType.Double:
		            double result;
		            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture , out result);
					_parameter = value != null
			            ? new NpgsqlParameter(name, GetDataType(type)) { Value = result }
			            : new NpgsqlParameter(name, GetDataType(type)) { Value = NullString };
		            _parameter.NpgsqlDbType = NpgsqlDbType.Double;
					break;
				default:
                    _parameter = new NpgsqlParameter(name, GetDataType(type)) {Value = value};
                    break;
            }
            _parameter.IsNullable = true;
        }

        public NpgsqlParameter Parameter => _parameter;

        private static string RemoveTimeZone(string value) => 
            value.EndsWith(TimeZoneInfo, StringComparison.OrdinalIgnoreCase) 
            ? value.Remove(value.Length - 1) : value;

        private static System.Data.DbType GetDataType(DbType type)
        {
            switch (type)
            {
                case DbType.Double:
                    return System.Data.DbType.Double;
                case DbType.Int64:
                    return System.Data.DbType.Int64;
	            case DbType.Bool:
		            return System.Data.DbType.Boolean;
	            case DbType.DateTime:
		            return System.Data.DbType.DateTime;
				case DbType.String:
                case DbType.Clob:
                    return System.Data.DbType.String;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

    }
}
