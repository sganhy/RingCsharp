using Ring.Data.Enums;
using Ring.Schema;
using Ring.Schema.Models;
using Ring.Util.Builders;

namespace Ring.Data.Models;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct AlterQuery
{
#pragma warning restore CA1815

    internal readonly Table Table;
    internal readonly AlterQueryType Type;
    internal readonly IDdlBuilder Builder;
    internal readonly IColumn? Column;

    /// <summary>
    /// Ctor
    /// </summary>
    public AlterQuery()
    {
        Table = Meta.GetEmptyTable(new Meta(string.Empty));
        Type = AlterQueryType.Undefined;
        Builder = new Util.Builders.PostgreSQL.DdlBuilder();
        Column = null;
    }

    internal AlterQuery(Table table, AlterQueryType type, IDdlBuilder builder, IColumn? column=null)
    {
        Table = table;
        Type = type;
        Builder = builder;
        Column = column;
    }
}
