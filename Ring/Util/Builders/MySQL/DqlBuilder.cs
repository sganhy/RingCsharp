using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.MySQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public sealed override DatabaseProvider Provider => DatabaseProvider.MySql;
    public DqlBuilder() : base() { }
    protected sealed override string GetSelection(Field field) => _ddlBuilder.GetPhysicalName(field);
    protected sealed override string GetSelection(Relation relation) => _ddlBuilder.GetPhysicalName(relation);

}
