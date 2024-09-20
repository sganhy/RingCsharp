using Ring.Data;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

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
    public void AppendFilter(int index, Field field, OperatorType operatorType, StringBuilder selectFrom)
#pragma warning restore CA1822 // Mark members as static
    {
        if (index == 0) selectFrom.Append(SqlWhere);
    }

    /// <summary>
    /// Get sorted list of logical table name
    /// </summary>
    protected static string[] GetTableIndex(DbSchema schema) {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        var mtmTaleDico = new HashSet<string>();
        var mtmIndex = 0;
        var result = new string[tableCount+mtmCount]; // reduce re-allocations
        for (var i = 0; i < tableCount; ++i) result[i] = schema.TablesById[i].Name;
        for (var i = 0; i < schema.TablesById.Length; ++i)
            for (var j = schema.TablesById[i].Relations.Length - 1; j >= 0; --j)
            {
                var relation = schema.TablesById[i].Relations[j];
                if (relation.Type == RelationType.Mtm && !mtmTaleDico.Contains(relation.ToTable.Name))
                {
                    result[mtmIndex + tableCount] = relation.ToTable.Name;
                    mtmTaleDico.Add(relation.ToTable.Name);
                    ++mtmIndex;
                }
            }
        Array.Sort(result, (x, y) => string.CompareOrdinal(x, y));
        return result;
    }

}
