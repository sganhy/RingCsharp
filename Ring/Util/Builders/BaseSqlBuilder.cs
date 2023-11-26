using Ring.Schema.Enums;
using Ring.Schema.Extensions;
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

    public abstract DatabaseProvider Provider { get; }

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
        Array.Sort(result);
        return result;
    }

}
