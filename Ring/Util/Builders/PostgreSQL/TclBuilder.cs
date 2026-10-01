using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class TclBuilder : BaseTclBuilder
{
	internal TclBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.PostgreSql, clientEncoding, logSql) {}

	/*
	BEGIN;
	UPDATE accounts SET balance = balance - 100.00 WHERE acctnum = 7534;
	UPDATE accounts SET balance = balance + 100.00 WHERE acctnum = 12345;
	COMMIT;
	*/

	public override bool TransactionalDdl => true;
	protected override string BeginStatement => "BEGIN;";
	protected override string CommitStatement => "COMMIT;";
	protected override string RollbackStatement => "ROLLBACK;";
}
