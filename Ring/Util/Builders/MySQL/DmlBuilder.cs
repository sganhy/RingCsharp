using Ring.Schema.Enums;

namespace Ring.Util.Builders.MySQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.MySql;
    public override string VariableNameTemplate => ":a{0}";
    public DmlBuilder() : base() { }

}
