using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.MariaDB;

internal sealed class DmlBuilder : BaseDmlBuilder
{
	protected override string VariableNameTemplate => ":a{0}";
    protected override string WrapVariable(string variable, FieldType fieldType) => variable;
	internal DmlBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.MariaDb, clientEncoding, logSql) {}
}
