namespace Ring.Util.Helpers;

internal static class HashHelper
{
	/// <summary>
	/// 	Hash code method: djb2 (xor version) 32 bits version
	/// </summary>
	internal static void Djb2X(string input, out int hash)
	{
		hash = 5381;
		foreach (var c in input.AsSpan()) hash ^= hash << 5 ^ c;
	}

	/// <summary>
	/// 	Hash code method: djb2 (xor version) 64 bits version
	/// </summary>
	internal static void Djb2X(string input, out long hash)
	{
		hash = 5381L;
		foreach (var c in input.AsSpan()) hash ^= hash << 5 ^ c;
	}
}
