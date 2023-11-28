namespace Ring.Util.Extensions;

internal static class StringExtensions
{
    internal static string? Truncate(this string? source, int length) =>
        source?.Length >= length ? source[..length] : source;

    internal static bool GetBitValue(this string? value, int position) {
        if (value == null) return false;
        var index = position >> 4; // divide by 16 (16 bits by char)
        if (index > value.Length) return false;
        return ((value[index] >> ((position&0xFFFF)-1))&1)>0;
    }

    /// <summary>
    /// Set to true a bit value
    /// </summary>
    internal static string SetBitValue(this string? value, int position)
    {
        var index = position >> 4; // divide by 16 (16 bits by char)
        char[] result;
        if (value == null)
        {
            result = new char[index + 1];
        }
        else if (value.Length > index + 1) { 

        }
        else 
        {

        }

        return string.Empty;   
    }

}
