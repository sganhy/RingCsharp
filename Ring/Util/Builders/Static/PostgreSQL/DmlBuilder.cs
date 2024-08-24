using Ring.Schema.Enums;

namespace Ring.Util.Builders.Static.PostgreSQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    public override string VariableNameTemplate => "${0}";
    public DmlBuilder() : base() { }
}
