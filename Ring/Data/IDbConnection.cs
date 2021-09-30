using System;

namespace Ring.Data
{
    public interface IDbConnection : IDisposable
    {


        string ConnectionString { get; }
        void Open();
        void Close();
        bool IsReadOnly { get; }
        DateTime CreationTime { get; }
        int Id { get; }
        ConnectionState State { get; }
        DatabaseProvider Provider { get; }
        IDbTransaction BeginTransaction();

        /// <summary>
        /// Add new instance of connection
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateNewInstance(int id, bool readonlyConnection);

        /// <summary>
        /// 
        /// </summary>
        IDbConnection CreateNewInstance(bool readonlyConnection, DatabaseProvider provider, string connectionString);

        /// <summary>
        /// Add new sqlCommand instance
        /// </summary>
        IDbCommand CreateNewCommandInstance();

        /// <summary>
        /// Add an instance of adapter 
        /// </summary>
        IDbAdapter CreateNewAdapterInstance(IDbCommand command);

        /// <summary>
        /// Add a dataset 
        /// </summary>
        IDbDataSet CreateNewDataSetInstance();

        /// <summary>
        /// Add an in stance of Parameter
        /// </summary>
        IDbParameter CreateNewParameterInstance(DbType type, string name, string value);

    }
}
