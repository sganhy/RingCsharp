using System;

namespace Ring.Data
{
    public interface IDbCommand : IDisposable
    {
        string CommandText { get; set; }
        int CommandTimeout { get; set; }
        IDbConnection Connection { get; set; }
        IDbTransaction Transaction { get; set; }
        int ExecuteNonQuery();
        object ExecuteScalar();
        IDbCommand CreateNewInstance();
        void SetParameterValue(int index, string value);
        void AddParameter(IDbParameter parameter);
        IDbDataReader ExecuteReader();
        void AddParameters(IDbParameter[] parameter);
    }
}
