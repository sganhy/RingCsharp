using Ring.Data.Enums;
using Ring.Schema.Models;

namespace Ring.Data.Models;

public readonly struct SaveQuery
{
    // 40 bytes
    internal readonly Table Table;
    internal readonly SaveQueryType Type;
    internal readonly string?[] Data;
    internal readonly int Offset;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal SaveQuery(Table table, SaveQueryType type, string?[] data, int offset)
    {
        Table = table;
        Type = type;
        Data = data;
        Offset = offset;
    }

#if DEBUG
    public override string ToString() => $"{Table.Name} - {Type}; Offset: {Offset}";
#endif

}
