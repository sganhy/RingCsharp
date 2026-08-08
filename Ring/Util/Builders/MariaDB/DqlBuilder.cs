using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.MariaDB;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public DqlBuilder() : base() { }        
    public override DatabaseProvider Provider => DatabaseProvider.MariaDb;
    protected sealed override string GetSelection(in Column column) => column.PhysicalName;

}
