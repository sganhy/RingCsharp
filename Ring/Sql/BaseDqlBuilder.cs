namespace Ring.Sql;

internal class BaseDqlBuilder : BaseSqlBuilder, IDqlBuilder
{
    // clauses
    protected static readonly string DqlSelect = @"SELECT ";
    protected static readonly string DqlFrom = @" FROM ";
    protected static readonly string DqlWhere = @" WHERE ";



}
