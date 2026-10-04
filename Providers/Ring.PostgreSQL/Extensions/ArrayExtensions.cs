using Ring.Data;
using Ring.PostgreSQL.Enums;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class ArrayExtensions
{
	private const int StackallocThreshold = 128; // Matched with PipeReader 128-byte threshold
	private const byte ErrorSeverity = (byte)ErrorTypeCode.Severity;
	private const byte ErrorCode = (byte)ErrorTypeCode.Code;
	private const byte ErrorMessage = (byte)ErrorTypeCode.Message;
	private const byte ErrorDetail = (byte)ErrorTypeCode.Detail;
	private const byte ErrorHint = (byte)ErrorTypeCode.Hint;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static OperationalError ParseErrorFields(this Span<byte> body) =>
		ParseErrorFields((ReadOnlySpan<byte>)body);

	internal static OperationalError ParseErrorFields(this ReadOnlySpan<byte> body)
	{
		string? severity = null, sqlState = null, message = null, detail = null, hint = null;
		var offset = 0;
		while (offset < body.Length && body[offset] != 0)
		{
			var field = body[offset++];
			var value = ReadCString(body, ref offset);
			switch (field)
			{
				case ErrorSeverity: severity = value; break;
				case ErrorCode: sqlState = value; break;
				case ErrorMessage: message = value; break;
				case ErrorDetail: detail = value; break;
				case ErrorHint: hint = value; break;
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
		if (valueLength < 2 || body[offset] != (byte)'\\' || body[offset + 1] != (byte)'x')
			ThrowInvalidByteaFormat();

		var hexLength = valueLength - 2;
		if ((hexLength & 1) != 0) ThrowInvalidByteaFormat();

		var byteCount = hexLength / 2;
		var hexStart = offset + 2;
		var hexSource = body.Slice(hexStart, hexLength);

		if (byteCount <= StackallocThreshold)
		{
			Span<byte> raw = stackalloc byte[byteCount];
			DecodeHex(hexSource, raw);
			return Convert.ToBase64String((ReadOnlySpan<byte>)raw);
		}

		var rented = ArrayPool<byte>.Shared.Rent(byteCount);
		try
		{
			var raw = rented.AsSpan(0, byteCount);
			DecodeHex(hexSource, raw);
			return Convert.ToBase64String((ReadOnlySpan<byte>)raw);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string ReadCString(ReadOnlySpan<byte> data, ref int offset)
	{
		var remaining = data.Slice(offset);
		var nullIdx = remaining.IndexOf((byte)0); // Vectorized SIMD search

		if (nullIdx < 0)
		{
			// Fallback if message isn't null-terminated
			var str = Encoding.UTF8.GetString(remaining);
			offset = data.Length;
			return str;
		}

		var value = Encoding.UTF8.GetString(remaining.Slice(0, nullIdx));
		offset += nullIdx + 1; // Advance past content + null terminator
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
	private static int HexNibble(byte c) => c is >= (byte)'0' and <= (byte)'9' ? c - '0' : ((c | 0x20) - 'a') + 10;

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidByteaFormat() =>
		throw new FormatException("Malformed bytea value received from server: expected Postgres hex format ('\\x' + an even number of hex digits).");

	#endregion
}