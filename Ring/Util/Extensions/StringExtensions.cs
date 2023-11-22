namespace Ring.Util.Extensions;

internal static class StringExtensions
{
    internal static string? Truncate(this string? source, int length) =>
        source?.Length >= length ? source[..length] : source;

}
