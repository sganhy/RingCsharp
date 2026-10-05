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
	internal static OperationalError ParseErrorFields(this Span<byte> body) =>
		ParseErrorFields((ReadOnlySpan<byte>)body);

	internal static OperationalError ParseErrorFields(this ReadOnlySpan<byte> body)
	{
		(int Offset, int Length) severity = (-1, 0);
		(int Offset, int Length) sqlState = (-1, 0);
		(int Offset, int Length) message = (-1, 0);
		(int Offset, int Length) detail = (-1, 0);
		(int Offset, int Length) hint = (-1, 0);

		var offset = 0;
		while (offset < body.Length && body[offset] != 0)
		{
			var field = body[offset++];
			var start = offset;
			ReadCStringSpan(body, ref offset);

			var isNullTerminated = offset <= body.Length && (offset == start || body[offset - 1] == 0);
			var length = Math.Max(0, offset - start - (isNullTerminated ? 1 : 0));

			switch (field)
			{
				case ErrorSeverity: severity = (start, length); break;
				case ErrorCode: sqlState = (start, length); break;
				case ErrorMessage: message = (start, length); break;
				case ErrorDetail: detail = (start, length); break;
				case ErrorHint: hint = (start, length); break;
			}
		}

		return new OperationalError
		{
			Message = severity.Offset >= 0 && severity.Offset + severity.Length <= body.Length
				? Encoding.UTF8.GetString(body.Slice(message.Offset, message.Length)) : string.Empty,
			SqlState = sqlState.Offset >= 0 && sqlState.Offset + sqlState.Length <= body.Length
				? Encoding.UTF8.GetString(body.Slice(sqlState.Offset, sqlState.Length)) : string.Empty,
			Severity = severity.Offset >= 0 && severity.Offset + severity.Length <= body.Length
				? Encoding.UTF8.GetString(body.Slice(severity.Offset, severity.Length)) : string.Empty,
			Detail = detail.Offset >= 0 && detail.Offset + detail.Length <= body.Length
				? Encoding.UTF8.GetString(body.Slice(detail.Offset, detail.Length)) : null,
			Hint = hint.Offset >= 0 && hint.Offset + hint.Length <= body.Length
				? Encoding.UTF8.GetString(body.Slice(hint.Offset, hint.Length)) : null
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
	internal static ReadOnlySpan<byte> ReadCStringSpan(ReadOnlySpan<byte> data, ref int offset)
	{
		var remaining = data.Slice(offset);
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
	internal static string ReadCString(ReadOnlySpan<byte> data, ref int offset)
	{
		var span = ReadCStringSpan(data, ref offset);
		return span.IsEmpty ? string.Empty : Encoding.UTF8.GetString(span);
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