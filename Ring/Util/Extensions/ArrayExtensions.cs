namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{
	internal static HashSet<string> ToHashSet(this string?[] elements)
	{
		// Code size: 59 (0x3b)
		var span = new ReadOnlySpan<string?>(elements);
        // 2x capacity: Avoid resizing overhead, maintains O(1) operations.
        var result = new HashSet<string>(elements.Length >> 1); 
		foreach (var element in span) if (element!=null) result.Add(element);
		return result;
	}

}
