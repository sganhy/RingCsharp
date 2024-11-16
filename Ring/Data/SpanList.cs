using System.Runtime.CompilerServices;

namespace Ring.Data;

internal struct SpanList<T> where T : struct
{
    //TODO use pool of array 
    private T[] _buffer;
	private int _count;

	public SpanList()
	{
		_buffer = Array.Empty<T>();
		_count = 0;
	}

	public SpanList(int initSize)
	{
		var initBucketSize = int.Max(2,initSize);// min 2 
		_buffer = new T[initBucketSize];
		_count = 0;
	}

	internal readonly int Count => _count;

	/// <summary>
	/// Indexer of TypedSpanList
	/// </summary>
	internal readonly ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get { return ref _buffer[index]; }
	}

	internal void Add(in T value)
	{
		var count = _count;
		if (count >= _buffer.Length) ReDim();
		_buffer[count] = value;
		++count;
        _count = count;
    }

	internal void Clear() => _count = 0;

	public readonly Enumerator GetEnumerator() => new(this);

	#region subclasses

	public ref struct Enumerator
	{
		private readonly SpanList<T> _span;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerator(SpanList<T> spanList)
		{
			_span = spanList;
			_index = -1;
		}

		/// <summary>Advances the enumerator to the next element of the span.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			var index = _index + 1;
			if (index < _span._count)
			{
				_index = index;
				return true;
			}
			return false;
		}

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		public ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _span[_index];
		}
	}

	#endregion

#pragma warning disable IDE0251 // Make member 'readonly'

	internal void Sort(Comparison<T> comparison) => Array.Sort(_buffer, comparison);
    
#pragma warning restore IDE0251

	#region private methods 

	private void ReDim()
	{
		var newSize = int.Max(_count<<1,4); 
		var buffer = new T[newSize];
		Array.Copy(_buffer,buffer, _buffer.Length);
		_buffer = buffer;
	}

	#endregion
}
