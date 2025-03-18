using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.MySQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public DqlBuilder() : base() { }
    public override DatabaseProvider Provider => DatabaseProvider.MySql;
    protected sealed override string GetSelection(Column column) => column.PhysicalName;

}
