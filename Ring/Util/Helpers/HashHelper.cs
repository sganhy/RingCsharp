namespace Ring.Util.Helpers;

internal static class HashHelper
{

	/// <summary>
	/// 	Hash algorithm: DJB-2 (add version) 32 bits version
	/// </summary>
	internal static void Djb2A(string input, out int hash)
	{
		// Code size: 54 (0x36)
		hash = 5381;
		foreach (var c in input.AsSpan()) hash = (hash << 5) + hash + c;  // Addition version: hash * 33 + c
	}

	/// <summary>
	/// 	Hash algorithm: DJB-2 (xor version) 32 bits version
	/// </summary>
	internal static void Djb2X(string input, out int hash)
	{
		// Code size: 54 (0x36)
		hash = 5381;
		foreach (var c in input.AsSpan()) hash = ((hash << 5) + hash) ^ c;
	}

	/// <summary>
	/// 	Hash algorithm: DJB-2 (xor version) 64 bits version
	/// </summary>
	internal static void Djb2X(string input, out long hash)
	{
		// Code size: 56 (0x38)
		hash = 5381L;
		foreach (var c in input.AsSpan()) hash = ((hash << 5) + hash) ^ c;
	}
}
