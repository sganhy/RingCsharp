using Ring.Schema.Enums;
using Ring.Util.Models;
using System.Text;

namespace Ring.Util.Builders;

internal abstract class BaseTclBuilder : BaseSqlBuilder, ITclBuilder
{
	private readonly SqlEntry _begin;
	private readonly SqlEntry _commit;
	private readonly SqlEntry _rollback;

	public abstract bool TransactionalDdl { get; }
	protected abstract string BeginStatement { get; }
	protected abstract string CommitStatement { get; }
	protected abstract string RollbackStatement { get; }

	protected BaseTclBuilder(DatabaseProvider provider, Encoding clientEncoding, bool logSql) : base(provider, clientEncoding, logSql)
	{
		_begin = new SqlEntry(BeginStatement, _clientEncoding.GetBytes(BeginStatement));
		_commit = new SqlEntry(CommitStatement, _clientEncoding.GetBytes(CommitStatement));
		_rollback = new SqlEntry(RollbackStatement, _clientEncoding.GetBytes(RollbackStatement));
	}

	public SqlEntry Commit => _commit; // Code size: 7 (0x7)
	public SqlEntry Rollback => _rollback; // Code size: 7 (0x7)
	public SqlEntry StartTransaction => _begin; // Code size: 7 (0x7)
}
