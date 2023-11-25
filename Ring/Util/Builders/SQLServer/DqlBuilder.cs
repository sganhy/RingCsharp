using Ring.Schema.Enums;

namespace Ring.Util.Builders.SQLServer;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public DqlBuilder() : base() { }
    public override DatabaseProvider Provider => DatabaseProvider.SqlServer;
}
