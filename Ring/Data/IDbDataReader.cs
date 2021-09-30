using System;

namespace Ring.Data
{
    public interface IDbDataReader : IDisposable
    {
        bool Read();
        void Close();
        string Get(string fieldName);
        bool HasRows { get; }
    }
}
