using System.Data.Common;
using System.Data.SQLite;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public class DbCommandLineAdapter : IDbCommand
    {
        private readonly SQLiteCommand _sqlCommand;
        private IDbConnection _connection;


        public DbCommandLineAdapter()
        {
            _sqlCommand = new SQLiteCommand {CommandType = System.Data.CommandType.Text};
        }

        public void Dispose()
        {
	        _sqlCommand.Connection = null; // remove reference on DbConnection 
			_sqlCommand.Dispose();
        }

        public SQLiteCommand Command => _sqlCommand;

        public string CommandText
        {
            get { return _sqlCommand.CommandText; }
            set { _sqlCommand.CommandText = value; } 
        }
        public string CommandType { get; set; }

        public int CommandTimeout
        {
            get { return _sqlCommand.CommandTimeout; } 
            set { _sqlCommand.CommandTimeout = value; } 
        }

        public IDbConnection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = value;
                var adapter = value as DbConnectionAdapter;
                _sqlCommand.Connection = adapter?.Connection;
            } 
        }
	    public object ExecuteScalar() => _sqlCommand.ExecuteScalar();
		public int ExecuteNonQuery() => _sqlCommand.ExecuteNonQuery();

        public IDbCommand CreateNewInstance()
        {
            return new DbCommandLineAdapter();
        }

        public void AddParameter(IDbParameter parameter)
        {
            var dbParameterAdapter = parameter as DbParameterAdapter;
            if (dbParameterAdapter != null)
                _sqlCommand.Parameters.Add(dbParameterAdapter.Parameter);
        }

        public void AddParameters(IDbParameter[] parameter)
        {
            _sqlCommand.Parameters.AddRange(ToParameters(parameter));
        }

        private DbParameter [] ToParameters(IDbParameter[] parameter)
        {
            if (parameter == null) return null;
            var result = new DbParameter[parameter.Length];
            for (var i = 0; i< parameter.Length; ++i)
            {
                var dbParameterAdapter = parameter[i] as DbParameterAdapter;
                if (dbParameterAdapter != null)
                    result[i] = dbParameterAdapter.Parameter;
            }
            return result;
        }
        public void SetParameterValue(int index, string value)
        {
	        var param = _sqlCommand?.Parameters[index];
	        if (param != null) param.Value = value;
        }
        public IDbTransaction Transaction
        {
            get { return _sqlCommand.Transaction !=null ? new DbTransactionAdapter(_sqlCommand.Transaction) : null; }
            set { _sqlCommand.Transaction = ((DbTransactionAdapter)value).Transaction; }
        }

        public IDbDataReader ExecuteReader()
        {
            return new DbDataReaderAdapter(_sqlCommand.ExecuteReader());
        }
    }
}
