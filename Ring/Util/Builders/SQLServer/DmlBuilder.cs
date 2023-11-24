using Ring.Schema.Enums;

namespace Ring.Util.Builders.SQLServer;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.SqlServer;
    public override string VariableNameTemplate => "@";
}
