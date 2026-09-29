using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.SQLServer;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    public override string VariableNameTemplate => "@";
    protected override string WrapVariable(string variable, FieldType fieldType) => variable;
	internal DmlBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.SqlServer, clientEncoding, logSql) {}
}
