using Ring.Schema.Enums;

namespace Ring.Util.Builders.MySQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.MySql;
    public DqlBuilder() : base() { }
}
