using Ring.Schema.Enums;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDqlBuilder : ISqlBuilder
{
    DatabaseProvider Provider { get; }
    void Init(DbSchema schema);
}
