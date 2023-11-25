using Ring.Schema.Models;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDqlBuilder : ISqlBuilder
{
    void Init(DbSchema schema);
    string Select(Table table, bool includeRelations);
}
