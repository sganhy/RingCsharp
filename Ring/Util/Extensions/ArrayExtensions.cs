using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{
	internal static HashSet<string> ToHashSet(this string?[] elements)
	{
		var span = new ReadOnlySpan<string?>(elements);
		// multiply by 4 size of bucket to reduce collisions (4 times is optimal on string)
		var result = new HashSet<string>(elements.Length * 4); 
		foreach (var element in span) if (element!=null && !result.Contains(element)) result.Add(element);
		return result;
	}
}
