using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.SQLite;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding) : base(DatabaseProvider.SqlLite, clientEncoding) { }

}
