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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static OperationalError ParseErrorFields(this Span<byte> body) => ParseErrorFields((ReadOnlySpan<byte>)body); // Code size: 12 (0xc)

	internal static OperationalError ParseErrorFields(this ReadOnlySpan<byte> body)
	{
		// Code size: 217 (0xd9)
		string? severity = null, sqlState = null, message = null, detail = null, hint = null;

		// ErrorResponse body: repeated (1-byte field code, NUL-terminated string), ended by a single 0 byte.
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
					// every other field (position, schema, table, file, line, routine...) is skipped
			}
		}

		return new OperationalError
		{
			Message = message ?? string.Empty,
			SqlState = sqlState ?? string.Empty,
			Severity = severity ?? string.Empty,
			Detail = detail,
			Hint = hint
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ParseByteaHexToBase64(this Span<byte> body, int offset, int valueLength) =>
		ParseByteaHexToBase64((ReadOnlySpan<byte>)body, offset, valueLength);

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static string ParseByteaHexToBase64(this ReadOnlySpan<byte> body, int offset, int valueLength)
	{
		// Code size: 177 (0xb1)
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

	private static string Decode(ReadOnlySpan<byte> value) => value.IsEmpty ? string.Empty : Encoding.UTF8.GetString(value); // Code size: 27 (0x1b)

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ReadOnlySpan<byte> ReadCStringSpan(ReadOnlySpan<byte> data, ref int offset)
	{
		// Code size: 51 (0x33)	
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
		// Code size: 70 (0x46)
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
		// Code size: 14 (0xe)
		var cInt = (int)c;
		return (cInt & 0xF) + (cInt >> 6) * 9;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidByteaFormat() =>
		throw new FormatException("Malformed bytea value received from server: expected Postgres hex format ('\\x' + an even number of hex digits).");

	#endregion
}