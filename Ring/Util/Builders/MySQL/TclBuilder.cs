using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.MySQL;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.MySql, clientEncoding, logSql) {}

}
