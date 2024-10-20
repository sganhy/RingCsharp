using Ring.Schema;
using Ring.Schema.Models;
using Ring.Data.Enums;
using Ring.Util.Builders;
using System.Runtime.InteropServices;

namespace Ring.Data.Models;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct BulkRetrieveQuery
#pragma warning restore CA1815
{
    internal readonly Table Table;
    internal readonly BulkRetrieveQueryType Type;
    internal readonly IDqlBuilder Builder;
    internal readonly int ParentQueryId;
    internal BulkSort? Sorts;
    internal readonly List<BulkRetrieveFilter> Filters;

    /// <summary>
    /// Ctor
    /// </summary>
    public BulkRetrieveQuery()
    {
        Table = Meta.GetEmptyTable(new Meta());
        Type = BulkRetrieveQueryType.Undefined;
        Builder = new Ring.Util.Builders.PostgreSQL.DqlBuilder();
        ParentQueryId = 0;
        Sorts = null;
        Filters = new List<BulkRetrieveFilter>();
    }
    internal BulkRetrieveQuery(Table table, BulkRetrieveQueryType type, IDqlBuilder builder, int parentQueryId)
    {
        Table = table;
        Type = type;
        Builder = builder;
        ParentQueryId = parentQueryId;
        Sorts = null;
        Filters = new List<BulkRetrieveFilter>();
    }

}
