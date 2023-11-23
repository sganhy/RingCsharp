namespace Ring.Sql;

/// <summary>
/// base class for non-specific RDBMS supporting SQL99-Standard
///     http://web.cecs.pdx.edu/~len/sql1999.pdf
/// </summary>
internal abstract class BaseSqlBuilder : ISqlBuilder
{
    // miscelanous
    protected static readonly char SqlSpace = ' ';
    protected static readonly char SqlLineFeed = '\n';
}
