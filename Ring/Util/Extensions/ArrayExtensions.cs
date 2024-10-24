using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HashSet<string> ToHashSet(this string?[] elements)
    {
        var span = new ReadOnlySpan<string?>(elements);
        // multiply by 4 size of bucket to reduce collisions (4 times is optimal on string)
        var result = new HashSet<string>(elements.Length * 4); 
        foreach (var element in span) if (element!=null && !result.Contains(element)) result.Add(element);
        return result;
    }

    /// <summary>
    /// Get index string by value, case sensitive search ==> O(log n) complexity
    /// </summary>
    /// <param name="elements">sorted string array</param>
    /// <param name="value">anonymous not null string</param>
    /// <returns>String index or -1 if not found</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetIndex(this string[] elements, string value)
    {
        var span = new ReadOnlySpan<string>(elements);
        int indexerLeft = 0, indexerRigth = span.Length - 1, indexerMiddle;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, span[indexerMiddle]);
            if (indexerCompare == 0) return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return -1;
    }

}
