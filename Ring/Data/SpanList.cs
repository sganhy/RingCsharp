using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System;
using System.Collections;

namespace Ring.Data;

internal ref struct SpanList<T> where T : struct
{

#pragma warning disable IDE0044 // Add readonly modifier
    
    private Span<T> _buffer;
    private int _count;

#pragma warning restore IDE0044 

    public SpanList()
    {
        _buffer = new Span<T>(Array.Empty<T>());
        _count = 0;
    }

    public readonly int Count => _count;

    /// <summary>
    /// Indexer of TypedSpanList
    /// </summary>
    internal readonly T this[int index] => _buffer[index];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(in T value)
    {
        var count = _count;
        if (_count >= _buffer.Length) ReDim();
        _buffer[_count] = value;
        ++count; 
        _count = count;
    }

#pragma warning disable IDE0251 // Make member 'readonly'
    internal void Sort(Comparison<T> comparison)
    {
        if (_count > 1) _buffer.Sort(comparison);
    }
#pragma warning restore IDE0251


    #region private methods 

    private void ReDim()
    {
        var newSize = int.Max(_count, 4) << 1; // min 8 
        var buffer = new Span<T>(new T[newSize]);
        _buffer.CopyTo(buffer);
        _buffer = buffer;
    }

    #endregion
}
