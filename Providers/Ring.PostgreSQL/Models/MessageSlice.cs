using System.Buffers;
using System.Runtime.CompilerServices;
using Ring.Data;
using Ring.PostgreSQL.Extensions;

namespace Ring.PostgreSQL.Models;

internal readonly struct MessageSlice
{
	internal readonly byte Code;
	internal readonly ReadOnlySequence<byte> Body;
	internal readonly SequencePosition EndPosition;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal MessageSlice(byte code, ReadOnlySequence<byte> body, SequencePosition endPosition)
	{
		Code = code;
		Body = body;
		EndPosition = endPosition;
	}

	/// <summary>
	///		Parses Postgres error fields directly from the sequence with zero GC allocations.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal OperationalError ParseErrorFields()
	{
		if (Body.IsEmpty) return new OperationalError();
		if (Body.IsSingleSegment) return (Body.FirstSpan).ParseErrorFields();

		var length = (int)Body.Length;
		byte[]? rented = null;
		Span<byte> span = length <= 128
			? stackalloc byte[length]
			: (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);

		try
		{
			Body.CopyTo(span);
			return ((ReadOnlySpan<byte>)span).ParseErrorFields();
		}
		finally
		{
			if (rented != null)
				ArrayPool<byte>.Shared.Return(rented);
		}
	}

	/// <summary>
	///		Copies the body into a destination span without creating managed byte[] arrays.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void CopyTo(Span<byte> destination)
	{
		if (Body.IsEmpty) return;
		Body.CopyTo(destination);
	}
}