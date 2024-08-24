using Ring.Schema.Enums;

namespace Ring.Util.Builders.Static.Oracle;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.Oracle;
    public override string VariableNameTemplate => ":a{0}";
    public DmlBuilder() : base() { }

}
