using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class ReadOnlySequenceExtensions
{
	private const int HeaderSize = 5;                 // 1 byte code + int32 length
	private const int MaxBodyLength = 0x3FFFFFFF - 4;  // PostgreSQL caps a message at 1 GB - 1 (the length field includes its own 4 bytes)
	private const int ColumnCountSize = 2;             // int16 at the start of a DataRow
	private const int StackallocThreshold = 128;

	// Same text as bool.ToString(): constants, nothing to allocate or initialise.
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture); 
	private const byte PgTrueChar = (byte)'t';
	private const byte PgTrueDigit = (byte)'1';

	/// <summary>
	/// Reads the message header (code + body length). Returns false only when fewer than 5 bytes are buffered;
	/// a corrupt length throws, so callers never see a negative or absurd bodyLength.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryReadHeader(this in ReadOnlySequence<byte> buffer, out byte code, out int bodyLength)
	{
		// Code size: 71 (0x47)
		var first = buffer.FirstSpan;
		if (first.Length >= HeaderSize) // fast path: header inside the first segment
		{
			code = first[0];
			bodyLength = BinaryPrimitives.ReadInt32BigEndian(first.Slice(1, 4)) - 4;

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

		if (body.IsSingleSegment)
			AppendFromSpan(body.FirstSpan, encoding, table, cells, count);
		else
			AppendFromSequence(body, encoding, table, cells, count);

		if (required > 0) cells[required - 1] = null;
	}

	#region private helpers

	// Fast path: the whole row is one contiguous span. No SequenceReader, no Slice, one bounds check per cell.
	private static void AppendFromSpan(ReadOnlySpan<byte> row, Encoding encoding, Table table, string?[] cells, int count)
	{
		var offset = ColumnCountSize;

		foreach (ref readonly var column in new ReadOnlySpan<Column>(table.Columns))
		{
			if (column.Type == EntityType.SearchableColumn) continue;
			if (row.Length - offset < 4) break;

			var valueLength = BinaryPrimitives.ReadInt32BigEndian(row.Slice(offset, 4));
			offset += 4;

			var index = column.RecordIndex + count;
			if (valueLength < 0) { cells[index] = null; continue; }
			if (valueLength > row.Length - offset) ThrowInvalidMessageLength();

			var value = row.Slice(offset, valueLength);
			offset += valueLength;

			if (column.Type == EntityType.TimeZoneColumn) continue;

			cells[index] = Decode(value, column.FieldType, encoding);
		}
	}

	// Rare: the row straddles pipe segments. Cells are still decoded from the current span when they fit in it.
	private static void AppendFromSequence(in ReadOnlySequence<byte> body, Encoding encoding, Table table, string?[] cells, int count)
	{
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
					? Decode(unread.Slice(0, valueLength), column.FieldType, encoding)
					: DecodeStraddling(ref reader, valueLength, column.FieldType, encoding);
			}

			reader.Advance(valueLength);
		}
	}

	private static string Decode(ReadOnlySpan<byte> value, FieldType fieldType, Encoding encoding) => fieldType switch
	{
		FieldType.ByteArray => value.ParseByteaHexToBase64(0, value.Length),
		FieldType.Boolean => value.Length == 1 && (value[0] == PgTrueChar || value[0] == PgTrueDigit) ? BooleanTrue : BooleanFalse,
		_ => encoding.GetString(value)
	};

	// A single cell split across two segments: copy just that cell, then decode it like any other.
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string DecodeStraddling(ref SequenceReader<byte> reader, int length, FieldType fieldType, Encoding encoding)
	{
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
		bodyLength = BinaryPrimitives.ReadInt32BigEndian(header.Slice(1, 4)) - 4;
		if ((uint)bodyLength > MaxBodyLength) ThrowInvalidMessageLength();
		return true;
	}

	// Multi-segment error bodies are very rare and tiny: a plain copy keeps this simple.
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static OperationalError ParseErrorFieldsSlow(in ReadOnlySequence<byte> sequence) =>	sequence.ToArray().AsSpan().ParseErrorFields();

	private static void EnsureCapacity(ref string?[] cells, int usedCount, int required)
	{
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
