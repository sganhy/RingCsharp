using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.MySQL;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.MySql, clientEncoding, logSql) {}
	public override bool TransactionalDdl => true;
	protected override string BeginStatement => ":a{0}";
	protected override string CommitStatement => ":a{0}";
	protected override string RollbackStatement => ":a{0}";

}
