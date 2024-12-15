using Ring.Schema.Enums;

namespace Ring.Util.Builders.SQLite;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.SqlLite;
    public override string VariableNameTemplate => "$";
    protected override string WrapVariable(string variable, FieldType fieldType) => variable;

}
