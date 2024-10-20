namespace Ring.Util.Extensions;

internal static class StringExtensions
{
      
    /// <summary>
    /// Reduce the length of the string it if it is longer than the given maximum 'length'
    /// </summary>
    internal static string? Truncate(this string? source, int length) => source?.Length >= length ? source[..length] : source;


}
