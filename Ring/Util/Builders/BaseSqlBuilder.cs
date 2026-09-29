using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders;

/// <summary>
/// base class for non-specific RDBMS supporting SQL99-Standard
///        http://web.cecs.pdx.edu/~len/sql1999.pdf
/// </summary>
internal abstract class BaseSqlBuilder : ISqlBuilder
{
	// miscelanous
	protected const char SqlSpace = ' ';
	protected const char StartParenthesis = '(';
	protected const char EndParenthesis = ')';
	protected const char ColumnDelimiter = ',';
	protected const char SqlLineFeed = '\n';

	// clauses
	protected static readonly char SqlQuote = '\'';

	// common operators in dml, dql and ddl
	protected static readonly string SqlAnd = @" AND ";
	protected readonly Encoding _clientEncoding;
	protected readonly DatabaseProvider _provider;
	protected readonly bool _logSql;

	// properties
	public bool LogSql => _logSql;
	public DatabaseProvider Provider => _provider;
	public Encoding ClientEncoding => _clientEncoding;

	protected BaseSqlBuilder(DatabaseProvider provider, Encoding clientEncoding, bool logSql)
	{
		_provider = provider;
		_clientEncoding = clientEncoding;
		_logSql = logSql;
	}
	

}
