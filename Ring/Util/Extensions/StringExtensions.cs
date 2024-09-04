using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class StringExtensions
{
    private static readonly string Date4Suffix = "-01-01";
    private static readonly string Date7Suffix = "-01";
    private static readonly char ZuluTimeSuffix = 'Z';
    private static readonly string ZuluTimeStrSuffix = ZuluTimeSuffix.ToString();
    private readonly static CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    // Number of 100ns ticks per time unit - year info 
    private static readonly Dictionary<string, string> DateTimeTemplate = new() {
        { "9999-99-99",        "yyyy-MM-dd"},
        { "99999999",          "yyyyMMdd"},
        { "9999-99",           "yyyy-MM-dd"},
        { "9999",              "yyyy-MM-dd"},
        { "T99",               "THH"},
        { "T99:99",            "THH:mm"},
        { "T99:99:99",         "THH:mm:ss"},
        { "T99:99:99.9",       "THH:mm:ss.f"},
        { "T99:99:99.99",      "THH:mm:ss.ff"},
        { "T99:99:99.999",     "THH:mm:ss.fff"},
        { "T99:99:99.9999",    "THH:mm:ss.ffff"},
        { "T99:99:99.99999",   "THH:mm:ss.fffff"},
        { "T99:99:99.999999",  "THH:mm:ss.ffffff"},
        { "T99:99:99.9999999", "THH:mm:ss.fffffff"},
        { "T9999",      "THHmm"},
        { "T999999",    "THHmmss"},
        { "+99:99",     "zzz"},
        { "-99:99",     "zzz"},
        { "-9999",      "zzz"},
        { "+9999",      "zzz"},
        { "+99",         "zz"},
        { "-99",         "zz"}
    };

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
        var i=count>0 && value[0]=='-'?1:0;
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

    internal static DateTimeOffset ParseIso8601Date(this string value)
    {
        var stringSize = value.Length;
        var i=0;
        var preTemplate = new char[stringSize];
        while (i<stringSize) if ((value[i] ^ '0') > 9) preTemplate[i] = value[i++]; else preTemplate[i++] = '9';
        var template = new string(preTemplate);
        var valueSuffix = string.Empty;
        if (stringSize==4) valueSuffix = Date4Suffix;
        else if (stringSize==7) valueSuffix = Date7Suffix;
        var timeIndex = template.IndexOf('T', StringComparison.OrdinalIgnoreCase);
        var timeZoneIndex = GetTimeZoneIndex(template, timeIndex);
        var dateTemplate = GetDateTemplate(template,timeIndex); 
        var timeTemplate = GetTimeTemplate(template,timeIndex,timeZoneIndex); 
        var timeZoneTemplate = GetTimeZoneTemplate(template, timeIndex, timeZoneIndex);
        if (dateTemplate != null && 
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
    private static string? GetDateTemplate(string template, int timeIndex)
    {
        var result = timeIndex>0 ? template[..timeIndex] : template;
        return DateTimeTemplate.ContainsKey(result)?DateTimeTemplate[result]:null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetTimeZoneIndex(string template, int timeIndex)
    {
        if (timeIndex > 0)
        {
            if (template[^1]==ZuluTimeSuffix) return template.Length-1;
            var index = template.LastIndexOf('+');
            if (index > 0) return index;
            index = template.LastIndexOf('-');
            if (index>timeIndex) return index;
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetTimeTemplate(string template, int timeIndex, int timeZoneIndex)
    {
        if (timeIndex > 0)
        {
            string result=template;
            if (timeZoneIndex > 0) result=result[..timeZoneIndex];
            result= result[timeIndex..];
            return DateTimeTemplate.ContainsKey(result) ? DateTimeTemplate[result] : null;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetTimeZoneTemplate(string template, int timeIndex, int timeZoneIndex)
    {
        if (timeIndex > 0 && timeZoneIndex>0)
        {
            if (timeZoneIndex>=template.Length-1) return ZuluTimeStrSuffix;
            var result = template[timeZoneIndex..];
            return DateTimeTemplate.ContainsKey(result) ? DateTimeTemplate[result] : null;
        }
        return null;
    }

    #endregion 

}
