using Ring.Schema.Enums;

namespace Ring.Util.Builders.MySQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public DqlBuilder() : base() { }
    public override DatabaseProvider Provider => DatabaseProvider.MySql;

}
