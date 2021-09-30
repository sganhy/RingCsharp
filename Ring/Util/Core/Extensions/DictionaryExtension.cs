using System.Collections.Generic;

namespace Ring.Util.Core.Extensions
{
    internal static class DictionaryExtension
    {

        /// <summary>
        /// Return an array of KeyValuePair
        /// </summary>
        /// <typeparam name="T">Key Type</typeparam>
        /// <typeparam name="TU">Value type</typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        internal static KeyValuePair<T, TU>[] ToArray<T, TU>(this Dictionary<T, TU> dictionary) =>
            dictionary == null ? null : new List<KeyValuePair<T, TU>>(dictionary).ToArray();


        /// <summary>
        /// Return an array of KeyValuePair
        /// </summary>
        /// <typeparam name="T">Key Type</typeparam>
        /// <typeparam name="TU">Value type</typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        internal static TU[] ToArrayOfValue<T, TU>(this Dictionary<T, TU> dictionary) =>
            dictionary == null ? null : new List<TU>(dictionary.Values).ToArray();

        /// <summary>
        /// Try to get value 
        /// </summary>
        /// <typeparam name="T">Key Type</typeparam>
        /// <typeparam name="TU">Value type</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static TU TryGetValue<T, TU>(this Dictionary<T, TU> dictionary, T key)
        {
            if (dictionary == null) return default(TU);
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TU);
        }


        /// <summary>
        /// Try to add value
        /// </summary>
        /// <typeparam name="T">Key Type</typeparam>
        /// <typeparam name="TU">Value type</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static void TryToAdd<T, TU>(this Dictionary<T, TU> dictionary, T key, TU value)
        {
            if (dictionary == null) return;
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
        }

    }
}
