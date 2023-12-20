namespace Ring.Util.Helpers;

internal static class HashHelper
{
    /// <summary>
    /// Hash code method: djb2 (xor version)
    /// </summary>
    internal static int Djb2X(string input)
    {
        var hash = 5381;
        var count = input.Length;
        for (var i=0; i<count; ++i) hash ^= hash << 5 ^ input[i];
        return hash;
    }
}
