using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.SQLite;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public sealed override DatabaseProvider Provider => DatabaseProvider.SqlLite;
    public DqlBuilder() : base() { }
    protected sealed override string GetSelection(in Column column) => column.PhysicalName;

}
