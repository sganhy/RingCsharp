namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{
	internal static HashSet<string> ToHashSet(this string?[] elements)
	{
		// Code size: 59 (0x3b)
		var span = new ReadOnlySpan<string?>(elements);
        // 2x capacity: Avoid resizing overhead, maintains O(1) operations.
        var result = new HashSet<string>(elements.Length * 2); 
		foreach (var element in span) if (element!=null) result.Add(element);
		return result;
	}

    internal static bool ArraysEqual<T>(this T[] left, T[] right) where T : IEquatable<T>
    {
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
            if (!left[i].Equals(right[i])) return false;
        return true;
    }

}
