using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.MySQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override string VariableNameTemplate => ":a{0}";
    protected override string WrapVariable(string variable, FieldType fieldType) => variable;

	internal DmlBuilder(Encoding clientEncoding) : base(DatabaseProvider.MySql, clientEncoding) { }
}
