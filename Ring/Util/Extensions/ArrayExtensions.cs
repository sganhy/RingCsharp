using Ring.Schema.Models;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{
	internal static HashSet<string> ToHashSet(this string?[] elements)
	{
		// Code size: 68 (0x44)
		var span = new ReadOnlySpan<string?>(elements);
		// multiply by 4 size of bucket to reduce collisions (4 times is optimal on string)
		var result = new HashSet<string>(elements.Length * 4); 
		foreach (var element in span) if (element!=null && !result.Contains(element)) result.Add(element);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetIndex(this int[] array, int value)
	{
		// Code size: 70 (0x46)
		var span = new ReadOnlySpan<int>(array);
		int indexerLeft = 0, indexerRight = span.Length - 1, indexerMiddle, indexerCompare;
		while (indexerLeft <= indexerRight)
		{
			indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
			indexerCompare = value - span[indexerMiddle];
			if (indexerCompare == 0) return indexerMiddle;
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return -1;
	}


}
