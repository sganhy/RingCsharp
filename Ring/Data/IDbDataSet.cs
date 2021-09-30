using System;

namespace Ring.Data
{
    public interface IDbDataSet : IDisposable
    {
        IDbDataTable FirstTable { get; }
    }
}
