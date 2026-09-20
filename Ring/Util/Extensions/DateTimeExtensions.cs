using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class DateTimeExtensions
{
	// templates
	private const int DecimalSys = 10;

	// ISO-8601 layout, shared by WriteTemplate and ToString so the template and the write positions cannot drift apart.
	// index : content
	//   0-3 yyyy | 4 '-' | 5-6 MM | 7 '-' | 8-9 dd | 10 'T' | 11-12 HH | 13 ':' | 14-15 mm | 16 ':' | 17-18 ss
	//   19 '.' | 20-22 milliseconds | 23-25 microseconds | 26 'Z' (DateTime) or offset sign (DateTimeOffset)
	//   27-28 offset HH | 29 ':' | 30-31 offset mm
	// Date:                   2005-12-12                           (10 chars)
	// DateTime:               2005-12-12T18:17:16.015000Z          (27 chars)
	// DateTime, skip ms:      2005-12-12T18:17:16Z                 (20 chars)
	// DateTimeOffset:         2005-12-12T18:17:16.015000+04:00     (32 chars, the longest)
	private const int YearEnd = 3;
	private const int MonthEnd = 6;
	private const int DayEnd = 9;
	private const int HourEnd = 12;
	private const int MinuteEnd = 15;
	private const int SecondEnd = 18;
	private const int MillisecondEnd = 22;
	private const int MicrosecondEnd = 25;
	private const int OffsetSign = 26;
	private const int OffsetHourEnd = 28;
	private const int OffsetMinuteEnd = 31;
	private const int MaxLength = OffsetMinuteEnd + 1; // stack buffer size (in chars)
	private const char Zero = '0';
	private const char DateSeparator = '-';
	private const char DateTimeSeparator = 'T';
	private const char TimeSeparator = ':';
	private const char FractionSeparator = '.';
	private const char UtcDesignator = 'Z';
	private const char PlusSign = '+';
	private const char MinusSign = '-';


	internal static string ToString(this in DateTime value, FieldType fieldType, bool skipmilliseconds, TimeSpan? offset)
	{
		// Code size: 329 (0x149)
		// IS0-8601 ==> "YYYY-MM-DDTHH:MM:SS.mmmmmZ" eg. 2005-12-12T18:17:16.015+04:00; lenght max ==> 32 (see layout above)
		// stackalloc must live in the caller: a span over stack memory cannot be returned from the method that allocated it.
		Span<char> buffer = stackalloc char[MaxLength];
		var result = buffer[..WriteTemplate(fieldType, skipmilliseconds, buffer)];
		var dateToConv = fieldType == FieldType.DateTime || (offset == null && fieldType != FieldType.Date) ?
			value.ToUniversalTime() : value;
		SetDateTime(result, 4, dateToConv.Year, YearEnd);
		SetDateTime(result, 2, dateToConv.Month, MonthEnd);
		SetDateTime(result, 2, dateToConv.Day, DayEnd);
		if (fieldType != FieldType.Date)
		{
			SetDateTime(result, 2, dateToConv.Hour, HourEnd);
			SetDateTime(result, 2, dateToConv.Minute, MinuteEnd);
			SetDateTime(result, 2, dateToConv.Second, SecondEnd);
			if (fieldType != FieldType.DateTime || !skipmilliseconds)
			{
				SetDateTime(result, 3, dateToConv.Millisecond, MillisecondEnd);
				SetDateTime(result, 3, dateToConv.Microsecond, MicrosecondEnd);
			}

			if (fieldType == FieldType.DateTimeOffset && offset != null)
			{
				int hours = offset.Value.Hours;
				int minutes = offset.Value.Minutes;
				if (offset.Value < TimeSpan.Zero) // not 'hours < 0': -00:30 has hours == 0
				{
					result[OffsetSign] = MinusSign;
					hours *= -1;
					minutes *= -1;
				}
				SetDateTime(result, 2, hours, OffsetHourEnd);
				SetDateTime(result, 2, minutes, OffsetMinuteEnd);
			}
		}
		return new string(result);
	}

	#region private methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetDateTime(Span<char> input, int size, int value, int lastPosition)
	{
		// Code size: 42 (0x2a)
		var i = 0;
		while (i < size)
		{
			input[lastPosition--] += (char)(value % DecimalSys);
			value /= DecimalSys;
			++i;
		}
	}

	/// <summary>
	/// Builds the '0'-filled template for <paramref name="fieldType"/> in <paramref name="destination"/> (SetDateTime adds
	/// the digits onto the '0' characters) and returns its length; 0 when the field type has no template.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteTemplate(FieldType fieldType, bool skipmilliseconds, Span<char> destination)
	{
		// Code size: 217 (0xd9)		
		int length;
		switch (fieldType)
		{
			case FieldType.Date:
				length = DayEnd + 1;
				break;
			case FieldType.DateTime:
				length = skipmilliseconds ? SecondEnd + 2 : MicrosecondEnd + 2; // + 'Z'
				break;
			case FieldType.DateTimeOffset:
				length = MaxLength; // skipmilliseconds does not apply to DateTimeOffset
				break;
			default:
				return 0;
		}

		var template = destination[..length];
		template.Fill(Zero);
		template[YearEnd + 1] = DateSeparator;
		template[MonthEnd + 1] = DateSeparator;
		if (fieldType == FieldType.Date) return length;

		template[DayEnd + 1] = DateTimeSeparator;
		template[HourEnd + 1] = TimeSeparator;
		template[MinuteEnd + 1] = TimeSeparator;

		if (fieldType == FieldType.DateTimeOffset)
		{
			template[SecondEnd + 1] = FractionSeparator;
			template[OffsetSign] = PlusSign;
			template[OffsetHourEnd + 1] = TimeSeparator;
		}
		else if (skipmilliseconds)
		{
			template[SecondEnd + 1] = UtcDesignator;
		}
		else
		{
			template[SecondEnd + 1] = FractionSeparator;
			template[MicrosecondEnd + 1] = UtcDesignator;
		}

		return length;
	}

	#endregion
}
