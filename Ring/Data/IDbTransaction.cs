using System;

namespace Ring.Data
{
    public interface IDbTransaction : IDisposable
    {
        void Rollback();

        void Commit();
    }
}
