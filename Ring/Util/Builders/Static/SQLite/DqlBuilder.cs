using Ring.Schema.Enums;

namespace Ring.Util.Builders.Static.SQLite;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public DqlBuilder() : base() { }
    public override DatabaseProvider Provider => DatabaseProvider.SqlLite;
}
