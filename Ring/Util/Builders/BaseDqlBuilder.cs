using Ring.Schema.Enums;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDqlBuilder : BaseSqlBuilder, IDqlBuilder
{
    public abstract DatabaseProvider Provider { get; }

    // clauses
    protected static readonly string DqlSelect = @"SELECT ";
    protected static readonly string DqlFrom = @" FROM ";
    protected static readonly string DqlWhere = @" WHERE ";

    public void Init(DbSchema schema)
    {
        throw new NotImplementedException();
    }

}
