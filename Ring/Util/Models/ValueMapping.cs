namespace Ring.Util.Models;

internal readonly struct ValueMapping
{
    readonly internal int Id;
    readonly internal string Value;

    internal ValueMapping(int id, string value)
    {
        Id = id;
        Value = value;
    }
}
