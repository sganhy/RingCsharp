using System.Runtime.CompilerServices;

namespace Ring.Console.Extensions;

internal static class ArraySegmentExtension
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetSegmentIndex(this ArraySegment<string> elements, string value)
    {
        int indexerLeft = 0, indexerRigth = elements.Count - 1, indexerMiddle;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, elements[indexerMiddle]);
            if (indexerCompare == 0) 
                return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return -1;
    }

}
