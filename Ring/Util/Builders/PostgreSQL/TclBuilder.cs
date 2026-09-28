using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding) : base(DatabaseProvider.PostgreSql, clientEncoding) { }
}
