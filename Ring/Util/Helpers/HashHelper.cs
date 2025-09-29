namespace Ring.Util.Helpers;

internal static class HashHelper
{
    private const uint FNV_OFFSET_BASIS = 2166136261U;
    private const uint FNV_PRIME = 16777619U;

    /// <summary>
    /// 	Hash algorithm: FNV-1a 32 bits version
    /// </summary>
    public static void Fnv1a(string input, out int hash)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        hash = (int)Fnv1a(bytes); // Assuming a byte array hasher exists
    }

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

    #region private methods 

    private static uint Fnv1a(byte[] data)
    {
        uint hash = FNV_OFFSET_BASIS;
        unchecked // Prevents overflow exceptions
        {
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= FNV_PRIME;
            }
        }
        return hash;
    }

    #endregion 
}
