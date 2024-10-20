using Ring.Schema;
using Ring.Schema.Models;
using Ring.Data.Enums;
using Ring.Util.Builders;

namespace Ring.Data.Models;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct RetrieveQuery
{
#pragma warning restore CA1815

    // 56 bytes
    internal readonly Table Table;
    internal readonly RetrieveQueryType Type;
    internal readonly IDqlBuilder Builder;
    internal readonly int ParentQueryId;
    internal readonly List<RetrieveFilter> Filters;
    internal RetrieveSort? Sorts;
    internal PageInfo? Page;

    /// <summary>
    /// Ctor
    /// </summary>
    public RetrieveQuery()
    {
        Table = Meta.GetEmptyTable(new Meta());
        Type = RetrieveQueryType.Undefined;
        Builder = new Util.Builders.PostgreSQL.DqlBuilder();
        ParentQueryId = 0;
        Sorts = null;
        Filters = new List<RetrieveFilter>();
        Page = null;
    }
    internal RetrieveQuery(Table table, RetrieveQueryType type, IDqlBuilder builder, int parentQueryId)
    {
        Table = table;
        Type = type;
        Builder = builder;
        ParentQueryId = parentQueryId;
        Sorts = null;
        Filters = new List<RetrieveFilter>();
        Page = null;
    }

}
