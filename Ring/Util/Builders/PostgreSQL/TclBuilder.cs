using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.PostgreSql, clientEncoding, logSql) {}
}
