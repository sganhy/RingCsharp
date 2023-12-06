﻿using Ring.Schema.Models;
using System.Runtime.CompilerServices;

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
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, elements[indexerMiddle]);
            if (indexerCompare == 0) return true;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
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
        int indexerLeft = 0, indexerRigth = elements.Length - 1, indexerMiddle;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, elements[indexerMiddle]);
            if (indexerCompare == 0) return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return -1;
    }

    /// <summary>
    /// Get index string by value, case sensitive search ==> O(log n) complexity
    /// </summary>
    /// <param name="elements">sorted string array</param>
    /// <param name="value">fields name not null</param>
    /// <returns>String index or -1 if not found</returns>
    internal static int GetIndex(this Field[] elements, string value)
    {
        int iLeft = 0, iRigth = elements.Length - 1, iMiddle;
        while (iLeft <= iRigth)
        {
            iMiddle = iLeft + iRigth;
            iMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(value, elements[iMiddle].Name);
            if (indexerCompare == 0) return iMiddle;
            if (indexerCompare > 0) iLeft = iMiddle + 1;
            else iRigth = iMiddle - 1;
        }
        return -1;
    }

}
