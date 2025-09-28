namespace Ring.Util.Extensions;

internal static class StringExtensions
{

	/// <summary>
	/// Reduce the length of the string it if it is longer than the given maximum 'length'
	/// </summary>
	internal static string? Truncate(this string? source, int length) => source?.Length >= length ? source[..length] : source; // Code size: 23 (0x17)

	internal static int CharCount(this string? source, char chr) 
	{
        // Code size: 50 (0x32)
        if (source != null)
		{
			var count = 0;
            foreach (var c in source.AsSpan()) { if (c == chr) count++; }
            return count;
		}
		return 0;
	}

}
