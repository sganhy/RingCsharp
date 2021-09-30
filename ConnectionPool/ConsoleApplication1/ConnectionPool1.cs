using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	public class ConnectionPool1
	{
		private readonly int _minPoolSize;
		private readonly int _maxPoolSize;
		private readonly int _lastIndex;     // last connection index 
		private readonly Element[] _bucket;
		private int _creationCount;
		private int _destroyCount;
		private int _cursor;
		private ushort _requestCount;       // ushort to avoid negative numbers & 
		private int _swapIndex;

		/// <summary>
		/// Ctor
		/// </summary>
		public ConnectionPool1(int minPoolSize, int maxPoolSize)
		{
			_minPoolSize = minPoolSize;
			_maxPoolSize = maxPoolSize;
			_bucket = new Element[maxPoolSize];
			_cursor = 0;
			_creationCount = 0;
			_destroyCount = 0;
			_cursor = minPoolSize - 1;     // cursor on last element 
			for (var i = 0; i < minPoolSize; ++i) _bucket[i] = CreateConnection();
			_lastIndex = maxPoolSize - 1;
			_requestCount = 0;
		}

		public int MaxPoolSize => _maxPoolSize;
		public int MinPoolSize => _minPoolSize;
		public int ConnectionCount => _creationCount - _destroyCount;

		/// <summary>
		/// Return connection to pool 
		/// </summary>
		public void Put(Element item)
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
		/// Get connection from pool 
		/// </summary>
		public Element Get()
		{
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


		public void ControlPool()
		{
			#region  destroy - unessary connections
			if (ConnectionCount > MinPoolSize && _minPoolSize <= _cursor)
			{
				Task.Factory.StartNew(() =>
				{
					var maxIteration = MaxPoolSize;
					while (_minPoolSize <= _cursor && maxIteration>0)
					{
						Thread.Sleep(5000);
						DestroyConnection(Get());
						--maxIteration;
					}
				}, TaskCreationOptions.LongRunning);
			}
			#endregion
			//TODO connection life cycle

		}

		/// <summary>
		/// private methods 
		/// </summary>
		private Element CreateConnection()
		{
			++_creationCount;
			return new Element { Id = _creationCount, Name = "Zorba" + _creationCount, Description = "Desc" + _creationCount };
		}


		private void DestroyConnection(Element item)
		{

			++_destroyCount;
			item.Close();
		}

		
	}


}