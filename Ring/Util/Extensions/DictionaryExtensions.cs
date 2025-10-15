namespace Ring.Util.Extensions;

internal static class DictionaryExtensions
{
	/// <summary>
	/// Clears all values in the dictionary while preserving keys.
	/// Sets each value to its default (null for reference types, default for value types).
	/// </summary>
	public static void ClearValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue defaultValue) where TKey : notnull
	{
		// Code size: 66 (0x42)
		if (dictionary.Count == 0) return;
		// Use struct enumerator for zero-allocation iteration
		foreach (var kvp in dictionary) dictionary[kvp.Key] = defaultValue;
	}
}
