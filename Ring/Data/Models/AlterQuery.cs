using Ring.Data.Enums;
using Ring.Schema;
using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;
using Ring.Util.Builders;

namespace Ring.Data.Models;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct AlterQuery
{
#pragma warning restore CA1815
    internal readonly int Id;
    internal readonly Table Table;
    internal readonly AlterQueryType Type;
    internal readonly IDdlBuilder Builder;
    internal readonly IColumn? Column;
    internal readonly Constraint? Constraint;
    internal readonly Index? Index;
    internal readonly TableSpace? TableSpace;


    /// <summary>
    /// Ctor
    /// </summary>
    public AlterQuery()
    {
        Id = -1;
        Table = Meta.GetEmptyTable(new Meta(string.Empty));
        Type = AlterQueryType.Undefined;
        Builder = new Util.Builders.PostgreSQL.DdlBuilder();
        Column = null;
        Constraint = null;
        Index = null;
        TableSpace = null;
    }

    internal AlterQuery(int id, Table table, AlterQueryType type, IDdlBuilder builder, IColumn? column, Constraint? constraint, 
        Index? index, TableSpace? tableSpace)
    {
        Id = id;
        Table = table;
        Type = type;
        Builder = builder;
        Column = column;
        Constraint = constraint;
        Index = index;
        TableSpace = tableSpace;
    }
}
