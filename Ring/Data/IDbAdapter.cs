using System;

namespace Ring.Data
{
    public interface IDbAdapter : IDisposable
    {
        void Fill(IDbDataSet dataset);
    }
}
