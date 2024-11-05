using System.Runtime.CompilerServices;

namespace Ring.Data;

public ref struct SpanListEnumerator
{
    private readonly SpanList<T> _span;
    /// <summary>The next index to yield.</summary>
    private int _index;

    /// <summary>Initialize the enumerator.</summary>
    /// <param name="span">The span to enumerate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Enumerator(Span<T> span)
    {
        _span = span;
        _index = -1;
    }

    /// <summary>Advances the enumerator to the next element of the span.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        int index = _index + 1;
        if (index < _span.Length)
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
