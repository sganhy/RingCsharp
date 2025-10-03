using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class StringExtensions
{
	private const char ZuluTimeSuffix = 'Z';
#pragma warning disable RCS1187 // Use constant instead of field
	private static readonly string Date4Suffix = "-01-01";
	private static readonly string Date7Suffix = "-01";
#pragma warning restore RCS1187
	private static readonly string ZuluTimeStrSuffix = ZuluTimeSuffix.ToString();
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	/// <summary>
	/// Reduce the length of the string it if it is longer than the given maximum 'length'
	/// </summary>
	internal static string? Truncate(this string? source, int length) => source?.Length >= length ? source[..length] : source; // Code size: 23 (0x17)

	internal static int CharCount(this string? source, char chr) 
	{
		// Code size: 50 (0x32)
		if (source is not null)
		{
			var count = 0;
			foreach (var c in source.AsSpan()) { if (c == chr) count++; }
			return count;
		}
		return 0;
	}

	internal static FieldType ToFieldType(this string? value) => value is not null && int.TryParse(value, out var intValue) ? intValue.ToFieldType() : FieldType.Undefined; // Code size: 23 (0x17)
	internal static EntityType ToEntityType(this string? value) => value is not null && int.TryParse(value, out var intValue) ? intValue.ToEntityType() : EntityType.Undefined; // Code size: 23 (0x17)

	/// <summary>
	/// Read a bit from a string
	/// </summary>
	internal static bool GetBitValue(this string value, int position)
	{
		// Code size: 36 (0x24)
		var index = position >> 4; // divide by 16 (16 bits by char)
		if (index > value.Length) return false;
		return ((value[index] >> (position & 0xF)) & 1) > 0; // index + (position modulo 16)
	}

	/// <summary>
	/// Set to true a bit value
	/// </summary>
	internal static void SetBitValue(this string value, int position)
	{
		// Code size: 68 (0x44)
		var index = position >> 4; // divide by 16 (16 bits by char)
								   // avoid to get troubles with pointer in unsafe mode
		if (index >= value.Length) throw new ArgumentOutOfRangeException(string.Empty);
		var mask = (char)1;
		mask <<= position & 0xF;
		unsafe			  // allows writing to memory; methods on System.String don't allow this
		{
			fixed (char* c = value) // get pointer to string originally stored in read only memory
				c[index] |= mask;
		}
	}

	internal static DateTimeOffset ParseIso8601Date(this string value)
	{
		// Code size: 259 (0x103)
		var spanValue = value.AsSpan();
		var stringSize = spanValue.Length;
		var i = 0;
		var preTemplate = new char[stringSize];
		while (i < stringSize) if ((spanValue[i] ^ '0') > 9) preTemplate[i] = spanValue[i++]; else preTemplate[i++] = '9';
		var template = new string(preTemplate);
		var valueSuffix = string.Empty;
		if (stringSize == 4) valueSuffix = Date4Suffix;
		else if (stringSize == 7) valueSuffix = Date7Suffix;
		var timeIndex = template.IndexOf('T', StringComparison.OrdinalIgnoreCase);
		var timeZoneIndex = GetTimeZoneIndex(template, timeIndex);
		var dateTemplate = GetDateTemplate(template, timeIndex);
		var timeTemplate = GetTimeTemplate(template, timeIndex, timeZoneIndex);
		var timeZoneTemplate = GetTimeZoneTemplate(template, timeIndex, timeZoneIndex);
		if (dateTemplate is not null &&
			DateTimeOffset.TryParseExact(value + valueSuffix, dateTemplate + timeTemplate + timeZoneTemplate,
				DefaultCulture, DateTimeStyles.AssumeUniversal, out var result))
		{
			return result;
		}
		throw new FormatException(string.Format(CultureInfo.InvariantCulture,
			ResourceHelper.GetErrorMessage(ResourceType.NotSupportedInputDateTime), value));
	}

	/// <summary>
	/// Extension method to test whether the value is a base64 string
	/// </summary>
	/// <param name="value">Value to test</param>
	/// <returns>Boolean value, true if the string is base64, otherwise false</returns>
	internal static bool IsBase64String(this string? value)
	{
		// Code size: 161 (0xa1)
#pragma warning disable RCS1113 // Use 'string.IsNullOrEmpty' method
		if (value is null || value.Length == 0 || value.Length % 4 != 0
			|| value.Contains(' ') || value.Contains('\t') || value.Contains('\r') || value.Contains('\n')) return false;
#pragma warning restore RCS1113
		var index = value.Length - 1;
		// if there is padding step back
		if (value[index] == '=') index--;
		// if there are two padding chars step back a second time
		if (value[index] == '=') index--;
		// Now traverse over characters
		for (var i = 0; i <= index; i++)
		{
			var c = (int)value[i];
			if (c >= 47 && c <= 57) continue; // '/' U ['0'..'9']
			if (c >= 65 && c <= 90) continue;
			if (c >= 97 && c <= 122) continue;
			if (c != 43) return false;
		}
		// If we got here, then the value is a valid base64 string
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static DateTime ToDateTime(this string value, FieldType fieldType)
	{
		// Code size: 192 (0xc0)
		var year = int.Parse(value.AsSpan(0, 4), DefaultCulture);
		var month = int.Parse(value.AsSpan(5, 2), NumberStyles.Integer, DefaultCulture);
		var day = int.Parse(value.AsSpan(8, 2), NumberStyles.Integer, DefaultCulture);
		if (fieldType == FieldType.ShortDateTime)
		{
			return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
		}
		else
		{
			var hour = int.Parse(value.AsSpan(11, 2), NumberStyles.Integer, DefaultCulture);
			var minute = int.Parse(value.AsSpan(14, 2), NumberStyles.Integer, DefaultCulture);
			var second = int.Parse(value.AsSpan(17, 2), NumberStyles.Integer, DefaultCulture);
			var milliSecond = int.Parse(value.AsSpan(20, 3), NumberStyles.Integer, DefaultCulture);
			if (fieldType == FieldType.DateTime) return new DateTime(year, month, day, hour, minute, second, milliSecond, DateTimeKind.Utc);
		}
		return DateTime.MinValue;
	}

	/// <summary>
	/// Is string contains only digits. string cannot be null
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsNumber(this string value)
	{
		// Code size: 71 (0x47)
		var span = value.AsSpan();
		var count = span.Length;
		var i = count > 0 && span[0] == '-' ? 1 : 0;
		if (i == count) return false;
		while (i < count) if ((span[i++] ^ '0') > 9) return false;
		return true;
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string? GetDateTemplate(string template, int timeIndex)
	{
		// Code size: 84 (0x54)
		switch (timeIndex > 0 ? template[..timeIndex] : template)
		{
			case "9999-99-99":
			case "9999-99":
			case "9999":
				return "yyyy-MM-dd";
			case "99999999":
				return "yyyyMMdd";
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetTimeZoneIndex(string template, int timeIndex)
	{
		if (timeIndex > 0)
		{
			if (template[^1] == ZuluTimeSuffix) return template.Length - 1;
			var index = template.LastIndexOf('+');
			if (index > 0) return index;
			index = template.LastIndexOf('-');
			if (index > timeIndex) return index;
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string? GetTimeTemplate(string template, int timeIndex, int timeZoneIndex)
	{
		// Code size: 445 (0x1bd)
		if (timeIndex > 0)
		{
			var time = template.AsSpan();
			if (timeZoneIndex > 0) time = time[..timeZoneIndex];
			time = time[timeIndex..];
			switch (time.ToString())
			{
				case "T99": return "THH";
				case "T99:99": return "THH:mm";
				case "T99:99:99": return "THH:mm:ss";
				case "T99:99:99.9": return "THH:mm:ss.f";
				case "T99:99:99.99": return "THH:mm:ss.ff";
				case "T99:99:99.999": return "THH:mm:ss.fff";
				case "T99:99:99.9999": return "THH:mm:ss.ffff";
				case "T99:99:99.99999": return "THH:mm:ss.fffff";
				case "T99:99:99.999999": return "THH:mm:ss.ffffff";
				case "T99:99:99.9999999": return "THH:mm:ss.fffffff";
				case "T9999": return "THHmm";
				case "T999999": return "THHmmss";
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string? GetTimeZoneTemplate(string template, int timeIndex, int timeZoneIndex)
	{
		// Code size: 169 (0xa9)
		if (timeIndex > 0 && timeZoneIndex > 0)
		{
			var spanTemlate = template.AsSpan();
			if (timeZoneIndex >= spanTemlate.Length - 1) return ZuluTimeStrSuffix;
			switch (spanTemlate[timeZoneIndex..].ToString())
			{
				case "+99:99":
				case "-99:99":
				case "-9999":
				case "+9999":
					return "zzz";
				case "+99":
				case "-99":
					return "zz";

			}
		}
		return null;
	}

	#endregion

}
