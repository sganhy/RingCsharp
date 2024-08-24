using Ring.Schema.Enums;

namespace Ring.Util.Builders.Static.SQLServer;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.SqlServer;
    public DqlBuilder() : base() { }
}
