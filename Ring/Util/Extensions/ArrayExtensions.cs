﻿using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{
    /// <summary>
    /// Find a string in a sorted array of string
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Exists(this string[] elements, string value)
    {
        int indexerLeft = 0, indexerRigth = elements.Length - 1;
        var iteration = 0;
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, elements[indexerMiddle]);
            if (indexerCompare == 0) return true;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
            ++iteration;
        }
        return false;
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
        int indexerLeft = 0, indexerRigth = elements.Length - 1;
        var iteration = 0;
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, elements[indexerMiddle]);
            if (indexerCompare == 0) return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
            ++iteration;
        }
        return -1;
    }

}