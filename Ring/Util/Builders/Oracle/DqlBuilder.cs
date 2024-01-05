using Ring.Schema.Enums;

namespace Ring.Util.Builders.Oracle;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.Oracle;
    public DqlBuilder() : base() { }
}
