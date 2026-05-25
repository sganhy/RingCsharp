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

	public static int? GetInt32Value<TKey>(this Dictionary<TKey, string> dictionary, TKey key, int? defaultValue=null) where TKey : notnull
	{
		// Code size: 47 (0x2f)
		if (dictionary?.ContainsKey(key) == true)
		{
			int? result = int.TryParse(dictionary[key], out var parsedValue) ? parsedValue : null;
			return result;
		}
		return defaultValue;
	}

	public static string? GetStringValue<TKey>(this Dictionary<TKey, string> dictionary, TKey key, string? defaultValue = null) where TKey : notnull
	{
		// Code size: 27 (0x1b)
		if (dictionary?.ContainsKey(key) == true)
		{
			return dictionary[key] ?? defaultValue;
		}
		return defaultValue;
	}


}
