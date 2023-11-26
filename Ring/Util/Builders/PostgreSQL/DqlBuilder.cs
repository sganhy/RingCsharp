using Ring.Schema.Enums;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    public DqlBuilder() : base() { }

}
