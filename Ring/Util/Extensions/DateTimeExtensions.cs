using Ring.Schema.Enums;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

internal static class DateTimeExtensions
{
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static string ToString(this in DateTime value, FieldType fieldType, bool skipMilliseconds, TimeSpan? offset)
	{
		// Code size: 98 (0x62)
		DateTime dateToConv = fieldType == FieldType.DateTime || (offset == null && fieldType != FieldType.Date)
			? value.ToUniversalTime()
			: value;

		int length = GetBufferLength(fieldType, skipMilliseconds);
		if (length == 0) return string.Empty;

		// string.Create allocates memory directly in the target string instance without stack-to-heap copies
		return string.Create(length, (dateToConv, fieldType, skipMilliseconds, offset), static (buffer, state) =>
		{
			var (dt, type, skipMs, off) = state;

			// Populate base template
			WriteTemplate(type, skipMs, buffer);

			// Write Digits (Using fast 2-digit writes instead of loop division)
			Write4Digits(buffer, 0, dt.Year);
			Write2Digits(buffer, 5, dt.Month);
			Write2Digits(buffer, 8, dt.Day);

			if (type == FieldType.Date) return;

			Write2Digits(buffer, 11, dt.Hour);
			Write2Digits(buffer, 14, dt.Minute);
			Write2Digits(buffer, 17, dt.Second);

			if (type != FieldType.DateTime || !skipMs)
			{
				Write3Digits(buffer, 20, dt.Millisecond);
				Write3Digits(buffer, 23, dt.Microsecond);
			}

			if (type == FieldType.DateTimeOffset && off.HasValue)
			{
				TimeSpan ts = off.Value;
				if (ts < TimeSpan.Zero)
				{
					buffer[26] = '-';
					ts = ts.Negate();
				}
				Write2Digits(buffer, 27, ts.Hours);
				Write2Digits(buffer, 30, ts.Minutes);
			}
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetBufferLength(FieldType fieldType, bool skipMilliseconds) => 
	fieldType switch
	{   // Code size: 49 (0x31)
		FieldType.Date => 10,
		FieldType.DateTime => skipMilliseconds ? 20 : 27,
		FieldType.DateTimeOffset => 32,
		_ => 0
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write2Digits(Span<char> buffer, int start, int value)
	{
		// Code size: 37 (0x25)
		buffer[start] = (char)('0' + (value / 10));
		buffer[start + 1] = (char)('0' + (value % 10));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write3Digits(Span<char> buffer, int start, int value)
	{
		// Code size: 59 (0x3b)
		buffer[start] = (char)('0' + (value / 100));
		buffer[start + 1] = (char)('0' + ((value / 10) % 10));
		buffer[start + 2] = (char)('0' + (value % 10));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write4Digits(Span<char> buffer, int start, int value)
	{
		// Code size: 84 (0x54)
		buffer[start] = (char)('0' + (value / 1000));
		buffer[start + 1] = (char)('0' + ((value / 100) % 10));
		buffer[start + 2] = (char)('0' + ((value / 10) % 10));
		buffer[start + 3] = (char)('0' + (value % 10));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteTemplate(FieldType fieldType, bool skipMs, Span<char> buffer)
	{
		// Code size: 156 (0x9c)
		buffer.Fill('0');
		buffer[4] = '-';
		buffer[7] = '-';

		if (fieldType == FieldType.Date) return;

		buffer[10] = 'T';
		buffer[13] = ':';
		buffer[16] = ':';

		if (fieldType == FieldType.DateTimeOffset)
		{
			buffer[19] = '.';
			buffer[26] = '+';
			buffer[29] = ':';
		}
		else if (skipMs)
		{
			buffer[19] = 'Z';
		}
		else
		{
			buffer[19] = '.';
			buffer[26] = 'Z';
		}
	}
}