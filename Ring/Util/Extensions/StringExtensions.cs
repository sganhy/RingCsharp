using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class StringExtensions
{
    // Number of 100ns ticks per time unit
    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;
    private const long TicksPerMinute = TicksPerSecond * 60;
    private const long TicksPerHour = TicksPerMinute * 60;
    private const long TicksPerDay = TicksPerHour * 24;
    private static readonly int[] DaysToMonth365 = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
    private static readonly int[] DaysToMonth366 = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

    private readonly static CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Reduce the length of the string it if it is longer than the given maximum 'length'
    /// </summary>
    internal static string? Truncate(this string? source, int length) => source?.Length >= length ? source[..length] : source;

    /// <summary>
    /// Read a bit from a string
    /// </summary>
    /// <param name="value"></param>
    /// <param name="position">First bit equal to 0 position</param>
    internal static bool GetBitValue(this string value, int position) {
        var index = position >> 4; // divide by 16 (16 bits by char)
        if (index > value.Length) return false;
        return ((value[index] >> (position&0xF))&1)>0; // index + (position modulo 16)
    }

    /// <summary>
    /// Set to true a bit value
    /// </summary>
    internal static void SetBitValue(this string value, int position)
    {
        var index = position >> 4; // divide by 16 (16 bits by char)
        // avoid to get troubles with pointer in unsafe mode
        if (index>=value.Length) throw new ArgumentOutOfRangeException(string.Empty);
        var mask = (char)1;
        mask <<= position & 0xF;
        unsafe                // allows writing to memory; methods on System.String don't allow this
        {
            fixed (char* c = value) // get pointer to string originally stored in read only memory
                c[index] |= mask;
        }
    }

    /// <summary>
    /// Is string contains only digits. string cannot be null
    /// </summary>
    internal static bool IsNumber(this string value)
    {
        var count=value.Length;
        var i=count>0 && value[0]=='-' ?1:0;
        if (i==count) return false;
        while (i<count) if ((value[i++]^'0')>9) return false;
        return true;
    }

    /// <summary>
    /// Is string defined a float or double value
    /// </summary>
    internal static bool IsFloat(this string value)
    {
        // replace occurence of '.' or ',' by null
        var pos = value.IndexOf('.');
        if (pos < 0) return value.IsNumber();
        return string.Join(null,value[..pos], value[(pos+1)..]).IsNumber();
    }

    /// <summary>
    /// Remove a character from a string
    /// </summary>
    internal static string RemoveChar(this string? s, char c)
    {
        var len=s?.Length??0;
        if (len <= 0) return s;
        var newChars = new char[len]; // allocation could be avoided
        char cc;
        int index = 0;
        for (int i = 0; i < len; ++i)
        {
#pragma warning disable CS8602 // cannot be null
            cc = s[i];
#pragma warning restore CS8602
            if (cc != c)
            {
                newChars[index] = cc;
                ++index;
            }
        }
        return new string(newChars, 0, index);
    }

    internal static (DateTime?, TimeSpan?) ParseIso8601Date(this string value)
    {
        (var ticks, var dateSize) = GetDateTicks(value);
        ticks += GetTimeTicks(value, dateSize);
        return (new DateTime(ticks, DateTimeKind.Utc), null);
    }

    /// <summary>
    /// Extension method to test whether the value is a base64 string
    /// </summary>
    /// <param name="value">Value to test</param>
    /// <returns>Boolean value, true if the string is base64, otherwise false</returns>
    internal static bool IsBase64String(this string value)
    {
        if (value == null || value.Length == 0 || value.Length % 4 != 0
            || value.Contains(' ') || value.Contains('\t') || value.Contains('\r') || value.Contains('\n')) return false;

        var index = value.Length - 1;

        // if there is padding step back
        if (value[index] == '=') index--;

        // if there are two padding chars step back a second time
        if (value[index] == '=') index--;

        // Now traverse over characters
        for (var i = 0; i <= index; i++)
        {
            //if (!Base64Chars.Contains(value[i])) return false;
            var c = (int)value[i];
            if (c >= 47 && c <= 57) continue; // '/' U ['0'..'9']
            if (c >= 65 && c <= 90) continue;
            if (c >= 97 && c <= 122) continue;
            if (c != 43) return false;
        }

        // If we got here, then the value is a valid base64 string
        return true;
    }

    #region private methods 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetDateTimeItem(string? value, int startposition, int size, int defaultValue)
    {
        var substring = value?.Substring(startposition, size); // no callvirt here!
        return substring!=null && IsNumber(substring)?int.Parse(substring,DefaultCulture):defaultValue;
    }

    private static long GetTicks(int year, int month, int day)
    {
        var days = DateTime.IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
        if (day >= 1 && day <= days[month] - days[month - 1])
        {
            var y = year - 1;
            var n = y * 365 + y / 4 - y / 100 + y / 400 + days[month - 1] + day - 1;
            return n * TicksPerDay;
        }
        ThrowUnRepresentableDateTime();
        return default;
    }

    private static (long, int) GetDateTicks(string value)
    {
        var stringSize = value.Length;
        var ticks = 0L;
        var dateSize = 0;

        // min dateTime size
        if (stringSize >= 4)
        {
            var year = GetDateTimeItem(value, 0, 4, 1);  // read year
            var month = 1;
            var day = 1;
            var charIndex = value.IndexOf('-');
            dateSize = 4;
            if (charIndex < 10 && charIndex > 0)
            { // yyyy-MM-dd
                if (stringSize >= 7)
                {
                    month = GetDateTimeItem(value, 5, 2, 1);
                    dateSize += 3;
                }
                if (stringSize >= 10) 
                {
                    day = GetDateTimeItem(value, 8, 2, 1);
                    dateSize += 3;
                }
            }
            else
            { // simplified version yyyyMMdd
                if (stringSize >= 6)
                {
                    month = GetDateTimeItem(value, 4, 2, 1);
                    dateSize += 2;
                }
                if (stringSize >= 8)
                {
                    day = GetDateTimeItem(value, 6, 2, 1);
                    dateSize += 2;
                }
            }
            // manage weeks ??  
            // validate DateTime
            if (year >= 1 && year <= 9999 && month >= 1 && month <= 12) ticks += GetTicks(year, month, day);
            else ThrowUnRepresentableDateTime();
        } 
        else ThrowUnRepresentableDateTime();

        return (ticks, dateSize);
    }

    private static long GetTimeTicks(string value, int dateSize)
    {
        var timeString = value[dateSize..];
        timeString = timeString.RemoveChar(':');
        var stringSize = timeString.Length;
        var ticks = 0L;

        if (stringSize > 0) 
        {
            // T16:45
            if (!Equals('T', value[dateSize])||stringSize<=2) ThrowNotSupportedInputDateTime(value);
            // get hours
            if (stringSize>2) {
                var hours = GetDateTimeItem(timeString, 1, 2, 0);
                if (hours>23) ThrowNotSupportedInputDateTime(value);
                ticks += hours * TicksPerHour;
            }
            // get minutes
            if (stringSize>4)
            {
                var minutes = GetDateTimeItem(timeString, 3, 2, 0);
                if (minutes > 59) ThrowNotSupportedInputDateTime(value);
                ticks += minutes * TicksPerMinute;
            }
            // get seconds
            if (stringSize>6)
            {
                var seconds = GetDateTimeItem(timeString, 5, 2, 1);
                if (seconds > 59) ThrowNotSupportedInputDateTime(value);
                ticks += seconds * TicksPerSecond;
            }
        }
        return ticks;
    }

    private static void ThrowUnRepresentableDateTime() =>
        throw new ArgumentOutOfRangeException(ResourceHelper.GetErrorMessage(ResourceType.UnRepresentableDateTime));

    private static void ThrowNotSupportedInputDateTime(string input) =>
        throw new FormatException(string.Format(CultureInfo.InvariantCulture, 
            ResourceHelper.GetErrorMessage(ResourceType.NotSupportedInputDateTime), input));
    
    #endregion 

}
