using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.SQLServer;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public sealed override DatabaseProvider Provider => DatabaseProvider.SqlServer;
    public DqlBuilder() : base() {}
    protected sealed override string GetSelection(Column column) => column.PhysicalName;
}
