using System;
using System.Data.SQLite;
using System.Text;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public class DbConnectionAdapter : IDbConnection
	{
	    private readonly IConfiguration _configuration;
	    private readonly SQLiteConnection _connection;
        private readonly bool _readonly;
		private readonly DateTime _creationTime;
		private const char ConnectionStringSeperator = ';'; 
        private static readonly string ReadonlyCommand= @"Read Only";
		private int _id;

        private DbConnectionAdapter(bool connectionReadonly, IConfiguration configuration)
        {
            _configuration = configuration;
            if (connectionReadonly)
            {
                var newConnectionString = new StringBuilder();
                var arrConnectionStringClause = configuration.ConnectionString.Split(ConnectionStringSeperator);
                foreach (var cmd in arrConnectionStringClause)
                {
                    if (string.IsNullOrWhiteSpace(cmd)) continue;
                    if (cmd.IndexOf(ReadonlyCommand, StringComparison.OrdinalIgnoreCase)>=0) continue;
                    newConnectionString.Append(cmd);
                    newConnectionString.Append(ConnectionStringSeperator);
                }
                newConnectionString.Append(ReadonlyCommand);
                newConnectionString.Append(" = True;");
                _connection = new SQLiteConnection { ConnectionString = newConnectionString.ToString() };
            }
            else _connection = new SQLiteConnection { ConnectionString = configuration.ConnectionString };
            _readonly = connectionReadonly;
	        _creationTime = DateTime.Now;
	        _id = 0;
		}

		public string ConnectionString => _connection?.ConnectionString;
		public DateTime CreationTime => _creationTime;
		public int Id => _id;
		public DatabaseProvider Provider => DatabaseProvider.Sqlite;
		public ConnectionState State
		{
			get
			{
				switch (_connection.State)
				{
					case System.Data.ConnectionState.Open: return ConnectionState.Open;
					case System.Data.ConnectionState.Broken: return ConnectionState.Broken;
					case System.Data.ConnectionState.Closed: return ConnectionState.Closed;
					case System.Data.ConnectionState.Executing: return ConnectionState.Executing;
				}
				return ConnectionState.NotDefined;
			}
		}

		public void Close()
		{
            _connection.Close();
		}

        public IDbCommand CreateNewCommandInstance()
        {
            return new DbCommandLineAdapter();
        }

	    public IDbAdapter CreateNewAdapterInstance(IDbCommand command)
	    {
            return new DbAdapter(command as DbCommandLineAdapter);
	    }

        public IDbDataSet CreateNewDataSetInstance()
        {
            return new DbDataSetAdapter();
        }

        public SQLiteConnection Connection => _connection;

	    public void Dispose()
		{
            _connection.Dispose();
		}

		public void Open()
		{
            _connection.Open();
		}

	    public IDbParameter CreateNewParameterInstance(DbType type, string name, string value)
	    {
	        return new DbParameterAdapter(type, name, value);
	    }

	    public IDbTransaction BeginTransaction()
	    {
	        return new DbTransactionAdapter(_connection.BeginTransaction());
	    }

	    public IDbConnection CreateNewInstance(int id, bool readonlyConnection)
	    {
		    _id = id;
            return new DbConnectionAdapter(readonlyConnection, _configuration);
        }

        public bool IsReadOnly => _readonly;

	    public IDbConnection CreateNewInstance(bool readonlyConnection, DatabaseProvider provider, string connectionString)
	    {
	        switch (provider)
	        {
                case DatabaseProvider.Sqlite:
                    return new DbConnectionAdapter(readonlyConnection, 
                        new Configuration(DatabaseProvider.Sqlite.ToString(), connectionString));
            }
	        return null;
	    }
	}
}
