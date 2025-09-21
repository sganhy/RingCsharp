using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

static internal class SpanExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetIndex(this Span<int> span, int value)
    {
        // Code size: 70 (0x46)
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
