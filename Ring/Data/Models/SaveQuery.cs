using Ring.Data.Enums;
using Ring.Schema;
using Ring.Schema.Models;
using Ring.Util.Builders;

namespace Ring.Data.Models;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct SaveQuery
{
#pragma warning restore CA1815

    // 40 bytes
    internal readonly Table Table;
    internal readonly SaveQueryType Type;
    internal readonly IDmlBuilder Builder;
    internal readonly string?[]? Data;
    internal readonly int Offset;

    /// <summary>
    /// Ctor
    /// </summary>
    public SaveQuery()
    {
        Table = Meta.GetEmptyTable(new Meta());
        Type = SaveQueryType.Undefined;
        Builder = new Util.Builders.PostgreSQL.DmlBuilder();
        Data = null;
        Offset =-1;
    }
    internal SaveQuery(Table table, SaveQueryType type, IDmlBuilder builder, string?[]? data, int offset)
    {
        Table = table;
        Type = type;
        Builder = builder;
        Data = data;
        Offset = offset;
    }
}
