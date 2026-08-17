using Ring.Data;
using Ring.PostgreSQL.Enums;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class ArrayExtensions
{
	private const int StackallocThreshold = 64; // bytes; larger bytea rent from the pool instead
	private const byte ErrorSeverity = (byte)ErrorTypeCode.Severity;
	private const byte ErrorCode = (byte)ErrorTypeCode.Code;
	private const byte ErrorMessage = (byte)ErrorTypeCode.Message;
	private const byte ErrorDetail = (byte)ErrorTypeCode.Detail;
	private const byte ErrorHint = (byte)ErrorTypeCode.Hint;

	internal static OperationalError ParseErrorFields(this byte[] body)
	{
		// Code size: 181 (0xb5)
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
		// string message, string state, string severity, string? detail, string hint, string? tableName
		var result = new OperationalError
		{
			Message = message ?? string.Empty,
			SqlState = sqlState ?? string.Empty,
			Severity = severity ?? string.Empty,
			Detail = detail,
			Hint = hint
		};
		return result;
	}

	// Postgres text-mode bytea is hex-encoded: '\x' followed by an even
	// number of hex digits (bytea_output = 'hex', the server default since
	// PG 9.0). This decodes that hex straight from the wire bytes - no
	// intermediate GetString - into raw bytes, then re-encodes as Base64 to
	// match what Record.GetField(out byte[]?) expects (Convert.FromBase64String).
	//
	// Kept out of AppendRecordData deliberately: a stackalloc anywhere in a
	// method is a JIT no-op for [AggressiveInlining] on that whole method
	// (confirmed elsewhere in this file), so the stackalloc below lives here
	// instead of poisoning AppendRecordData's inlining for every column type.
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static string ParseByteaHexToBase64(this byte[] body, int offset, int valueLength)
	{
		if (valueLength < 2 || body[offset] != (byte)'\\' || body[offset + 1] != (byte)'x')	ThrowInvalidByteaFormat();

		var hexLength = valueLength - 2;
		if ((hexLength & 1) != 0) ThrowInvalidByteaFormat();

		var byteCount = hexLength / 2;
		var hexStart = offset + 2;
		if (byteCount <= StackallocThreshold)
		{
			Span<byte> raw = stackalloc byte[byteCount];
			DecodeHex(body, hexStart, raw);
			return Convert.ToBase64String(raw);
		}

		var rented = ArrayPool<byte>.Shared.Rent(byteCount);
		try
		{
			var raw = rented.AsSpan(0, byteCount);
			DecodeHex(body, hexStart, raw);
			return Convert.ToBase64String(raw);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rented);
		}
	}

	#region private methods 

	private static string ReadCString(byte[] data, ref int offset)
	{
		// Code size: 47 (0x2f)
		var start = offset;
		while (offset < data.Length && data[offset] != 0) offset++;
		var value = Encoding.UTF8.GetString(data, start, offset - start);
		offset++; // skip null terminator
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void DecodeHex(byte[] source, int sourceOffset, Span<byte> destination)
	{
		// Code size: 62 (0x3e) - no virtual call
		for (var i = 0; i < destination.Length; i++)
		{
			var hi = HexNibble(source[sourceOffset + i * 2]);
			var lo = HexNibble(source[sourceOffset + i * 2 + 1]);
			destination[i] = (byte)((hi << 4) | lo);
		}
	}

	
	// '0'-'9' -> 0-9; 'a'-'f'/'A'-'F' -> 10-15 (the |0x20 folds upper to lower case)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int HexNibble(byte c) =>	c is >= (byte)'0' and <= (byte)'9' ? c - '0' : ((c | 0x20) - 'a') + 10; // Code size: 26 (0x1a)

	/// <summary>
	///     Throws an exception indicating the connection was closed by the server.
	/// </summary>
	// TODO: route through ResourceHelper/ResourceType like the other Throw*
	// helpers in this file once a matching resource entry exists - left as a
	// literal for now since ResourceType.cs wasn't available to edit.
	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidByteaFormat() =>
		throw new FormatException("Malformed bytea value received from server: expected Postgres hex format ('\\x' + an even number of hex digits).");

	#endregion

}
