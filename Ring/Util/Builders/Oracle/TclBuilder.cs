using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.Oracle;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.Oracle, clientEncoding, logSql) {}
	protected override string BeginStatement => ":a{0}";
	protected override string CommitStatement => ":a{0}";
	protected override string RollbackStatement => ":a{0}";
	public override bool TransactionalDdl => false;
}
