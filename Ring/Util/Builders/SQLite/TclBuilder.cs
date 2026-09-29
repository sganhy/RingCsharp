using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.SQLite;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.SqlLite, clientEncoding, logSql) {}
}
