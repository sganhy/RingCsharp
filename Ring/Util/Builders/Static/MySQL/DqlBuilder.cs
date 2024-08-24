using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.Static.MySQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.MySql;
    public DqlBuilder() : base() { }

}
