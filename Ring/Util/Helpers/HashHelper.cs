namespace Ring.Util.Helpers;

internal static class HashHelper
{
    /// <summary>
    /// Hash code method: djb2 (xor version)
    /// </summary>
    internal static void Djb2X(string input, out int hash)
    {
        hash = 5381;
        var span = input.AsSpan();
        foreach (var c in span) hash ^= hash << 5 ^ c;
    }

    /// <summary>
    /// Hash code method: djb2 (xor version) 64 bits version
    /// </summary>
    internal static void Djb2X(string input, out long hash)
    {
        hash = 5381L;
        var span = input.AsSpan();
        foreach (var c in span) hash ^= hash << 5 ^ c;
    }
}
