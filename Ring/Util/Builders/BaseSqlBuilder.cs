namespace Ring.Util.Builders;

/// <summary>
/// base class for non-specific RDBMS supporting SQL99-Standard
///     http://web.cecs.pdx.edu/~len/sql1999.pdf
/// </summary>
internal abstract class BaseSqlBuilder : ISqlBuilder
{
    // miscelanous
    protected const char SqlSpace = ' ';
    protected const char StartParenthesis = '(';
    protected const char EndParenthesis = ')';
    protected const char ColumnDelimiter = ',';
    protected const char SqlLineFeed = '\n';
}
