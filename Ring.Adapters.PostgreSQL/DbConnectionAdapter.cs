using System;
using Npgsql;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public class DbConnectionAdapter : IDbConnection
	{
	    private readonly IConfiguration _configuration;
	    private readonly NpgsqlConnection _connection;
		private readonly DateTime _creationTime;
		private readonly bool _readonly;
		private int _id;

		public DbConnectionAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = new NpgsqlConnection { ConnectionString = configuration.ConnectionString};
            _readonly = false;
	        _creationTime = DateTime.Now;
	        _id = 0;
        }
        private DbConnectionAdapter(int id, bool connectionReadonly, IConfiguration configuration)
        {
            // no readOnly connection on PostGre
            _configuration = configuration;
            _connection = new NpgsqlConnection { ConnectionString = configuration.ConnectionString };
            _readonly = connectionReadonly;
	        _creationTime = DateTime.Now;
	        _id = id;
		}

		public string ConnectionString => _connection?.ConnectionString;
		public DateTime CreationTime => _creationTime;
		public int Id => _id;

		public DatabaseProvider Provider => DatabaseProvider.PostgreSql;

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

        public NpgsqlConnection Connection => _connection;

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

	    public IDbConnection CreateNewInstance(int id, bool readonlyConnection) => 
			new DbConnectionAdapter(id, readonlyConnection, _configuration);

		public bool IsReadOnly => _readonly;

	    public IDbConnection CreateNewInstance(bool readonlyConnection, DatabaseProvider provider, string connectionString)
	    {
	        switch (provider)
	        {
                case DatabaseProvider.PostgreSql: 
                    return new DbConnectionAdapter(0, false, new Configuration(DatabaseProvider.PostgreSql.ToString(), connectionString));
	        }
	        return null;
	    }
	}
}
