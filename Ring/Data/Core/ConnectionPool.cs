using System;
using System.Threading;

namespace Ring.Data.Core
{
    internal sealed class ConnectionPool : IDisposable
    {
        private static int _currrentPoolId = Constants.DefaultPoolId;
        private readonly int _lastIndex;     // last connection index 
        private readonly IDbConnection[] _bucket; // only readonly connection for sqlite
        private int _creationCount;
        private int _destroyCount;
        private int _cursor;
        private ushort _requestCount;       // ushort to avoid negative numbers & 
        private int _swapIndex;
        private bool _disposed;

        public readonly IDbConnection ConnectionRef;
        public readonly int PoolId;
        public readonly int MaxSize;
        public readonly int MinSize;

        /// <summary>
        /// Ctor
        /// </summary>
        public ConnectionPool(int minPoolSize, int maxPoolSize, IDbConnection connectionRef)
        {
            ++_currrentPoolId;
            PoolId = _currrentPoolId;
            _disposed = false;
            if (maxPoolSize <= 0) throw new ArgumentOutOfRangeException(Constants.ArgObjectPoolExceptionName, maxPoolSize, Constants.ArgObjectPoolExceptionMsg);
            MaxSize = maxPoolSize < 1 ? 1 : maxPoolSize;
            MinSize = minPoolSize < 1 ? 1 : minPoolSize;
            if (maxPoolSize < minPoolSize) MaxSize = MinSize;
            ConnectionRef = connectionRef;
            _bucket = new IDbConnection[maxPoolSize];
            _cursor = minPoolSize - 1;     // cursor on min last element 
            _creationCount = 0;
            _destroyCount = 0;
            _lastIndex = maxPoolSize - 1;

            // close reference connection
            if (ConnectionRef.State == ConnectionState.Open) ConnectionRef.Close();

            switch (connectionRef.Provider)
            {
                case DatabaseProvider.Sqlite:
                    //TODO manage only one connection to sqlite to write
                    MaxSize = 1; // max connection to update and change database
                    MinSize = 1;
                    break;
                case DatabaseProvider.Oracle:
                    break;
                case DatabaseProvider.PostgreSql:
                    for (var i = 0; i < minPoolSize; ++i) _bucket[i] = CreateConnection();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public int ConnectionCount => _creationCount - _destroyCount;

        /// <summary>
        /// Retrieves an item from the pool. 
        /// </summary>
        /// <returns>The item retrieved from the pool.</returns>
        public IDbConnection Get()
        {
            // no lock here !!!!
            if (_cursor >= 0)
            {
                Monitor.Enter(_bucket); // start lock 
                var result = _bucket[_cursor];
                --_cursor;
                Monitor.Exit(_bucket); // end lock 
                return result;
            }
            return CreateConnection();
        }

        /// <summary>
        /// Places an item in the pool.
        /// </summary>
        public void Put(IDbConnection item)
        {
            Monitor.Enter(_bucket);     // start lock to lock before comparison (_cursor < _lastIndex) 
            if (_cursor < _lastIndex)
            {
                ++_requestCount;
                ++_cursor;
                _swapIndex = _cursor != 0 ? _requestCount % _cursor : 0;
                // swap 
                _bucket[_cursor] = _bucket[_swapIndex];
                _bucket[_swapIndex] = item;
                Monitor.Exit(_bucket); // end lock 
                return;
            }
            Monitor.Exit(_bucket); // end lock 
            DestroyConnection(item);
        }

        /// <summary>
        /// Disposes of items in the pool that implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                var count = ConnectionCount;
                // destroy all 
                for (var i = 0; i < count; ++i) DestroyConnection(Get());
                _disposed = true;
            }
        }

        /// <summary>
        /// TODO implement sanity check 
        /// </summary>
        public void SanityCheck()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        private IDbConnection CreateConnection(bool readonlyConnection = false)
        {
            ++_creationCount;
            var oc = ConnectionRef.CreateNewInstance(Constants.BaseConnectionId + _creationCount, readonlyConnection);
            oc.Open();


            switch (ConnectionRef.Provider)
            {
                case DatabaseProvider.Sqlite:
                    //TODO reimplement !!
                    break;
                case DatabaseProvider.PostgreSql:
                    // define default schema 

                    break;
            }
            return oc;
        }
        private void DestroyConnection(IDbConnection connection)
        {
            try
            {
                if (connection.State != ConnectionState.Closed) connection.Close();
                connection.Close();
                connection.Dispose();
            }
            finally
            {
                ++_destroyCount; // manage counter 
            }
        }

        ~ConnectionPool()
        {
            Dispose();
        }
    }
}
