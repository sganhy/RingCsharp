using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.SQLServer;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.SqlServer, clientEncoding, logSql) {}
}
