namespace Ring.Schema.Models;

internal sealed class LexiconItem
{
    internal readonly int Id;
    internal readonly int LexiconId;
    internal readonly string Value;

    internal LexiconItem(int id, int lexiconId, string value)
    {
        Id = id;
        LexiconId = lexiconId;
        Value = value;
    }
}