using System.Collections.Generic;

namespace Ring.Util.Core.Extensions
{
    internal static class SortedDictionaryExtension
    {

        /// <summary>
        /// Return an array of KeyValuePair
        /// </summary>
        internal static KeyValuePair<T, TU>[] ToArray<T, TU>(this SortedDictionary<T, TU> dictionary) =>
            dictionary == null ? null : new List<KeyValuePair<T, TU>>(dictionary).ToArray();

        /// <summary>
        /// Try to get value 
        /// </summary>
        internal static TU TryGetValue<T, TU>(this SortedDictionary<T, TU> dictionary, T key)
        {
            if (dictionary == null) return default(TU);
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TU);
        }

        /// <summary>
        ///  Deep copy of Dictionary 
        /// </summary>
        internal static SortedDictionary<T, TU> Copy<T, TU>(this SortedDictionary<T, TU> dictionary)
        {
            var result = new SortedDictionary<T, TU>();
            var arr = dictionary.ToArray();
            for (var i = 0; i < arr.Length; ++i) result.Add(arr[i].Key, arr[i].Value);
            return result;
        }


        /// <summary>
        /// Try to add value
        /// </summary>
        internal static void TryToAdd<T, TU>(this SortedDictionary<T, TU> dictionary, T key, TU value)
        {
            if (dictionary == null) return;
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
        }

    }
}
