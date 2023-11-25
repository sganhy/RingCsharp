using Ring.Schema.Enums;
using Ring.Schema.Models;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDmlBuilder
{
    DatabaseProvider Provider { get; }
    void Init(DbSchema schema);
    string Insert(Table table, bool includeRelations);
    string Delete(Table table);
}
