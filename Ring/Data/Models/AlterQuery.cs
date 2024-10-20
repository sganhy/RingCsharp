using Ring.Data.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;

namespace Ring.Data.Models;

internal readonly struct AlterQuery
{
    internal readonly Table Table;
    internal readonly AlterQueryType Type;
    internal readonly IDdlBuilder Builder;
    internal readonly IColumn? Column;

    internal AlterQuery(Table table, AlterQueryType type, IDdlBuilder builder, IColumn? column=null)
    {
        Table = table;
        Type = type;
        Builder = builder;
        Column = column;
    }
}
