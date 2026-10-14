using Ring.Schema.Models;
using Ring.Data.Enums;
using Ring.Data.Extensions;

namespace Ring.Data.Models;

public struct RetrieveQuery : IEquatable<RetrieveQuery>
{
    // 56 bytes
    internal readonly Table Table;
    internal readonly RetrieveQueryType Type;
    internal readonly int ParentQueryId;
    internal readonly PooledList<RetrieveFilter> Filters;
    internal readonly PooledList<RetrieveSort> Sorts;
    internal readonly PageInfo? Page;

    /// <summary>
    ///     Ctor
    /// </summary>
    internal RetrieveQuery(Table table, RetrieveQueryType type, int parentQueryId)
    {
        Table = table;
        Type = type;
        ParentQueryId = parentQueryId;
        Sorts = new PooledList<RetrieveSort>();
		Filters = new PooledList<RetrieveFilter>();
        Page = null;
    }

	public static bool operator ==(RetrieveQuery left, RetrieveQuery right) => left.Equals(right);
	public static bool operator !=(RetrieveQuery left, RetrieveQuery right) => !left.Equals(right);
	public readonly bool Equals(RetrieveQuery other) => this.IsEquivalentTo(other);
	public override readonly int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Type} - {Table}";
#endif

}
