using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ring.Data;

/// <summary>
/// 	List of structs backed by a pooled array, usable as a class field. Call Dispose to return the buffer, and never copy the struct.
/// </summary>
internal struct PooledList<T> : IDisposable where T : struct
{
	private const int DefaultCapacity = 4;
	private const int MaxCapacity = 0x7FFFFFC7; // Array.MaxLength
	private T[] _buffer;
	private int _count;

	internal PooledList(int capacity)
	{
		// Code size: 31 (0x1f)
		_buffer = capacity == 0 ? Array.Empty<T>() : ArrayPool<T>.Shared.Rent(capacity);
		_count = 0;
	}

	internal readonly int Count => _count;
	internal readonly int Capacity => _buffer.Length; // Code size: 15 (0xf)
	internal readonly bool IsEmpty => _count == 0;

	internal readonly ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			// Code size: 18 (0x12) - no virtual calls
			return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), index);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Add(in T value)
	{
		// Code size: 59 (0x3b) - no virtual calls
		var buffer = _buffer;
		var count = _count;
		if (buffer is not null && (uint)count < (uint)buffer.Length)
		{
			buffer[count] = value;
			_count = count + 1;
		}
		else
		{
			AddWithGrow(value); // by value: 'in' may alias our own buffer, which Grow returns to the pool
		}
	}

	/// <summary>'items' must not alias this list's own buffer.</summary>
	internal void AddRange(ReadOnlySpan<T> items)
	{
		// Code size: 75 (0x4b)
		if (items.IsEmpty) return;
		EnsureCapacity(_count + items.Length);
		items.CopyTo(_buffer.AsSpan(_count));
		_count += items.Length;
	}

	internal void RemoveAt(int index)
	{
		// Code size: 64 (0x40)
		var buffer = _buffer;
		var count = --_count;
		if (index < count) Array.Copy(buffer, index + 1, buffer, index, count - index);
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) buffer[count] = default;
	}

	internal void EnsureCapacity(int capacity)
	{
		// Code size: 17 (0x11)
		if (capacity > Capacity) Grow(capacity);
	}

	internal void Clear()
	{
		// Code size: 42 (0x2a)
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() && _count > 0) Array.Clear(_buffer, 0, _count);
		_count = 0;
	}

	internal readonly void Sort(Comparison<T> comparison) => AsSpan().Sort(comparison);
	internal readonly T[] ToArray() => AsReadOnlySpan().ToArray();

	public readonly Span<T> AsSpan() => new(_buffer, 0, _count);
	public readonly ReadOnlySpan<T> AsReadOnlySpan() => new(_buffer, 0, _count);

	/// <summary>Returns the buffer to the pool. Safe to call twice on the same instance, NOT safe on copies.</summary>
	public void Dispose()
	{
		// Code size: 35 (0x23) - no virtual calls
		var buffer = _buffer;
		_buffer = Array.Empty<T>();
		_count = 0;
		if (buffer is not null) ReturnBuffer(buffer);
	}

	public readonly SpanEnumerator<T> GetEnumerator() => new(AsSpan());

	#region private methods

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddWithGrow(T value)
	{
		// Code size: 44 (0x2c) - no virtual calls
		Grow(_count + 1);
		_buffer[_count++] = value;
	}

	private void Grow(int required)
	{
		// Code size: 97 (0x61)
		var old = _buffer;
		var current = old.Length;
		var newCapacity = current == 0 ? DefaultCapacity : (int)Math.Min((long)current << 1, MaxCapacity);
		if (newCapacity < required) newCapacity = required;
		var next = ArrayPool<T>.Shared.Rent(newCapacity);
		if (old is not null)
		{
			old.AsSpan(0, _count).CopyTo(next);
			ReturnBuffer(old);
		}
		_buffer = next;
	}

	private static void ReturnBuffer(T[] buffer) =>	ArrayPool<T>.Shared.Return(buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());

	#endregion

}
