using Ring.Schema.Models;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDmlBuilder : ISqlBuilder
{
    void Init(DbSchema schema, string[] tableIndex); 
    string Insert(Table table);
    string Delete(Table table);
    string Update(Table table);
}
