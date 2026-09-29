using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.Oracle;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.Oracle, clientEncoding, logSql) {}
}
