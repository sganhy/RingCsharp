namespace Ring.Util.Extensions;

internal static class StringExtensions
{

	/// <summary>
	/// Reduce the length of the string it if it is longer than the given maximum 'length'
	/// </summary>
	internal static string? Truncate(this string? source, int length) => source?.Length >= length ? source[..length] : source; // Code size: 23 (0x17)

	internal static int CharCount(this string? source, char chr) 
	{
		// Code size: 34 (0x22)
		if (source != null)
		{
			var count = 0;
			var n = 0;
			while ((n = source.IndexOf(chr, n)) != -1)
			{
				++n;
				++count;
			}
			return count;
		}
		return 0;
	}

}
