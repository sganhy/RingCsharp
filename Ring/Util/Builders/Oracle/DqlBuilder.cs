using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.Oracle;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public sealed override DatabaseProvider Provider => DatabaseProvider.Oracle;
    public DqlBuilder() : base() { }
    protected sealed override string GetSelection(Field field) => field.PhysicalName;
    protected sealed override string GetSelection(Relation relation) => relation.PhysicalName;

}
