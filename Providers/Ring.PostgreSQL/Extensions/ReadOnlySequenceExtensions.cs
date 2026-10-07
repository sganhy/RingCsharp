using Ring.Data;
using Ring.PostgreSQL.Enums;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class ReadOnlySequenceExtensions
{
	internal const int HeaderSize = 5;                 // 1 byte code + int32 length
	private const int MaxBodyLength = 0x3FFFFFFF - 4;  // PostgreSQL caps a message at 1 GB - 1 (the length field includes its own 4 bytes)
	private const int ColumnCountSize = 2;             // int16 at the start of a DataRow
	private const int StackallocThreshold = 128;

	// Same text as bool.ToString(): constants, nothing to allocate or initialise.
	private const string BooleanTrue = "True";
	private const string BooleanFalse = "False";
	private const byte PgTrueChar = (byte)'t';
	private const byte PgTrueDigit = (byte)'1';

	/// <summary>
	/// Reads the message header (code + body length). Returns false only when fewer than 5 bytes are buffered;
	/// a corrupt length throws, so callers never see a negative or absurd bodyLength.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryReadHeader(this in ReadOnlySequence<byte> buffer, out byte code, out int bodyLength)
	{
		// Code size: 70 (0x46)
		var first = buffer.FirstSpan;
		if (first.Length >= HeaderSize) // fast path: header inside the first segment
		{
			code = first[0];
			bodyLength = BinaryPrimitives.ReadInt32BigEndian(first[1..]) - 4;

			// one unsigned compare rejects both negative and oversized lengths
			if ((uint)bodyLength > MaxBodyLength) ThrowInvalidMessageLength();
			return true;
		}

		return TryReadHeaderSlow(buffer, out code, out bodyLength);
	}

	/// <summary>Parses an ErrorResponse body. Single segment (always, in practice): parsed in place, no copy.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static OperationalError ParseErrorFieldsFromSequence(this in ReadOnlySequence<byte> sequence) => // Code size: 27 (0x1b)
		sequence.IsSingleSegment
			? sequence.FirstSpan.ParseErrorFields()
			: ParseErrorFieldsSlow(sequence);

	/// <summary>
	/// Decodes one DataRow body into <paramref name="cells"/> at <paramref name="count"/> (growing the pooled array when needed).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void AppendRecordData(this in ReadOnlySequence<byte> body, Encoding encoding, Table table, ref string?[] cells, int count)
	{
		// Code size: 77 (0x4d)
		var required = count + table.RecordSize;
		if (required > cells.Length) EnsureCapacity(ref cells, count, required);

		if (body.IsSingleSegment) AppendFromSpan(body.FirstSpan, encoding, table, cells, count);
		else AppendFromSequence(body, encoding, table, cells, count);

		// Ensure array is non-empty before writing the trailing null terminator
		if (required > 0) cells[required - 1] = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryReadMessageHeader(this in ReadOnlySequence<byte> buffer, out BackendMessageCode code, out int payloadLength)
	{
		// Code size: 70 (0x46) - no virtual calls, no SequenceReader, no Slice, one bounds check per cell.
		var first = buffer.FirstSpan;
		if (first.Length >= HeaderSize) // Fast path: Header is contiguous in the first segment
		{
			code = (BackendMessageCode)first[0];
			payloadLength = BinaryPrimitives.ReadInt32BigEndian(first[1..]) - 4;

			if ((uint)payloadLength > MaxBodyLength) ThrowInvalidMessageLength();
			return true;
		}

		return TryReadMessageHeaderSlow(buffer, out code, out payloadLength);
	}

	#region private helpers

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool TryReadMessageHeaderSlow(in ReadOnlySequence<byte> buffer, out BackendMessageCode code, out int payloadLength)
	{
		// Code size: 95 (0x5f)
		if (buffer.Length < HeaderSize)
		{
			code = default;
			payloadLength = default;
			return false;
		}

		Span<byte> header = stackalloc byte[HeaderSize];
		buffer.Slice(0, HeaderSize).CopyTo(header);

		code = (BackendMessageCode)header[0];
		payloadLength = BinaryPrimitives.ReadInt32BigEndian(header[1..]) - 4;

		if ((uint)payloadLength > MaxBodyLength) ThrowInvalidMessageLength();
		return true;
	}

	// Fast path: the whole row is one contiguous span. No SequenceReader, no Slice, one bounds check per cell.
	private static void AppendFromSpan(ReadOnlySpan<byte> row, Encoding encoding, Table table, string?[] cells, int count)
	{
		// Code size: 177 (0xb1) - no virtual calls
		var offset = ColumnCountSize;

		foreach (ref readonly var column in new ReadOnlySpan<Column>(table.Columns))
		{
			if (column.Type == EntityType.SearchableColumn) continue;
			if (row.Length - offset < 4) break;

			// Micro-optimization 1: Slice explicitly by length instead of using range indexing [offset..]
			// to give the JIT explicit bounds and prevent range construction overhead.
			var valueLength = BinaryPrimitives.ReadInt32BigEndian(row.Slice(offset, 4));
			offset += 4;

			var index = column.RecordIndex + count;
			if (valueLength < 0) { cells[index] = null; continue; }
			if (valueLength > row.Length - offset) ThrowInvalidMessageLength();

			// Micro-optimization 2: Use row.Slice directly instead of range syntax row[offset..]
			var value = row.Slice(offset, valueLength);
			offset += valueLength;

			if (column.Type == EntityType.TimeZoneColumn) continue;

			cells[index] = Decode(value, column.FieldType, encoding);
		}
	}

	// Rare: the row straddles pipe segments. Cells are still decoded from the current span when they fit in it.
	private static void AppendFromSequence(in ReadOnlySequence<byte> body, Encoding encoding, Table table, string?[] cells, int count)
	{
		// Code size: 220 (0xdc)
		var reader = new SequenceReader<byte>(body);
		reader.Advance(ColumnCountSize);

		foreach (ref readonly var column in new ReadOnlySpan<Column>(table.Columns))
		{
			if (column.Type == EntityType.SearchableColumn) continue;
			if (!reader.TryReadBigEndian(out int valueLength)) break;

			var index = column.RecordIndex + count;
			if (valueLength < 0) { cells[index] = null; continue; }
			if (valueLength > reader.Remaining) ThrowInvalidMessageLength();

			if (column.Type != EntityType.TimeZoneColumn)
			{
				var unread = reader.UnreadSpan;
				cells[index] = unread.Length >= valueLength
					? Decode(unread[..valueLength], column.FieldType, encoding)
					: DecodeStraddling(ref reader, valueLength, column.FieldType, encoding);
			}

			reader.Advance(valueLength);
		}
	}

	private static string Decode(ReadOnlySpan<byte> value, FieldType fieldType, Encoding encoding) => fieldType switch
	{
		// Code size: 90 (0x5a)
		FieldType.ByteArray => value.ParseByteaHexToBase64(0, value.Length),
		FieldType.Boolean => value.Length == 1 && (value[0] == PgTrueChar || value[0] == PgTrueDigit) ? BooleanTrue : BooleanFalse,
		_ => encoding.GetString(value)
	};

	// A single cell split across two segments: copy just that cell, then decode it like any other.
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string DecodeStraddling(ref SequenceReader<byte> reader, int length, FieldType fieldType, Encoding encoding)
	{
		// Code size: 92 (0x5c)
		byte[]? rented = null;
		Span<byte> span = length <= StackallocThreshold
			? stackalloc byte[length]
			: (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);

		try
		{
			reader.TryCopyTo(span);
			return Decode(span, fieldType, encoding);
		}
		finally
		{
			if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
		}
	}

	// Not inlined on purpose: stackalloc would stop the JIT from inlining the fast path above.
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool TryReadHeaderSlow(in ReadOnlySequence<byte> buffer, out byte code, out int bodyLength)
	{
		// Code size: 95 (0x5f)
		if (buffer.Length < HeaderSize)
		{
			code = default;
			bodyLength = default;
			return false;
		}

		// header straddles two segments (rare)
		Span<byte> header = stackalloc byte[HeaderSize];
		buffer.Slice(0, HeaderSize).CopyTo(header);

		code = header[0];
		bodyLength = BinaryPrimitives.ReadInt32BigEndian(header[1..]) - 4;
		if ((uint)bodyLength > MaxBodyLength) ThrowInvalidMessageLength();
		return true;
	}

	// Multi-segment error bodies are very rare and tiny: a plain copy keeps this simple.
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static OperationalError ParseErrorFieldsSlow(in ReadOnlySequence<byte> sequence) =>
		sequence.ToArray().AsSpan().ParseErrorFields();

	private static void EnsureCapacity(ref string?[] cells, int usedCount, int required)
	{
		// Code size: 45 (0x2d)
		var pool = ArrayPool<string?>.Shared;
		var grown = pool.Rent(Math.Max(cells.Length * 2, required));
		Array.Copy(cells, grown, usedCount);
		pool.Return(cells, clearArray: true);
		cells = grown;
	}

	[DoesNotReturn]
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ThrowInvalidMessageLength() =>
		throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.InvalidMessageLengthFromServer));

	#endregion
}
