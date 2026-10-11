using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Ring.Data;
using Ring.PostgreSQL.Enums;

namespace Ring.PostgreSQL.Extensions;

internal static class ArrayExtensions
{
	private const int StackallocThreshold = 128;
	private const byte ErrorSeverity = (byte)ErrorTypeCode.Severity;
	private const byte ErrorCode = (byte)ErrorTypeCode.Code;
	private const byte ErrorMessage = (byte)ErrorTypeCode.Message;
	private const byte ErrorDetail = (byte)ErrorTypeCode.Detail;
	private const byte ErrorHint = (byte)ErrorTypeCode.Hint;
	private const byte ErrorTableName = (byte)ErrorTypeCode.TableName;
	private const byte ErrorSchemaName = (byte)ErrorTypeCode.SchemaName;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static OperationalError ParseErrorFields(this Span<byte> body) => ParseErrorFields((ReadOnlySpan<byte>)body);

	internal static OperationalError ParseErrorFields(this ReadOnlySpan<byte> body)
	{
		string? severity = null, sqlState = null, message = null, detail = null, hint = null, tableName = null, schemaName = null;

		var offset = 0;
		while (offset < body.Length)
		{
			var field = body[offset++];
			if (field == 0) break;

			var value = ReadCStringSpan(body, ref offset);

			switch (field)
			{
				case ErrorSeverity: severity = Decode(value); break;
				case ErrorCode: sqlState = Decode(value); break;
				case ErrorMessage: message = Decode(value); break;
				case ErrorDetail: detail = Decode(value); break;
				case ErrorHint: hint = Decode(value); break;
				case ErrorTableName: tableName = Decode(value); break;
				case ErrorSchemaName: schemaName = Decode(value); break;
			}
		}

		return new OperationalError(
			errorType: OperationalErrorType.PgError,
			message: message ?? string.Empty,
			sqlState: sqlState ?? string.Empty,
			severity: severity ?? string.Empty,
			detail: detail,
			hint: hint,
			tableName: tableName,
			schemaName: schemaName
		);
	}

	internal static OperationalError ParseErrorFieldsFromSequence(this ReadOnlySequence<byte> sequence)
	{
		if (sequence.IsSingleSegment)
		{
			return sequence.FirstSpan.ParseErrorFields();
		}

		var length = (int)sequence.Length;
		if (length <= StackallocThreshold)
		{
			Span<byte> span = stackalloc byte[length];
			sequence.CopyTo(span);
			return span.ParseErrorFields();
		}

		var rented = ArrayPool<byte>.Shared.Rent(length);
		try
		{
			var span = rented.AsSpan(0, length);
			sequence.CopyTo(span);
			return span.ParseErrorFields();
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ParseByteaHexToBase64(this Span<byte> body, int offset, int valueLength) =>
		ParseByteaHexToBase64((ReadOnlySpan<byte>)body, offset, valueLength);

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static string ParseByteaHexToBase64(this ReadOnlySpan<byte> body, int offset, int valueLength)
	{
		if (valueLength < 2 || body[offset] != (byte)'\\' || body[offset + 1] != (byte)'x')
			ThrowInvalidByteaFormat();

		var hexLength = valueLength - 2;
		if ((hexLength & 1) != 0) ThrowInvalidByteaFormat();

		var rawByteCount = hexLength / 2;
		var hexSource = body.Slice(offset + 2, hexLength);

		if (rawByteCount <= StackallocThreshold)
		{
			Span<byte> raw = stackalloc byte[rawByteCount];
			DecodeHex(hexSource, raw);
			return Convert.ToBase64String(raw);
		}

		var rented = ArrayPool<byte>.Shared.Rent(rawByteCount);
		try
		{
			var raw = rented.AsSpan(0, rawByteCount);
			DecodeHex(hexSource, raw);
			return Convert.ToBase64String(raw);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}

	#region Private Methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string Decode(ReadOnlySpan<byte> value) => value.IsEmpty ? string.Empty : Encoding.UTF8.GetString(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ReadOnlySpan<byte> ReadCStringSpan(ReadOnlySpan<byte> data, ref int offset)
	{
		var remaining = data[offset..];
		var nullIdx = remaining.IndexOf((byte)0);

		if (nullIdx < 0)
		{
			offset = data.Length;
			return remaining;
		}

		var value = remaining.Slice(0, nullIdx);
		offset += nullIdx + 1;
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void DecodeHex(ReadOnlySpan<byte> source, Span<byte> destination)
	{
		for (var i = 0; i < destination.Length; i++)
		{
			var hi = HexNibble(source[i * 2]);
			var lo = HexNibble(source[i * 2 + 1]);
			destination[i] = (byte)((hi << 4) | lo);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int HexNibble(byte c)
	{
		var cInt = (int)c;
		return (cInt & 0xF) + (cInt >> 6) * 9;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidByteaFormat() =>
		throw new FormatException("Malformed bytea value received from server: expected Postgres hex format ('\\x' + an even number of hex digits).");

	#endregion
}