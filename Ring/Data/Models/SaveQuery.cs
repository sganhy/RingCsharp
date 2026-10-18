using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Schema.Models;

namespace Ring.Data.Models;

public readonly struct SaveQuery : IEquatable<SaveQuery>
{
	// Total: 21 bytes + 3 bytes padding => With 8-byte alignment: 24 bytes
	// No attribute is worth adding here, neither Sequential nor Auto nor Explicit!
	internal readonly Table Table;
    internal readonly SaveQueryType Type;
    internal readonly string?[] Data;
    internal readonly int Offset;

	/// <summary>
	///		Ctor
	/// </summary>
	internal SaveQuery(Table table, SaveQueryType type, string?[] data, int offset)
    {
        Table = table;
        Type = type;
        Data = data;
        Offset = offset;
    }

	public static bool operator ==(in SaveQuery left, in SaveQuery right) => left.Equals(right);
	public static bool operator !=(in SaveQuery left, in SaveQuery right) => !left.Equals(right);
	public override readonly bool Equals(object? obj) => obj is SaveQuery saveQuery && Equals(saveQuery);
	public readonly bool Equals(SaveQuery other) => this.IsEquivalentTo(other);
	public override readonly int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Table.Name} - {Type}; Offset: {Offset}";
#endif

}
