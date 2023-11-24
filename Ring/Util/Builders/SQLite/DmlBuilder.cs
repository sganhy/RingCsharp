using Ring.Schema.Enums;

namespace Ring.Util.Builders.SQLite;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.SqlLite;
    public override string VariableNameTemplate => "$";
}
