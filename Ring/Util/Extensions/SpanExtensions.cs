using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

static internal class SpanExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetIndex(this Span<int> span, int value)
    {
		// Code size: 57 (0x39)
		int indexerLeft = 0, indexerRight = span.Length - 1;
        while (indexerLeft <= indexerRight)
        {
            var indexerMiddle = (indexerLeft + indexerRight) >> 1;
            var indexerCompare = value - span[indexerMiddle];
            if (indexerCompare == 0) return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRight = indexerMiddle - 1;
        }
        return -1;
    }



}
