using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.MariaDB;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding) : base(DatabaseProvider.MariaDb, clientEncoding) {}
}
