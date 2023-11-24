using Ring.Schema.Enums;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    public override string VariableNameTemplate => "${0}";
}
