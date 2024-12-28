using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.MySQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public DqlBuilder() : base() { }
    public override DatabaseProvider Provider => DatabaseProvider.MySql;
    protected sealed override string GetSelection(Field field) => field.PhysicalName;
    protected sealed override string GetSelection(Relation relation) => relation.PhysicalName;

}
