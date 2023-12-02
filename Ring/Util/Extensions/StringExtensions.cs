namespace Ring.Util.Extensions;

internal static class StringExtensions
{
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

}
