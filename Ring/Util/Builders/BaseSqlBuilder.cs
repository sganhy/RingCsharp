using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Text;

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

    // clauses
    protected static readonly string SqlSelect = @"SELECT ";
    protected static readonly string SqlFrom = @" FROM ";
    protected static readonly string SqlWhere = @" WHERE ";

    public abstract DatabaseProvider Provider { get; }

    protected BaseSqlBuilder() {}


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // Mark members as static
    public void AppendFilter(int index, Field field, Operator operatorType, StringBuilder selectFrom)
#pragma warning restore CA1822 // Mark members as static
    {
        if (index == 0) selectFrom.Append(SqlWhere);
    }
        
}
