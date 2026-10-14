using System.Runtime.CompilerServices;

namespace Ring.Data;

public ref struct SpanEnumerator<T> where T : struct
{
	private readonly Span<T> _items;
	private int _index;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal SpanEnumerator(Span<T> items)
	{
		// Code size: 15 (0xf)
		_items = items;
		_index = -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		// Code size: 34 (0x22)
		var index = _index + 1;
		if (index < _items.Length)
		{
			_index = index;
			return true;
		}
		return false;
	}

	public readonly ref T Current
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref _items[_index]; // Code size: 18 (0x12) - no virtual calls 
	}
}
